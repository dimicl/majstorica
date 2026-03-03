import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject, OnInit, OnDestroy, NgZone } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { UserRole } from '../../shared/enums/user-role.enum';
import {
  JobService,
  type JobListItem,
} from '../../shared/services/job.service';
import { MasterService } from '../../shared/services/master.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types/button.type';
import { SIGNALR_STATUS } from '../../shared/types';
import { NewJobRequestPayload } from '../../shared/interfaces';
import { MASTER_CATEGORY_OPTIONS } from '../../shared/enums';
import { FormsModule } from '@angular/forms';
import { signal, computed } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SvgIconComponent } from 'angular-svg-icon';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { JobEditModalComponent } from '../../components/job-edit-modal/job-edit-modal.component';

const HUB_URL = 'http://localhost:5187/hubs/document';

@Component({
  selector: 'app-profil',
  imports: [
    AsyncPipe,
    DatePipe,
    RouterLink,
    ButtonComponent,
    FormsModule,
    SvgIconComponent,
    JobEditModalComponent,
  ],
  templateUrl: './profil.component.html',
  styleUrl: './profil.component.scss',
})
export class ProfilComponent implements OnInit, OnDestroy {
  private auth = inject(AuthSelectorService);
  private jobService = inject(JobService);
  private masterService = inject(MasterService);
  private signalr = inject(SignalrService);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);
  private router = inject(Router);

  user$ = this.auth.userSelector$;
  public userRole = UserRole;
  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;

  masterProfile = signal<{
    category: string | null;
    rating: number | null;
  } | null>(null);
  masterProfileLoading = signal(false);
  masterCategorySaving = signal(false);
  masterCategoryError = signal<string | null>(null);
  /** Opcije za dropdown kategorije (iz enum-a). */
  public categoryOptions = MASTER_CATEGORY_OPTIONS.map((o) => ({
    value: o.label,
    label: o.label,
  }));

  myJobs = signal<JobListItem[]>([]);
  loadingMyJobs = signal(false);
  requestError = signal<string | null>(null);
  actingRequestId = signal<string | null>(null);
  jobEditModal = signal<JobListItem | null>(null);
  /** Toast kada Edit nije moguć (drugi uređuje) – samo poruka, bez modala. */
  editLockToast = signal<string | null>(null);

  pendingRequests = computed(() =>
    this.myJobs().filter((j) => j.status === 'Pending')
  );

  assignedJobs = computed(() =>
    this.myJobs().filter((j) => j.status !== 'Pending')
  );

  private newJobRequestHandlerRegistered = false;
  private editLockToastTimer: ReturnType<typeof setTimeout> | null = null;

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
        void this.loadMasterProfile();
      } else if (user?.role === UserRole.Client) {
        void this.loadJobs();
      } else {
        this.masterProfile.set(null);
      }
    });
  }

  async loadMasterProfile(): Promise<void> {
    this.masterProfileLoading.set(true);
    this.masterCategoryError.set(null);
    try {
      const res = await firstValueFrom(this.masterService.getMyMasterProfile());
      this.masterProfile.set({
        category: res.category ?? null,
        rating: res.rating ?? null,
      });
    } catch {
      this.masterProfile.set(null);
    } finally {
      this.masterProfileLoading.set(false);
    }
  }

  async setMasterCategory(category: string): Promise<void> {
    if (this.masterCategorySaving()) return;
    this.masterCategorySaving.set(true);
    this.masterCategoryError.set(null);
    try {
      await firstValueFrom(
        this.masterService.updateMyCategory(category || null)
      );
      this.masterProfile.update((p) =>
        p
          ? { ...p, category: category || null }
          : { category: category || null, rating: null }
      );
    } catch (err: unknown) {
      const msg =
        (err as { error?: { message?: string } })?.error?.message ??
        'Nije moguće sačuvati kategoriju.';
      this.masterCategoryError.set(msg);
    } finally {
      this.masterCategorySaving.set(false);
    }
  }

  ngOnDestroy(): void {
    if (this.editLockToastTimer) clearTimeout(this.editLockToastTimer);
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

  openJobEdit(job: JobListItem): void {
    this.ensureSignalR();
    const jobId = job.jobId.toLowerCase();
    const myUserId = (this.authService.getUserIdFromStorage() ?? '').toLowerCase();

    const onGranted = (...args: unknown[]) => {
      this.ngZone.run(() => {
        const id = args[0] != null ? String(args[0]).toLowerCase() : '';
        if (id !== jobId) return;
        const nextUserId = args.length > 1 && args[1] != null ? String(args[1]).toLowerCase() : undefined;
        const isForMe = nextUserId === undefined || nextUserId === myUserId;
        if (!isForMe) return;
        this.signalr.off('WriteGranted', onGranted as (...a: unknown[]) => void);
        this.signalr.off('WriteDenied', onDenied as (...a: unknown[]) => void);
        this.editLockToast.set(null);
        if (this.editLockToastTimer) {
          clearTimeout(this.editLockToastTimer);
          this.editLockToastTimer = null;
        }
        this.jobEditModal.set(job);
      });
    };

    const onDenied = (...args: unknown[]) => {
      this.ngZone.run(() => {
        const id = args[0] != null ? String(args[0]).toLowerCase() : '';
        if (id !== jobId) return;
        this.signalr.off('WriteDenied', onDenied as (...a: unknown[]) => void);
        this.editLockToast.set('Trenutno ne možete uređivati.');
        if (this.editLockToastTimer) clearTimeout(this.editLockToastTimer);
        this.editLockToastTimer = setTimeout(() => {
          this.editLockToast.set(null);
          this.editLockToastTimer = null;
        }, 4000);
      });
    };

    this.signalr.on('WriteGranted', onGranted as (p: unknown) => void);
    this.signalr.on('WriteDenied', onDenied as (p: unknown) => void);
    this.signalr.invoke('JoinJob', job.jobId).catch(() => {
      this.ngZone.run(() => {
        this.signalr.off('WriteGranted', onGranted as (...a: unknown[]) => void);
        this.signalr.off('WriteDenied', onDenied as (...a: unknown[]) => void);
        this.editLockToast.set('Trenutno ne možete uređivati.');
        if (this.editLockToastTimer) clearTimeout(this.editLockToastTimer);
        this.editLockToastTimer = setTimeout(() => {
          this.editLockToast.set(null);
          this.editLockToastTimer = null;
        }, 4000);
      });
    });
  }

  closeJobEdit(): void {
    const job = this.jobEditModal();
    if (job) {
      this.signalr.invoke('LeaveJob', job.jobId).catch(() => {});
    }
    this.jobEditModal.set(null);
  }
}
