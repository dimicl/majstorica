import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CompanyService } from '../../../shared/services/company.service';
import { ChatService } from '../../../shared/services/chat.service';
import { JobService } from '../../../shared/services/job.service';
import { ButtonComponent } from '../../../components/button/button.component';
import { type CompanyPublicDto } from '../../../shared/interfaces/company.interface';
import { type CreateJobMaster } from '../../../components/create-job-modal/create-job-modal.component';
import { BUTTON_TYPES } from '../../../shared/types';

@Component({
  selector: 'app-company-detail-modal',
  imports: [CommonModule, ButtonComponent],
  templateUrl: './company-detail-modal.component.html',
  styleUrl: './company-detail-modal.component.scss',
})
export class CompanyDetailModalComponent implements OnChanges {
  private companyService = inject(CompanyService);
  private chatService = inject(ChatService);
  private jobService = inject(JobService);
  private router = inject(Router);

  @Input() companyId: string | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() openCreateJob = new EventEmitter<{ master: CreateJobMaster }>();

  company = signal<CompanyPublicDto | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  isStartingChat = signal(false);
  hasRequestedOwner = signal(false);

  eButtonType = BUTTON_TYPES;

  headerTitle = computed(() => {
    if (this.isLoading()) return 'Učitavanje…';
    return this.company()?.name?.trim() || 'Firma';
  });

  createJobTarget = computed((): CreateJobMaster | null => {
    const c = this.company();
    if (!c?.ownerUserId) return null;
    return {
      id: c.ownerUserId,
      fullName: c.name,
      username: c.email?.trim() ? c.email.trim() : '—',
    };
  });

  createJobButtonLabel = computed(() =>
    this.hasRequestedOwner() ? 'Već kreirano' : 'Kreiraj posao'
  );

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['companyId']) return;

    if (!this.companyId) {
      this.company.set(null);
      this.error.set(null);
      this.isLoading.set(false);
      this.hasRequestedOwner.set(false);
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);
    this.company.set(null);
    this.hasRequestedOwner.set(false);
    this.companyService.getPublicCompany(this.companyId).subscribe({
      next: async (c) => {
        this.company.set(c);
        this.isLoading.set(false);
        try {
          const has = await this.jobService.hasSentRequestToMaster(
            c.ownerUserId
          );
          this.hasRequestedOwner.set(has);
        } catch {
          this.hasRequestedOwner.set(false);
        }
      },
      error: () => {
        this.error.set('Nije moguće učitati podatke o firmi.');
        this.isLoading.set(false);
      },
    });
  }

  close(): void {
    this.closed.emit();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.close();
  }

  async onWriteMessage(): Promise<void> {
    const c = this.company();
    if (!c?.ownerUserId) return;
    this.isStartingChat.set(true);
    try {
      const { id: conversationId } =
        await this.chatService.startConversationWithMaster(c.ownerUserId);
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
    const target = this.createJobTarget();
    if (target) {
      this.openCreateJob.emit({ master: target });
      this.close();
    }
  }
}
