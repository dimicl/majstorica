import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
  inject,
  signal,
  computed,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MasterService } from '../../../shared/services/master.service';
import { ChatService } from '../../../shared/services/chat.service';
import { JobService } from '../../../shared/services/job.service';
import { ButtonComponent } from '../../../components/button/button.component';
import { type CreateJobMaster } from '../../../components/create-job-modal/create-job-modal.component';
import { BUTTON_TYPES } from '../../../shared/types';
import { UserResponse } from '../../../shared/interfaces';
import { AvatarComponent } from '../../../components/avatar/avatar.component';
import { SharedSvgRoutes } from '../../../shared/constants/shared_svg_routes';
import { SvgIconComponent } from 'angular-svg-icon';
import { AddressDisplayPipe } from '../../../shared/pipes/address-display.pipe';

@Component({
  selector: 'app-technician-detail-modal',
  imports: [
    CommonModule,
    ButtonComponent,
    AvatarComponent,
    SvgIconComponent,
    AddressDisplayPipe,
  ],
  templateUrl: './technician-detail-modal.component.html',
  styleUrl: './technician-detail-modal.component.scss',
})
export class TechnicianDetailModalComponent {
  private masterService = inject(MasterService);
  private chatService = inject(ChatService);
  jobService = inject(JobService);
  private router = inject(Router);

  @Input() set masterId(value: string | null) {
    this._masterId.set(value ?? null);
  }
  _masterId = signal<string | null>(null);

  public sharedSvgRoutes = SharedSvgRoutes;

  @Output() closed = new EventEmitter<void>();
  /** Emituje se pre zatvaranja modala – roditelj prikazuje Create Job na svom nivou. */
  @Output() openCreateJob = new EventEmitter<{ master: CreateJobMaster }>();

  master = signal<UserResponse | null>(null);
  isLoading = signal(true);
  error = signal<string | null>(null);
  isStartingChat = signal(false);

  public eButtonType = BUTTON_TYPES;

  fullName = computed(() => {
    const u = this.master()?.user;
    if (!u) return '';
    return `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.username;
  });

  createJobMaster = computed(() => {
    const m = this.master();
    if (!m?.user) return null;
    return {
      id: m.user.id,
      fullName: this.fullName(),
      username: '@' + m.user.username,
    };
  });

  /** Da li postoji posao (zahtev) između trenutnog klijenta i ovog majstora. */
  hasRequestedThisMaster = signal(false);
  createJobButtonLabel = computed(() =>
    this.hasRequestedThisMaster() ? 'Već kreirano' : 'Kreiraj posao'
  );

  constructor() {
    effect(() => {
      const id = this._masterId();
      if (!id) {
        this.master.set(null);
        this.error.set(null);
        return;
      }
      this.loadMaster(id);
    });
  }

  usernameWithAt(username: string): string {
    return '@' + username;
  }

  async loadMaster(id: string): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    this.hasRequestedThisMaster.set(false);
    try {
      const data = await this.masterService.getMasterById(id);
      this.master.set(data);
      if (!data) {
        this.error.set('Majstor nije pronađen.');
      } else {
        const hasSent = await this.jobService.hasSentRequestToMaster(
          data.user.id
        );
        this.hasRequestedThisMaster.set(hasSent);
      }
    } catch {
      this.error.set('Nije moguće učitati profil.');
    } finally {
      this.isLoading.set(false);
    }
  }

  close(): void {
    this.closed.emit();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.close();
  }

  async onWriteMessage(): Promise<void> {
    const m = this.master();
    if (!m?.user?.id) return;
    this.isStartingChat.set(true);
    try {
      const { id: conversationId } =
        await this.chatService.startConversationWithMaster(m.user.id);
      this.close();
      await this.router.navigate(['/chat'], {
        queryParams: { open: conversationId },
      });
    } catch {
      this.error.set('Nije moguće otvoriti chat.');
    } finally {
      this.isStartingChat.set(false);
    }
  }

  openCreateJobModal(): void {
    const master = this.createJobMaster();
    if (master) {
      this.openCreateJob.emit({ master });
      this.close();
    }
  }
}
