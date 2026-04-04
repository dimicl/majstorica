import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
  signal,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { toSignal } from '@angular/core/rxjs-interop';
import { Store } from '@ngrx/store';
import { JobService } from '../../shared/services/job.service';
import { ButtonComponent } from '../button/button.component';
import { BUTTON_TYPES } from '../../shared/types/button.type';
import { selectClientProfile } from '../../shared/store';
import { MASTER_CATEGORY_OPTIONS } from '../../shared/enums/master-category.enum';

export interface CreateJobMaster {
  id: string;
  fullName: string;
  username: string;
}

@Component({
  selector: 'app-create-job-modal',
  imports: [CommonModule, FormsModule, DragDropModule, ButtonComponent],
  templateUrl: './create-job-modal.component.html',
  styleUrl: './create-job-modal.component.scss',
})
export class CreateJobModalComponent {
  private jobService = inject(JobService);
  private store = inject(Store);

  @Input() set master(value: CreateJobMaster | null | undefined) {
    this.selectedMaster.set(value ?? null);
  }
  selectedMaster = signal<CreateJobMaster | null>(null);

  /** Adresa klijenta koji kreira posao (iz profila). */
  clientProfile = toSignal(this.store.select(selectClientProfile), {
    initialValue: null,
  });

  public eButtonType = BUTTON_TYPES;

  @Output() closed = new EventEmitter<void>();
  @Output() created = new EventEmitter<{ jobId: string; masterId?: string }>();

  title = '';
  description = '';
  scheduledDate = '';
  price: number | null = null;
  isEmergency = false;
  /** Prikazno ime kategorije (marketplace); prazno dok korisnik ne izabere. */
  serviceCategoryLabel = '';
  readonly masterCategoryOptions = MASTER_CATEGORY_OPTIONS;
  isSubmitting = signal(false);
  submitError = signal<string | null>(null);

  get minDate(): string {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    return d.toISOString().slice(0, 10);
  }


  close(): void {
    this.closed.emit();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.close();
  }

  async onSubmit(): Promise<void> {
    this.submitError.set(null);
    const desc = this.description.trim();
    if (!desc) return;

    const m = this.selectedMaster();
    if (!m) {
      const cat = this.serviceCategoryLabel.trim();
      if (!cat) {
        this.submitError.set('Izaberite kategoriju majstora.');
        return;
      }
    }

    this.isSubmitting.set(true);
    try {
      const jobId = await this.jobService.createJob({
        title: this.title.trim(),
        description: desc,
        scheduledDate: this.scheduledDate || null,
        price: this.price ?? null,
        isEmergency: this.isEmergency,
        serviceCategory: m ? undefined : this.serviceCategoryLabel.trim(),
      });

      if (jobId && m) await this.jobService.sendRequests(jobId, [m.id]);
      this.created.emit(m ? { jobId, masterId: m.id } : { jobId });
      this.close();
    } catch (err) {
      this.submitError.set(
        err instanceof Error
          ? err.message
          : 'Nije moguće kreirati posao. Pokušajte ponovo.'
      );
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
