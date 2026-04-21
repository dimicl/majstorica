import { AsyncPipe, DatePipe } from '@angular/common';
import { Component, inject, OnInit, OnDestroy, NgZone } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { selectUser } from '../../shared/store/auth/auth.selectors';
import { UserRole } from '../../shared/enums/user-role.enum';
import {
  isClientUserRole,
  isMasterLikeUserRole,
} from '../../shared/utils/user-role.util';
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
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { Subscription } from 'rxjs';
import { SvgIconComponent } from 'angular-svg-icon';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { JobEditModalComponent } from '../../components/job-edit-modal/job-edit-modal.component';
import { AddressDisplayPipe } from '../../shared/pipes/address-display.pipe';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { AvatarComponent } from '../../components/avatar/avatar.component';
import { CreateJobModalComponent } from '../../components/create-job-modal/create-job-modal.component';

const HUB_URL = 'http://localhost:5187/hubs/document';

@Component({
  selector: 'app-profil',
  imports: [
    NgbTooltipModule,
    AsyncPipe,
    DatePipe,
    RouterLink,
    ButtonComponent,
    AvatarComponent,
    FormsModule,
    SvgIconComponent,
    JobEditModalComponent,
    CreateJobModalComponent,
    AddressDisplayPipe,
  ],
  templateUrl: './profil.component.html',
  styleUrl: './profil.component.scss',
})
export class ProfilComponent implements OnInit, OnDestroy {
  private store = inject(Store);
  private auth = inject(AuthSelectorService);
  private jobService = inject(JobService);
  private masterService = inject(MasterService);
  private signalr = inject(SignalrService);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);
  private router = inject(Router);

  user$ = this.auth.userSelector$;
  public userRole = UserRole;

  /** Za šablon: majstor ili company worker – isti UI kao Master. */
  masterLikeProfile(role: unknown): boolean {
    return isMasterLikeUserRole(role);
  }

  clientProfile(role: unknown): boolean {
    return isClientUserRole(role);
  }
  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;

  masterProfile = signal<{
    category: string | null;
    rating: number | null;
  } | null>(null);
  masterProfileLoading = signal(false);
  masterCategorySaving = signal(false);
  masterCategoryError = signal<string | null>(null);
  readonly masterCategoryOptions = MASTER_CATEGORY_OPTIONS;

  myJobs = signal<JobListItem[]>([]);
  loadingMyJobs = signal(false);
  requestError = signal<string | null>(null);
  actingRequestId = signal<string | null>(null);
  jobEditModal = signal<JobListItem | null>(null);
  /** Toast kada Edit nije moguć (drugi uređuje) – samo poruka, bez modala. */
  editLockToast = signal<string | null>(null);
  showCreateJobModal = signal(false);

  pendingRequests = computed(() =>
    this.myJobs().filter((j) => j.status === 'Pending')
  );

  assignedJobs = computed(() =>
    this.myJobs().filter((j) => j.status !== 'Pending')
  );

  private newJobRequestHandlerRegistered = false;
  private editLockToastTimer: ReturnType<typeof setTimeout> | null = null;
  private userContextSub?: Subscription;

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
    // Samo promena korisnika/uloge — ne i svaki patchUser({ category }) (inače petlja: profile → patch → emit → load…).
    this.userContextSub = this.auth.userSelector$
      .pipe(
        map((u) => (u ? { id: u.id, role: u.role } : null)),
        distinctUntilChanged((a, b) => a?.id === b?.id && a?.role === b?.role)
      )
      .subscribe((ctx) => {
        if (!ctx) return;
        if (isMasterLikeUserRole(ctx.role)) {
          this.ensureSignalR();
          void this.loadJobs();
          this.loadMasterProfile();
        } else if (isClientUserRole(ctx.role)) {
          void this.loadJobs();
        } else {
          this.masterProfile.set(null);
          this.auth.dispatchPatchUser({ category: null });
        }
      });
  }

  loadMasterProfile(): void {
    this.masterProfileLoading.set(true);
    this.masterCategoryError.set(null);
    this.masterService.getMyMasterProfile().subscribe({
      next: (res) => {
        this.masterProfile.set({
          category: res.category ?? null,
          rating: res.rating ?? null,
        });
        this.auth.dispatchPatchUser({ category: res.category ?? null });
        this.masterProfileLoading.set(false);
      },
      error: () => {
        this.masterProfile.set(null);
        this.auth.dispatchPatchUser({ category: null });
        this.masterProfileLoading.set(false);
      },
    });
  }

  /** PATCH kategorije, zatim ponovo učitavanje profila sa servera. */
  updateCategory(value: string): void {
    if (this.masterProfile()?.category?.trim()) return;
    const category = (value ?? '').trim();
    if (!category || this.masterCategorySaving()) return;
    if (category === this.masterProfile()?.category) return;

    this.masterCategorySaving.set(true);
    this.masterCategoryError.set(null);

    this.masterService
      .updateCategory(category)
      .pipe(switchMap(() => this.masterService.getMyMasterProfile()))
      .subscribe({
        next: (res) => {
          this.masterProfile.set({
            category: res.category ?? null,
            rating: res.rating ?? null,
          });
          this.auth.dispatchPatchUser({ category: res.category ?? null });
          this.masterCategorySaving.set(false);
        },
        error: () => {
          this.masterCategoryError.set('Nije moguće sačuvati kategoriju.');
          this.masterCategorySaving.set(false);
        },
      });
  }

  ngOnDestroy(): void {
    this.userContextSub?.unsubscribe();
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
      serviceCategory: p.serviceCategory ?? null,
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
    const myUserId = (
      this.authService.getUserIdFromStorage() ?? ''
    ).toLowerCase();

    const onGranted = (...args: unknown[]) => {
      this.ngZone.run(() => {
        const id = args[0] != null ? String(args[0]).toLowerCase() : '';
        if (id !== jobId) return;
        const nextUserId =
          args.length > 1 && args[1] != null
            ? String(args[1]).toLowerCase()
            : undefined;
        const isForMe = nextUserId === undefined || nextUserId === myUserId;
        if (!isForMe) return;
        this.signalr.off(
          'WriteGranted',
          onGranted as (...a: unknown[]) => void
        );
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
        this.signalr.off(
          'WriteGranted',
          onGranted as (...a: unknown[]) => void
        );
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

  public onOpenCreateJobModal(): void {
    this.showCreateJobModal.set(true);
  }

  public closeCreateJobModal(): void {
    this.showCreateJobModal.set(false);
  }

  public onJobCreated(): void {
    this.closeCreateJobModal();
    this.loadJobs();
  }
}
