import {
  AsyncPipe,
  DatePipe,
  DecimalPipe,
  NgTemplateOutlet,
} from '@angular/common';
import { HttpErrorResponse, HttpStatusCode } from '@angular/common/http';
import {
  Component,
  HostListener,
  inject,
  OnInit,
  OnDestroy,
  NgZone,
  signal,
  computed,
} from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom, Subscription } from 'rxjs';
import { distinctUntilChanged, map, switchMap } from 'rxjs/operators';
import { FormsModule } from '@angular/forms';
import { SvgIconComponent } from 'angular-svg-icon';
import { NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';

import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { CompanyService } from '../../shared/services/company.service';
import type { User } from '../../shared/models/user.model';
import { UserRole } from '../../shared/enums/user-role.enum';
import {
  JobService,
  type JobListItem,
} from '../../shared/services/job.service';
import { MasterService } from '../../shared/services/master.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES, type ButtonType } from '../../shared/types/button.type';
import { SIGNALR_STATUS } from '../../shared/types';
import { NewJobRequestPayload } from '../../shared/interfaces';
import { MASTER_CATEGORY_OPTIONS } from '../../shared/enums';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { JobEditModalComponent } from '../../components/job-edit-modal/job-edit-modal.component';
import { AddressDisplayPipe } from '../../shared/pipes/address-display.pipe';
import { AvatarComponent } from '../../components/avatar/avatar.component';
import { InputComponent } from '../../components/input/input.component';
import { CreateJobModalComponent } from '../../components/create-job-modal/create-job-modal.component';
import type {
  CompanyDto,
  CompanyWorkerMember,
  MasterSearchForInviteItem,
} from '../../shared/interfaces/company.interface';
import type {
  MasterProfileResponse,
  MasterReviewListItem,
} from '../../shared/models/master.model';

import {
  AUTH_PATCH_CLEARED,
  authPatchFromMasterProfile,
  buildPendingJobFromNewJobRequest,
  jobStatusLabel,
  masterProfileVmFromResponse,
  normalizeMasterUserId,
  PROFIL_SIGNALR_HUB_URL,
  runProfileRoleActions,
  type MasterStatKind,
  workerCategoriesLine as workerCategoriesLineHelper,
  workerZonesLine as workerZonesLineHelper,
} from './helpers/profil.helper';

@Component({
  selector: 'app-profil',
  providers: [AddressDisplayPipe],
  imports: [
    NgbTooltipModule,
    AsyncPipe,
    DatePipe,
    DecimalPipe,
    NgTemplateOutlet,
    ButtonComponent,
    AvatarComponent,
    InputComponent,
    FormsModule,
    SvgIconComponent,
    JobEditModalComponent,
    CreateJobModalComponent,
  ],
  templateUrl: './profil.component.html',
  styleUrl: './profil.component.scss',
})
export class ProfilComponent implements OnInit, OnDestroy {
  // --- private fields ---
  private auth = inject(AuthSelectorService);
  private jobService = inject(JobService);
  private masterService = inject(MasterService);
  private companyService = inject(CompanyService);
  private signalr = inject(SignalrService);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);
  private router = inject(Router);
  private addressDisplay = inject(AddressDisplayPipe);

  private masterSearchDebounce: ReturnType<typeof setTimeout> | null = null;
  private newJobRequestHandlerRegistered = false;
  private editLockToastTimer: ReturnType<typeof setTimeout> | null = null;
  private userContextSub?: Subscription;

  // --- public fields (template) ---
  public user$ = this.auth.userSelector$;
  public userRole = UserRole;
  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;

  public masterProfile = signal<{
    category: string | null;
    rating: number | null;
    yearsOfExperience: number;
    hourlyRateAmount: number;
    hourlyRateCurrency: string;
    totalReviews: number;
  } | null>(null);
  public masterProfileLoading = signal(false);
  public masterCategorySaving = signal(false);
  public masterCategoryError = signal<string | null>(null);
  public editingMasterStat = signal<MasterStatKind | null>(null);
  public experienceDraft = signal('');
  public hourlyDraft = signal('');
  public masterStatsSaving = signal(false);
  public masterStatsError = signal<string | null>(null);
  public masterReviews = signal<MasterReviewListItem[]>([]);
  public loadingMasterReviews = signal(false);
  public masterReviewsError = signal<string | null>(null);
  public readonly masterCategoryOptions = MASTER_CATEGORY_OPTIONS;

  /** Redovi kontakta u levoj kartici (ikonica + tekst). */
  public readonly contactRowDefs = [
    { key: 'phone' as const, icon: SharedSvgRoutes.PHONE_ICON },
    { key: 'address' as const, icon: SharedSvgRoutes.ADDRESS_ICON },
    { key: 'email' as const, icon: SharedSvgRoutes.EMAIL_ICON },
  ];

  public myJobs = signal<JobListItem[]>([]);
  public loadingMyJobs = signal(false);
  public requestError = signal<string | null>(null);
  public actingRequestId = signal<string | null>(null);
  public jobEditModal = signal<JobListItem | null>(null);
  /** Toast kada Edit nije moguć (drugi uređuje) – samo poruka, bez modala. */
  public editLockToast = signal<string | null>(null);
  public showCreateJobModal = signal(false);

  /** Vlasnik firme: podaci o firmi i pozivi majstora. */
  public companyTeam = signal<CompanyDto | null | undefined>(undefined);
  public masterSearchResults = signal<MasterSearchForInviteItem[]>([]);
  public masterInviteLoading = signal(false);
  public masterInviteError = signal<string | null>(null);
  public invitingMasterId = signal<string | null>(null);
  /** Majstori kojima je u ovoj sesiji uspešno poslat poziv (dugme ostaje „Poslato”). */
  public invitedMasterIds = signal<ReadonlySet<string>>(new Set());
  /** Pending pozivnice sa servera (ostaje posle refresh-a). */
  public pendingOutboundInviteMasterIds = signal<ReadonlySet<string>>(
    new Set()
  );
  public companyMembers = signal<CompanyWorkerMember[]>([]);
  public loadingCompanyMembers = signal(false);

  public pendingRequests = computed(() =>
    this.myJobs().filter((j) => j.status === 'Pending')
  );

  public assignedJobs = computed(() =>
    this.myJobs().filter((j) => j.status !== 'Pending')
  );

  // --- host listeners ---
  @HostListener('document:keydown.escape')
  public onMasterStatEscape(): void {
    if (this.editingMasterStat()) {
      this.editingMasterStat.set(null);
      this.masterStatsError.set(null);
    }
  }

  // --- private methods ---
  private initProfilePage(): void {
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
        runProfileRoleActions(ctx?.role, {
          onMaster: () => {
            this.ensureSignalR();
            void this.loadJobs();
            this.loadMasterProfile();
            this.loadMasterReviews();
          },
          onCompanyWorker: () => {
            this.ensureSignalR();
            void this.loadJobs();
            this.loadMasterProfile();
            this.loadMasterReviews();
          },
          onClient: () => {
            void this.loadJobs();
          },
          onCompanyOwner: () => {
            this.clearMasterState();
            void this.loadCompanyTeam();
          },
          onDefault: () => {
            this.clearMasterState();
            this.clearCompanyState();
          },
        });
      });
  }

  private ensureSignalR(): void {
    const token = this.authService.getToken();
    if (token && this.signalr.status() !== SIGNALR_STATUS.CONNECTED) {
      void this.signalr.connect(PROFIL_SIGNALR_HUB_URL, {
        accessTokenFactory: () => this.authService.getToken() ?? '',
      });
    }
  }

  private applyMasterProfileResponse(res: MasterProfileResponse): void {
    this.masterProfile.set(masterProfileVmFromResponse(res));
  }

  private patchAuthFromMasterProfile(res: MasterProfileResponse): void {
    this.auth.dispatchPatchUser(authPatchFromMasterProfile(res));
  }

  private clearMasterState(): void {
    this.masterProfile.set(null);
    this.masterReviews.set([]);
    this.masterReviewsError.set(null);
    this.auth.dispatchPatchUser(AUTH_PATCH_CLEARED);
  }

  private clearCompanyState(): void {
    this.companyTeam.set(undefined);
    this.masterSearchResults.set([]);
    this.companyMembers.set([]);
    this.invitedMasterIds.set(new Set());
    this.pendingOutboundInviteMasterIds.set(new Set());
  }

  private saveMasterStatError(message: string): void {
    this.masterStatsError.set(message);
    this.masterStatsSaving.set(false);
  }

  private applyMasterStatSuccess(res: MasterProfileResponse): void {
    this.applyMasterProfileResponse(res);
    this.editingMasterStat.set(null);
    this.masterStatsSaving.set(false);
  }

  private addRequestFromPayload(p: NewJobRequestPayload): void {
    const item = buildPendingJobFromNewJobRequest(p);
    if (!item) return;
    const conversationId = item.conversationId;
    this.myJobs.update((list) => {
      if (list.some((r) => r.conversationId === conversationId)) return list;
      return [...list, item];
    });
  }

  private async loadPendingOutboundInvites(): Promise<void> {
    try {
      const ids = await firstValueFrom(
        this.companyService.getPendingOutboundInviteRecipients()
      );
      this.pendingOutboundInviteMasterIds.set(
        new Set((ids ?? []).map((x) => normalizeMasterUserId(x)))
      );
    } catch {
      this.pendingOutboundInviteMasterIds.set(new Set());
    }
  }

  private async runMasterSearch(q: string): Promise<void> {
    this.masterInviteLoading.set(true);
    this.masterInviteError.set(null);
    try {
      const listPromise = firstValueFrom(
        this.companyService.searchMastersForInvite(q)
      );
      await this.loadPendingOutboundInvites();
      this.masterSearchResults.set(await listPromise);
    } catch {
      this.masterSearchResults.set([]);
      this.masterInviteError.set('Pretraga nije uspela.');
    } finally {
      this.masterInviteLoading.set(false);
    }
  }

  private clearEditLockToast(): void {
    this.editLockToast.set(null);
    if (!this.editLockToastTimer) return;
    clearTimeout(this.editLockToastTimer);
    this.editLockToastTimer = null;
  }

  private showEditLockToast(): void {
    this.editLockToast.set('Trenutno ne možete uređivati.');
    if (this.editLockToastTimer) clearTimeout(this.editLockToastTimer);
    this.editLockToastTimer = setTimeout(() => this.clearEditLockToast(), 4000);
  }

  // --- public methods ---
  public ngOnInit(): void {
    this.initProfilePage();
  }

  public ngOnDestroy(): void {
    this.userContextSub?.unsubscribe();
    if (this.editLockToastTimer) clearTimeout(this.editLockToastTimer);
  }

  public profileContactText(
    user: User,
    key: 'phone' | 'address' | 'email'
  ): string {
    switch (key) {
      case 'phone':
        return user.phone?.trim() || 'No Phone';
      case 'address':
        return this.addressDisplay.transform(user.address);
      case 'email':
        return user.email?.trim() ?? 'No Email';
    }
  }

  public loadMasterReviews(): void {
    this.loadingMasterReviews.set(true);
    this.masterReviewsError.set(null);
    this.masterService.getMyMasterReviews().subscribe({
      next: (list) => {
        this.masterReviews.set(list ?? []);
        this.loadingMasterReviews.set(false);
      },
      error: () => {
        this.masterReviews.set([]);
        this.masterReviewsError.set('Nije moguće učitati recenzije.');
        this.loadingMasterReviews.set(false);
      },
    });
  }

  public loadMasterProfile(): void {
    this.masterProfileLoading.set(true);
    this.masterCategoryError.set(null);
    this.editingMasterStat.set(null);
    this.masterService.getMyMasterProfile().subscribe({
      next: (res) => {
        this.applyMasterProfileResponse(res);
        this.patchAuthFromMasterProfile(res);
        this.masterProfileLoading.set(false);
      },
      error: () => {
        this.clearMasterState();
        this.masterProfileLoading.set(false);
      },
    });
  }

  /** PATCH kategorije, zatim ponovo učitavanje profila sa servera. */
  public updateCategory(value: string): void {
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
          this.applyMasterProfileResponse(res);
          this.patchAuthFromMasterProfile(res);
          this.masterCategorySaving.set(false);
        },
        error: () => {
          this.masterCategoryError.set('Nije moguće sačuvati kategoriju.');
          this.masterCategorySaving.set(false);
        },
      });
  }

  public openMasterStatEdit(kind: MasterStatKind, ev?: Event): void {
    ev?.stopPropagation();
    if (this.editingMasterStat() === kind) return;
    if (this.masterStatsSaving() || this.masterProfileLoading()) return;
    const p = this.masterProfile();
    if (!p) return;
    this.masterStatsError.set(null);
    if (kind === 'experience') {
      this.experienceDraft.set(
        p.yearsOfExperience > 0 ? String(p.yearsOfExperience) : ''
      );
    } else {
      this.hourlyDraft.set(
        p.hourlyRateAmount > 0 ? String(p.hourlyRateAmount) : ''
      );
    }
    this.editingMasterStat.set(kind);
  }

  public saveMasterExperience(ev?: Event): void {
    ev?.stopPropagation();
    if (this.masterStatsSaving()) return;
    // type="number" + ngModel može vratiti broj — bez String() .trim() baca grešku
    const raw = String(this.experienceDraft() ?? '').trim();
    const n = parseInt(raw, 10);
    if (raw === '' || Number.isNaN(n) || n < 0 || n > 80) {
      this.masterStatsError.set('Unesi broj godina iskustva (0–80).');
      return;
    }
    this.masterStatsSaving.set(true);
    this.masterStatsError.set(null);
    this.masterService
      .patchProfileStats({ yearsOfExperience: n })
      .pipe(switchMap(() => this.masterService.getMyMasterProfile()))
      .subscribe({
        next: (res) => this.applyMasterStatSuccess(res),
        error: () => this.saveMasterStatError('Nije moguće sačuvati iskustvo.'),
      });
  }

  public saveMasterHourly(ev?: Event): void {
    ev?.stopPropagation();
    if (this.masterStatsSaving()) return;
    const raw = String(this.hourlyDraft() ?? '')
      .trim()
      .replace(',', '.');
    const n = parseFloat(raw);
    if (raw === '' || Number.isNaN(n) || n < 0) {
      this.masterStatsError.set('Unesi satnicu (broj ≥ 0).');
      return;
    }
    const p = this.masterProfile();
    this.masterStatsSaving.set(true);
    this.masterStatsError.set(null);
    this.masterService
      .patchProfileStats({
        hourlyRateAmount: n,
        hourlyRateCurrency: p?.hourlyRateCurrency || 'RSD',
      })
      .pipe(switchMap(() => this.masterService.getMyMasterProfile()))
      .subscribe({
        next: (res) => this.applyMasterStatSuccess(res),
        error: () => this.saveMasterStatError('Nije moguće sačuvati satnicu.'),
      });
  }

  public async loadJobs(): Promise<void> {
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

  public statusLabel(status: string): string {
    return jobStatusLabel(status);
  }

  public async acceptRequest(item: JobListItem): Promise<void> {
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

  public async declineRequest(item: JobListItem): Promise<void> {
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

  public navigateToMasters(): void {
    this.router.navigate(['/masters']);
  }

  public async loadCompanyTeam(): Promise<void> {
    this.companyTeam.set(undefined);
    this.companyMembers.set([]);
    this.pendingOutboundInviteMasterIds.set(new Set());
    try {
      const c = await firstValueFrom(this.companyService.getMyCompany());
      this.companyTeam.set(c);
      if (c) {
        void this.loadCompanyMembers();
        void this.loadPendingOutboundInvites();
      }
    } catch {
      this.companyTeam.set(null);
    }
  }

  public async loadCompanyMembers(): Promise<void> {
    this.loadingCompanyMembers.set(true);
    try {
      const list = await firstValueFrom(
        this.companyService.getMyCompanyWorkers()
      );
      this.companyMembers.set(list);
    } catch {
      this.companyMembers.set([]);
    } finally {
      this.loadingCompanyMembers.set(false);
    }
  }

  public workerCategoriesLine(w: CompanyWorkerMember): string {
    return workerCategoriesLineHelper(w);
  }

  public workerZonesLine(w: CompanyWorkerMember): string {
    return workerZonesLineHelper(w);
  }

  public onCompanyMemberTrashClick(member: CompanyWorkerMember): void {
    console.info(
      '[CompanyOwner] Trash click placeholder for worker:',
      member.userId
    );
  }

  public onMasterSearchInput(raw: string): void {
    const q = (raw ?? '').trim();
    if (this.masterSearchDebounce) clearTimeout(this.masterSearchDebounce);
    if (q.length < 2) {
      this.masterSearchResults.set([]);
      this.masterInviteError.set(null);
      return;
    }
    this.masterSearchDebounce = setTimeout(
      () => void this.runMasterSearch(q),
      320
    );
  }

  public masterAlreadyEmployed(userId: string): boolean {
    const n = normalizeMasterUserId(userId);
    return this.companyMembers().some(
      (w) => normalizeMasterUserId(w.userId) === n
    );
  }

  public masterInviteAlreadySent(userId: string): boolean {
    const n = normalizeMasterUserId(userId);
    return (
      this.invitedMasterIds().has(n) ||
      this.pendingOutboundInviteMasterIds().has(n)
    );
  }

  public inviteMasterButtonDisabled(m: MasterSearchForInviteItem): boolean {
    return (
      this.masterAlreadyEmployed(m.userId) ||
      this.masterInviteAlreadySent(m.userId) ||
      this.invitingMasterId() !== null
    );
  }

  public inviteMasterButtonLabel(m: MasterSearchForInviteItem): string {
    if (this.masterAlreadyEmployed(m.userId)) return 'U firmi';
    if (this.masterInviteAlreadySent(m.userId)) return 'Poslato';
    const busy = this.invitingMasterId();
    if (
      busy &&
      normalizeMasterUserId(busy) === normalizeMasterUserId(m.userId)
    ) {
      return 'Šaljem…';
    }
    return 'Pošalji poziv';
  }

  public inviteMasterButtonType(m: MasterSearchForInviteItem): ButtonType {
    if (
      this.masterAlreadyEmployed(m.userId) ||
      this.masterInviteAlreadySent(m.userId)
    ) {
      return BUTTON_TYPES.NEUTRAL;
    }
    return BUTTON_TYPES.POSITIVE;
  }

  public async inviteMasterToCompany(masterUserId: string): Promise<void> {
    if (this.invitingMasterId()) return;
    if (this.masterAlreadyEmployed(masterUserId)) return;
    if (this.masterInviteAlreadySent(masterUserId)) return;
    this.invitingMasterId.set(masterUserId);
    this.masterInviteError.set(null);
    const norm = normalizeMasterUserId(masterUserId);
    try {
      await firstValueFrom(this.companyService.inviteMaster(masterUserId));
      this.invitedMasterIds.update((prev) => new Set([...prev, norm]));
      void this.loadPendingOutboundInvites();
    } catch (err: unknown) {
      if (
        err instanceof HttpErrorResponse &&
        err.status === HttpStatusCode.Conflict
      ) {
        this.invitedMasterIds.update((prev) => new Set([...prev, norm]));
        void this.loadPendingOutboundInvites();
      } else {
        const msg = CompanyService.mapApiError(err as HttpErrorResponse);
        this.masterInviteError.set(msg);
      }
    } finally {
      this.invitingMasterId.set(null);
    }
  }

  public openJobEdit(job: JobListItem): void {
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
        this.clearEditLockToast();
        this.jobEditModal.set(job);
      });
    };

    const onDenied = (...args: unknown[]) => {
      this.ngZone.run(() => {
        const id = args[0] != null ? String(args[0]).toLowerCase() : '';
        if (id !== jobId) return;
        this.signalr.off('WriteDenied', onDenied as (...a: unknown[]) => void);
        this.showEditLockToast();
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
        this.showEditLockToast();
      });
    });
  }

  public closeJobEdit(): void {
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

  public onDeleteCompanyWorker(): void {}
}
