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
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { UserRole } from '../../shared/enums/user-role.enum';
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

  readonly eButtonType = BUTTON_TYPES;
  readonly weekdays = REQUESTS_WEEKDAYS;

  requests = signal<JobListItem[]>([]);
  loadingRequests = signal(false);
  requestError = signal<string | null>(null);
  actingRequestId = signal<string | null>(null);
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
    this.auth.userSelector$.subscribe((user) => this.onUserSelected(user));
  }

  private onUserSelected(user: { role?: UserRole } | null): void {
    if (user?.role !== UserRole.Master) {
      this.router.navigate(['/home']);
      return;
    }
    this.ensureSignalR();
    this.registerNewJobRequestHandlerOnce();
    void this.loadRequests();
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
}
