import {
  Component,
  computed,
  inject,
  NgZone,
  OnInit,
  signal,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { firstValueFrom } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { UserRole } from '../../shared/enums/user-role.enum';
import { CompanyService } from '../../shared/services/company.service';
import { ChatService } from '../../shared/services/chat.service';
import { AuthActions } from '../../shared/store/auth/auth.actions';
import type { User } from '../../shared/models/user.model';
import type { CompanyInvitationPending } from '../../shared/interfaces/company.interface';
import {
  JobService,
  type JobListItem,
} from '../../shared/services/job.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { ButtonComponent } from '../../components/button/button.component';
import { MasterService } from '../../shared/services/master.service';
import { BUTTON_TYPES } from '../../shared/types';
import { NewJobRequestPayload } from '../../shared/interfaces';
import { HUB_CHAT_URL } from '../../shared/constants/api.constants';
import { SIGNALR_STATUS } from '../../shared/types';
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
  private store = inject(Store);
  private jobService = inject(JobService);
  private companyService = inject(CompanyService);
  private chatService = inject(ChatService);
  private signalr = inject(SignalrService);
  private authService = inject(AuthService);
  private ngZone = inject(NgZone);
  private router = inject(Router);
  private masterService = inject(MasterService);

  readonly eButtonType = BUTTON_TYPES;
  readonly weekdays = REQUESTS_WEEKDAYS;

  requests = signal<JobListItem[]>([]);
  companyInvitations = signal<CompanyInvitationPending[]>([]);
  loadingCompanyInvites = signal(false);
  companyInviteError = signal<string | null>(null);
  loadingRequests = signal(false);
  requestError = signal<string | null>(null);
  actingRequestId = signal<string | null>(null);
  actingCompanyInviteId = signal<string | null>(null);
  currentMonth = signal<Date>(new Date());
  selectedDate = signal<string | null>(null);

  private realtimeHandlersRegistered = false;

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
    this.auth.userSelector$.subscribe((user) => this.onUserSelected(user));
  }

  private onUserSelected(user: { role?: UserRole } | null): void {
    if (
      user?.role !== UserRole.Master &&
      user?.role !== UserRole.CompanyWorker
    ) {
      this.router.navigate(['/home']);
      return;
    }
    this.ensureSignalR();
    this.registerRealtimeHandlersOnce();
    void this.loadRequests();
    void this.loadCompanyInvitations();
  }

  private registerRealtimeHandlersOnce(): void {
    if (this.realtimeHandlersRegistered) return;
    this.realtimeHandlersRegistered = true;
    this.signalr.on<NewJobRequestPayload>('NewJobRequest', (payload) => {
      this.ngZone.run(() =>
        this.requests.update((list) => mergeRequestFromPayload(list, payload))
      );
    });
    this.signalr.on<unknown>('CompanyInvitation', (payload) => {
      this.ngZone.run(() => this.mergeCompanyInvitationFromPayload(payload));
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

  private mergeCompanyInvitationFromPayload(raw: unknown): void {
    const p = raw as Record<string, unknown>;
    const invitationId = String(p['invitationId'] ?? p['InvitationId'] ?? '');
    const companyId = String(p['companyId'] ?? p['CompanyId'] ?? '');
    const companyName = String(p['companyName'] ?? p['CompanyName'] ?? '');
    const createdAtUtc = String(
      p['createdAtUtc'] ?? p['CreatedAtUtc'] ?? new Date().toISOString()
    );
    if (!invitationId || !companyId || !companyName) return;
    const item: CompanyInvitationPending = {
      invitationId,
      companyId,
      companyName,
      createdAtUtc,
    };
    this.companyInvitations.update((list) => {
      if (list.some((i) => i.invitationId === invitationId)) return list;
      return [item, ...list];
    });
  }

  async loadCompanyInvitations(): Promise<void> {
    this.loadingCompanyInvites.set(true);
    this.companyInviteError.set(null);
    try {
      const list = await firstValueFrom(
        this.companyService.getMyPendingCompanyInvitations()
      );
      this.companyInvitations.set(list);
    } catch {
      this.companyInvitations.set([]);
    } finally {
      this.loadingCompanyInvites.set(false);
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

  openChat(conversationId: string): void {
    this.router.navigate(['/chat'], { queryParams: { open: conversationId } });
  }

  async acceptCompanyInvite(item: CompanyInvitationPending): Promise<void> {
    if (this.actingCompanyInviteId() || this.actingRequestId()) return;
    this.actingCompanyInviteId.set(item.invitationId);
    this.companyInviteError.set(null);
    try {
      const res = await firstValueFrom(
        this.companyService.acceptCompanyInvitation(item.invitationId)
      );
      this.authService.saveToken(res.token);
      this.store.dispatch(
        AuthActions.loadUserSuccess({
          user: res.user as User,
          token: res.token,
        })
      );
      this.companyInvitations.update((list) =>
        list.filter((i) => i.invitationId !== item.invitationId)
      );
      this.realtimeHandlersRegistered = false;
      await this.signalr.disconnect();
      await this.signalr.connect(HUB_CHAT_URL, {
        accessTokenFactory: () => this.authService.getToken() ?? '',
      });
      this.registerRealtimeHandlersOnce();
      this.chatService.clearRealtimeHandlers();
      this.chatService.registerRealtimeHandlers();
      try {
        const profile = await firstValueFrom(
          this.masterService.getMyMasterProfile()
        );
        this.store.dispatch(
          AuthActions.patchUser({
            partial: {
              category: profile.category ?? null,
              employerCompanyName: profile.employerCompanyName?.trim()
                ? profile.employerCompanyName
                : null,
            },
          })
        );
      } catch {
        /* profil nije kritičan za prihvat */
      }
    } catch (err: unknown) {
      this.companyInviteError.set(
        err instanceof HttpErrorResponse
          ? CompanyService.mapApiError(err)
          : 'Nije moguće prihvatiti poziv.'
      );
    } finally {
      this.actingCompanyInviteId.set(null);
    }
  }

  async declineCompanyInvite(item: CompanyInvitationPending): Promise<void> {
    if (this.actingCompanyInviteId()) return;
    this.actingCompanyInviteId.set(item.invitationId);
    this.companyInviteError.set(null);
    try {
      await firstValueFrom(
        this.companyService.declineCompanyInvitation(item.invitationId)
      );
      this.companyInvitations.update((list) =>
        list.filter((i) => i.invitationId !== item.invitationId)
      );
    } catch {
      this.companyInviteError.set('Nije moguće odbiti poziv.');
    } finally {
      this.actingCompanyInviteId.set(null);
    }
  }
}
