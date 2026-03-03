import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnInit,
  OnDestroy,
  NgZone,
  inject,
  signal,
  HostListener,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonComponent } from '../button/button.component';
import type { JobListItem } from '../../shared/services/job.service';
import { JobService } from '../../shared/services/job.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { SIGNALR_STATUS } from '../../shared/types';
import { HUB_CHAT_URL } from '../../shared/constants/api.constants';

@Component({
  selector: 'app-job-edit-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, ButtonComponent],
  templateUrl: './job-edit-modal.component.html',
  styleUrl: './job-edit-modal.component.scss',
})
export class JobEditModalComponent implements OnInit, OnDestroy {
  @Input({ required: true }) job!: JobListItem;
  @Output() closed = new EventEmitter<void>();
  @Output() saved = new EventEmitter<void>();

  private jobService = inject(JobService);
  private signalr = inject(SignalrService);
  private auth = inject(AuthService);
  private ngZone = inject(NgZone);

  canEdit = signal(false);
  lockMessage = signal<string | null>(null);
  toastMessage = signal<string | null>(null);
  lockLoading = signal(true);
  savingDescription = signal(false);
  savingPrice = signal(false);
  saveError = signal<string | null>(null);

  description = '';
  price: number | null = null;

  private writeGrantedHandler: ((...args: unknown[]) => void) | null = null;
  private writeDeniedHandler: ((...args: unknown[]) => void) | null = null;
  private toastDismissTimer: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.description = this.job.description ?? '';
    this.price = this.job.price ?? null;
    this.ensureSignalRAndJoinJob();
  }

  @HostListener('window:beforeunload')
  onBeforeUnload(): void {
    this.leaveJobAndCleanup();
  }

  ngOnDestroy(): void {
    if (this.toastDismissTimer) clearTimeout(this.toastDismissTimer);
    this.leaveJobAndCleanup();
  }

  private scheduleToastDismiss(): void {
    if (this.toastDismissTimer) clearTimeout(this.toastDismissTimer);
    this.toastDismissTimer = setTimeout(() => {
      this.toastMessage.set(null);
      this.toastDismissTimer = null;
    }, 4000);
  }

  private ensureSignalRAndJoinJob(): void {
    const token = this.auth.getToken();
    if (!token) {
      this.lockLoading.set(false);
      this.canEdit.set(false);
      this.lockMessage.set('Niste ulogovani.');
      return;
    }
    if (this.signalr.status() !== SIGNALR_STATUS.CONNECTED) {
      this.signalr
        .connect(HUB_CHAT_URL, { accessTokenFactory: () => this.auth.getToken() ?? '' })
        .then(() => this.joinJobAndRegisterHandlers());
    } else {
      this.joinJobAndRegisterHandlers();
    }
  }

  private joinJobAndRegisterHandlers(): void {
    const jobId = this.job.jobId;
    const myUserId = (this.auth.getUserIdFromStorage() ?? '').toLowerCase();

    this.writeGrantedHandler = (...args: unknown[]) => {
      this.ngZone.run(() => {
        const receivedJobId = args[0] != null ? String(args[0]).toLowerCase() : '';
        if (receivedJobId !== jobId.toLowerCase()) return;
        const nextUserId = args.length > 1 && args[1] != null ? String(args[1]).toLowerCase() : undefined;
        const isForMe = nextUserId === undefined || nextUserId === myUserId;
        if (isForMe) {
          this.canEdit.set(true);
          this.lockMessage.set(null);
          this.toastMessage.set(null);
        }
        this.lockLoading.set(false);
      });
    };

    this.writeDeniedHandler = (...args: unknown[]) => {
      this.ngZone.run(() => {
        const receivedJobId = args[0] != null ? String(args[0]).toLowerCase() : '';
        if (receivedJobId !== jobId.toLowerCase()) return;
        this.canEdit.set(false);
        this.lockMessage.set(
          'Posao trenutno uređuje drugi korisnik. Sačekajte da završi.'
        );
        this.toastMessage.set('Trenutno ne možete uređivati.');
        this.lockLoading.set(false);
        this.scheduleToastDismiss();
      });
    };

    this.signalr.on('WriteGranted', this.writeGrantedHandler as (p: unknown) => void);
    this.signalr.on('WriteDenied', this.writeDeniedHandler as (p: unknown) => void);

    this.signalr.invoke('JoinJob', jobId).then(
      () => this.lockLoading.set(false),
      () => {
        this.lockLoading.set(false);
        this.canEdit.set(false);
        this.lockMessage.set('Greška pri zaključavanju.');
      }
    );
  }

  private leaveJobAndCleanup(): void {
    const jobId = this.job.jobId;
    if (this.writeGrantedHandler) {
      this.signalr.off('WriteGranted', this.writeGrantedHandler as (...a: unknown[]) => void);
      this.writeGrantedHandler = null;
    }
    if (this.writeDeniedHandler) {
      this.signalr.off('WriteDenied', this.writeDeniedHandler as (...a: unknown[]) => void);
      this.writeDeniedHandler = null;
    }
    this.signalr.invoke('LeaveJob', jobId).catch(() => {});
  }

  close(): void {
    this.closed.emit();
  }

  async saveDescription(): Promise<void> {
    if (!this.canEdit()) return;
    this.saveError.set(null);
    this.savingDescription.set(true);
    try {
      await this.jobService.changeDescription(this.job.jobId, this.description);
      this.saved.emit();
    } catch (err: unknown) {
      const msg =
        (err as { error?: { message?: string } })?.error?.message ??
        (err as Error)?.message ??
        'Greška pri čuvanju opisa.';
      this.saveError.set(msg);
    } finally {
      this.savingDescription.set(false);
    }
  }

  async savePrice(): Promise<void> {
    if (!this.canEdit()) return;
    this.saveError.set(null);
    this.savingPrice.set(true);
    try {
      await this.jobService.changePrice(this.job.jobId, this.price);
      this.saved.emit();
    } catch (err: unknown) {
      const msg =
        (err as { error?: { message?: string } })?.error?.message ??
        (err as Error)?.message ??
        'Greška pri čuvanju cene.';
      this.saveError.set(msg);
    } finally {
      this.savingPrice.set(false);
    }
  }
}
