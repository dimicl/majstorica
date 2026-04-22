import {
  Component,
  computed,
  DestroyRef,
  inject,
  NgZone,
  OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { isMasterLikeUserRole } from '../../shared/utils/user-role.util';
import {
  JobService,
  type JobListItem,
} from '../../shared/services/job.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types';
import { NewJobRequestPayload } from '../../shared/interfaces';
import { HUB_CHAT_URL } from '../../shared/constants/api.constants';
import { SIGNALR_STATUS } from '../../shared/types';
import { CompanyService } from '../../shared/services/company.service';
import { CompanyInvitationPending } from '../../shared/interfaces/company.interface';
import { firstValueFrom } from 'rxjs';
import {
  type CalendarDay,
  REQUESTS_WEEKDAYS,
  buildCalendarDays,
  groupRequestsByDayKey,
  getSelectedDayRequests,
  computeStats,
  buildChartData,
  getMonthLabel,
  prevMonthDate,
  nextMonthDate,
  toggleDaySelection,
  mergeRequestFromPayload,
  removeRequestByJobId,
  removeRequestByConversationId,
} from '../../shared/helpers/requests.helper';

@Component({
  selector: 'app-requests',
  standalone: true,
  imports: [CommonModule, DatePipe, ButtonComponent],
  templateUrl: './requests.component.html',
  styleUrl: './requests.component.scss',
})
export class RequestsComponent implements OnInit {
  private auth = inject(AuthSelectorService);
  private jobService = inject(JobService);
  private signalr = inject(SignalrService);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);
  private companyService = inject(CompanyService);

  readonly eButtonType = BUTTON_TYPES;
  readonly weekdays = REQUESTS_WEEKDAYS;

  requests = signal<JobListItem[]>([]);
  loadingRequests = signal(false);
  requestError = signal<string | null>(null);
  actingRequestId = signal<string | null>(null);
  companyInvitations = signal<CompanyInvitationPending[]>([]);
  loadingCompanyInvites = signal(false);
  companyInviteError = signal<string | null>(null);
  actingCompanyInviteId = signal<string | null>(null);
  currentMonth = signal<Date>(new Date());
  selectedDate = signal<string | null>(null);

  private newJobRequestHandlerRegistered = false;

  calendarDays = computed<CalendarDay[]>(() =>
    buildCalendarDays(this.currentMonth(), this.requests())
  );
  requestsByDayKey = computed(() => groupRequestsByDayKey(this.requests()));
  selectedDayRequests = computed(() =>
    getSelectedDayRequests(
      this.requests(),
      this.requestsByDayKey(),
      this.selectedDate()
    )
  );
  stats = computed(() => computeStats(this.requests()));
  chartData = computed(() => buildChartData(this.requestsByDayKey(), 3));
  monthLabel = computed(() => getMonthLabel(this.currentMonth()));

  ngOnInit(): void {
    this.auth.userSelector$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((user) => this.onUserSelected(user));
  }

  private onUserSelected(user: { role?: unknown } | null): void {
    // Dok je user null (pre loadUser), ne redirectuj – inače majstor odmah ide na /home.
    if (!user) return;
    if (!isMasterLikeUserRole(user.role)) {
      this.router.navigate(['/home']);
      return;
    }
    this.ensureSignalR();
    this.registerNewJobRequestHandlerOnce();
    void this.loadRequests();
    void this.loadCompanyInvitations();
  }

  private registerNewJobRequestHandlerOnce(): void {
    if (this.newJobRequestHandlerRegistered) return;
    this.newJobRequestHandlerRegistered = true;
    this.signalr.on<NewJobRequestPayload>('NewJobRequest', (payload) => {
      this.ngZone.run(() =>
        this.requests.update((list) => mergeRequestFromPayload(list, payload))
      );
    });
  }

  private ensureSignalR(): void {
    const token = this.authService.getToken();
    if (token && this.signalr.status() !== SIGNALR_STATUS.CONNECTED) {
      void this.signalr.connect(HUB_CHAT_URL, {
        accessTokenFactory: () => this.authService.getToken() ?? '',
      });
    }
  }

  async loadRequests(): Promise<void> {
    this.loadingRequests.set(true);
    this.requestError.set(null);
    try {
      const all = await this.jobService.getJobs();
      this.requests.set(all.filter((j) => j.status === 'Pending'));
    } catch (err: unknown) {
      this.requests.set([]);
    } finally {
      this.loadingRequests.set(false);
    }
  }

  async loadCompanyInvitations(): Promise<void> {
    this.loadingCompanyInvites.set(true);
    this.companyInviteError.set(null);
    try {
      const invites = await firstValueFrom(
        this.companyService.getMyPendingCompanyInvitations()
      );
      this.companyInvitations.set(invites);
    } catch {
      this.companyInvitations.set([]);
      this.companyInviteError.set('Nije moguće učitati pozive u firmu.');
    } finally {
      this.loadingCompanyInvites.set(false);
    }
  }

  prevMonth(): void {
    this.currentMonth.set(prevMonthDate(this.currentMonth()));
  }

  nextMonth(): void {
    this.currentMonth.set(nextMonthDate(this.currentMonth()));
  }

  selectDay(day: CalendarDay): void {
    if (!day.isCurrentMonth) return;
    this.selectedDate.set(toggleDaySelection(this.selectedDate(), day.dayKey));
  }

  clearSelection(): void {
    this.selectedDate.set(null);
  }

  async acceptRequest(item: JobListItem): Promise<void> {
    if (this.actingRequestId()) return;
    this.actingRequestId.set(item.jobId);
    this.requestError.set(null);
    try {
      await this.jobService.acceptJob(item.jobId);
      this.requests.update((list) => removeRequestByJobId(list, item.jobId));
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
      this.requests.update((list) =>
        removeRequestByConversationId(list, item.conversationId)
      );
    } catch {
      this.requestError.set('Nije moguće odbiti zahtev.');
    } finally {
      this.actingRequestId.set(null);
    }
  }

  async acceptCompanyInvite(invitation: CompanyInvitationPending): Promise<void> {
    if (this.actingCompanyInviteId() || this.actingRequestId()) return;
    this.actingCompanyInviteId.set(invitation.invitationId);
    this.companyInviteError.set(null);
    try {
      const authResponse = await firstValueFrom(
        this.companyService.acceptCompanyInvitation(invitation.invitationId)
      );
      this.authService.saveToken(authResponse.token);
      this.authService.saveUserId(authResponse.user.id);
      this.auth.dispatchLoadUser();
      this.companyInvitations.update((list) =>
        list.filter((x) => x.invitationId !== invitation.invitationId)
      );
    } catch {
      this.companyInviteError.set('Nije moguće prihvatiti poziv u firmu.');
    } finally {
      this.actingCompanyInviteId.set(null);
    }
  }

  async declineCompanyInvite(
    invitation: CompanyInvitationPending
  ): Promise<void> {
    if (this.actingCompanyInviteId()) return;
    this.actingCompanyInviteId.set(invitation.invitationId);
    this.companyInviteError.set(null);
    try {
      await firstValueFrom(
        this.companyService.declineCompanyInvitation(invitation.invitationId)
      );
      this.companyInvitations.update((list) =>
        list.filter((x) => x.invitationId !== invitation.invitationId)
      );
    } catch {
      this.companyInviteError.set('Nije moguće odbiti poziv u firmu.');
    } finally {
      this.actingCompanyInviteId.set(null);
    }
  }

  openChat(conversationId: string): void {
    this.router.navigate(['/chat'], { queryParams: { open: conversationId } });
  }
}
