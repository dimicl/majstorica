import {
  Component,
  OnInit,
  inject,
  signal,
  computed,
  NgModule,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MasterService } from '../../../shared/services/master.service';
import { UserResponse } from '../../../shared/interfaces';
import { ChatService } from '../../../shared/services/chat.service';
import { JobService } from '../../../shared/services/job.service';
import { ButtonComponent } from '../../../components/button/button.component';
import { CreateJobModalComponent } from '../../../components/create-job-modal/create-job-modal.component';
import { BUTTON_TYPES } from '../../../shared/types';
import { SvgIconComponent } from 'angular-svg-icon';
import { SharedSvgRoutes } from '../../../shared/constants/shared_svg_routes';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-technician-detail',
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent,
    CreateJobModalComponent,
    SvgIconComponent,
    NgbModule,
  ],
  templateUrl: './technician-detail.component.html',
  styleUrl: './technician-detail.component.scss',
})
export class TechnicianDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private masterService = inject(MasterService);
  private chatService = inject(ChatService);
  jobService = inject(JobService);

  master = signal<UserResponse | null>(null);
  isLoading = signal(true);
  error = signal<string | null>(null);
  isStartingChat = signal(false);
  showCreateJobModal = signal(false);

  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;

  constructor(private modalService: NgbModal) {}

  fullName = computed(() => {
    const u = this.master()?.user;
    if (!u) return '';
    return `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.username;
  });

  /** Podaci za create-job modal (izabrani majstor). */
  createJobMaster = computed(() => {
    const m = this.master();
    if (!m?.user) return null;
    return {
      id: m.user.id,
      fullName: this.fullName(),
      username: '@' + m.user.username,
    };
  });

  /** Da li postoji job između mene (klijenta) i ovog majstora. */
  /** Da li postoji posao (zahtev) između trenutnog klijenta i ovog majstora. */
  hasRequestedThisMaster = signal(false);
  createJobButtonLabel = computed(() =>
    this.hasRequestedThisMaster() ? 'Već kreirano' : 'Kreiraj posao'
  );

  usernameWithAt(username: string): string {
    return '@' + username;
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Majstor nije pronađen.');
      this.isLoading.set(false);
      return;
    }
    this.loadMaster(id);
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

  async onWriteMessage(): Promise<void> {
    const m = this.master();
    if (!m?.user?.id) return;
    this.isStartingChat.set(true);
    try {
      const { id: conversationId } =
        await this.chatService.startConversationWithMaster(m.user.id);
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
    const modalRef = this.modalService.open(CreateJobModalComponent, {
      size: 'md',
      centered: true,
    });
    modalRef.componentInstance.selectedMaster = this.createJobMaster();
    modalRef.componentInstance.created.subscribe(() => {
      this.hasRequestedThisMaster.set(true);
    });
    modalRef.closed.subscribe(() => this.closeCreateJobModal());
  }

  closeCreateJobModal(): void {
    this.showCreateJobModal.set(false);
  }

  onJobCreated(): void {
    this.closeCreateJobModal();
  }
}
