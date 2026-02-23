import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject, OnInit, OnDestroy, NgZone } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { UserRole } from '../../shared/enums/user-role.enum';
import {
  JobService,
  type JobRequestItem,
} from '../../shared/services/job.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types/button.type';
import { SIGNALR_STATUS } from '../../shared/types';
import { NewJobRequestPayload } from '../../shared/interfaces';
import { signal } from '@angular/core';

const HUB_URL = 'http://localhost:5187/hubs/document';

@Component({
  selector: 'app-profil',
  imports: [AsyncPipe, DatePipe, RouterLink, ButtonComponent],
  templateUrl: './profil.component.html',
  styleUrl: './profil.component.scss',
})
export class ProfilComponent implements OnInit, OnDestroy {
  private auth = inject(AuthSelectorService);
  private jobService = inject(JobService);
  private signalr = inject(SignalrService);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);

  user$ = this.auth.userSelector$;
  readonly userRole = UserRole;
  public eButtonType = BUTTON_TYPES;

  /** Zahtevi za majstora (samo kada je uloga Master). */
  requests = signal<JobRequestItem[]>([]);
  loadingRequests = signal(false);
  requestError = signal<string | null>(null);
  /** Zaštićeno polje za akciju (da ne dupliramo klik). */
  actingRequestId = signal<string | null>(null);

  private newJobRequestHandlerRegistered = false;

  private ensureSignalR(): void {
    const token = this.authService.getToken();
    if (token && this.signalr.status() !== SIGNALR_STATUS.CONNECTED) {
      void this.signalr.connect(HUB_URL, {
        accessTokenFactory: () => this.authService.getToken() ?? '',
      });
    }
  }

  ngOnInit(): void {
    if (!this.newJobRequestHandlerRegistered) {
      this.newJobRequestHandlerRegistered = true;
      this.signalr.on<NewJobRequestPayload>('NewJobRequest', (payload) => {
        this.ngZone.run(() => this.addRequestFromPayload(payload));
      });
    }
    this.auth.userSelector$.subscribe((user) => {
      if (user?.role === UserRole.Master) {
        this.ensureSignalR();
        void this.loadRequests();
      }
    });
  }

  ngOnDestroy(): void {
    // Handler ostaje; SignalR se ne disconnect-uje (korisnik je i dalje ulogovan).
  }

  private addRequestFromPayload(p: NewJobRequestPayload): void {
    const jobId = p.jobId ?? '';
    const conversationId = p.conversationId ?? '';
    const jobTitle = p.jobTitle ?? '';
    const description = p.description ?? '';
    const date = p.date ?? new Date().toISOString();
    const clientName = p.clientName ?? 'Klijent';
    const clientId = p.clientId ?? '';
    const price = p.price ?? null;
    const isEmergency = p.isEmergency ?? false;
    if (!jobId || !conversationId) return;
    const now = new Date().toISOString();
    const item: JobRequestItem = {
      jobId,
      conversationId,
      jobTitle,
      description,
      clientName,
      clientId,
      date,
      price,
      isEmergency,
      createdAt: now,
      updatedAt: now,
    };
    this.requests.update((list) => {
      if (list.some((r) => r.conversationId === conversationId)) return list;
      return [...list, item];
    });
  }

  async loadRequests(): Promise<void> {
    this.loadingRequests.set(true);
    this.requestError.set(null);
    try {
      const list = await this.jobService.getPendingRequests();
      this.requests.set(list);
    } catch (err: unknown) {
      const msg =
        (err as { error?: { message?: string } })?.error?.message ??
        (err as Error)?.message ??
        'Nije moguće učitati zahteve.';
      this.requestError.set(msg);
      this.requests.set([]);
    } finally {
      this.loadingRequests.set(false);
    }
  }

  async acceptRequest(item: JobRequestItem): Promise<void> {
    if (this.actingRequestId()) return;
    this.actingRequestId.set(item.jobId);
    this.requestError.set(null);
    try {
      await this.jobService.acceptJob(item.jobId);
      this.requests.update((list) =>
        list.filter((r) => r.jobId !== item.jobId)
      );
    } catch {
      this.requestError.set('Nije moguće prihvatiti posao.');
    } finally {
      this.actingRequestId.set(null);
    }
  }

  async declineRequest(item: JobRequestItem): Promise<void> {
    if (this.actingRequestId()) return;
    this.actingRequestId.set(item.conversationId);
    this.requestError.set(null);
    try {
      await this.jobService.declineRequest(item.conversationId);
      this.requests.update((list) =>
        list.filter((r) => r.conversationId !== item.conversationId)
      );
    } catch {
      this.requestError.set('Nije moguće odbiti zahtev.');
    } finally {
      this.actingRequestId.set(null);
    }
  }
}
