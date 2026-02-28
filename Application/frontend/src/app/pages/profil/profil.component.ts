import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject, OnInit, OnDestroy, NgZone } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { UserRole } from '../../shared/enums/user-role.enum';
import {
  JobService,
  type JobListItem,
} from '../../shared/services/job.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types/button.type';
import { SIGNALR_STATUS } from '../../shared/types';
import { NewJobRequestPayload } from '../../shared/interfaces';
import { signal, computed } from '@angular/core';

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
  private router = inject(Router);

  user$ = this.auth.userSelector$;
  public userRole = UserRole;
  public eButtonType = BUTTON_TYPES;

  /** Svi poslovi za korisnika (jedan API poziv). */
  myJobs = signal<JobListItem[]>([]);
  loadingMyJobs = signal(false);
  requestError = signal<string | null>(null);
  actingRequestId = signal<string | null>(null);

  /** Za majstora: poslovi sa statusom Pending (zahtevi na čekanju). */
  pendingRequests = computed(() =>
    this.myJobs().filter((j) => j.status === 'Pending')
  );
  /** Za majstora: poslovi koji nisu Pending (dodeljeni). */
  assignedJobs = computed(() =>
    this.myJobs().filter((j) => j.status !== 'Pending')
  );

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
        void this.loadJobs();
      } else if (user?.role === UserRole.Client) {
        void this.loadJobs();
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
    const item: JobListItem = {
      jobId,
      conversationId,
      jobTitle,
      description,
      clientName,
      masterName: null,
      date,
      clientId,
      price,
      isEmergency,
      status: 'Pending',
      createdAt: now,
      updatedAt: now,
    };
    this.myJobs.update((list) => {
      if (list.some((r) => r.conversationId === conversationId)) return list;
      return [...list, item];
    });
  }

  async loadJobs(): Promise<void> {
    this.loadingMyJobs.set(true);
    this.requestError.set(null);
    try {
      const list = await this.jobService.getJobs();
      this.myJobs.set(list);
    } catch (err: unknown) {
      const msg =
        (err as { error?: { message?: string } })?.error?.message ??
        (err as Error)?.message ??
        'Nije moguće učitati poslove.';
      this.requestError.set(msg);
      this.myJobs.set([]);
    } finally {
      this.loadingMyJobs.set(false);
    }
  }

  statusLabel(status: string): string {
    const labels: Record<string, string> = {
      Created: 'Kreiran',
      Pending: 'Na čekanju',
      Accepted: 'Prihvaćen',
      InProgress: 'U toku',
      Completed: 'Završen',
    };
    return labels[status] ?? status;
  }

  async acceptRequest(item: JobListItem): Promise<void> {
    if (this.actingRequestId()) return;
    this.actingRequestId.set(item.jobId);
    this.requestError.set(null);
    try {
      await this.jobService.acceptJob(item.jobId);
      await this.loadJobs();
    } catch {
      this.requestError.set('Nije moguće prihvatiti posao.');
    } finally {
      this.actingRequestId.set(null);
    }
  }

  async declineRequest(item: JobListItem): Promise<void> {
    if (this.actingRequestId()) return;
    this.actingRequestId.set(item.conversationId);
    this.requestError.set(null);
    try {
      await this.jobService.declineRequest(item.conversationId);
      this.myJobs.update((list) =>
        list.filter((r) => r.conversationId !== item.conversationId)
      );
    } catch {
      this.requestError.set('Nije moguće odbiti zahtev.');
    } finally {
      this.actingRequestId.set(null);
    }
  }

  navigateToMasters(): void {
    this.router.navigate(['/masters']);
  }
}
