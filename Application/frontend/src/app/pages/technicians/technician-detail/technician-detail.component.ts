import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MasterService } from '../../../shared/services/master.service';
import { UserResponse } from '../../../shared/interfaces';
import { ChatService } from '../../../shared/services/chat.service';
import { ButtonComponent } from '../../../components/button/button.component';
import { BUTTON_TYPES } from '../../../shared/types';

@Component({
  selector: 'app-technician-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ButtonComponent],
  templateUrl: './technician-detail.component.html',
  styleUrl: './technician-detail.component.scss',
})
export class TechnicianDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private masterService = inject(MasterService);
  private chatService = inject(ChatService);

  master = signal<UserResponse | null>(null);
  isLoading = signal(true);
  error = signal<string | null>(null);
  isStartingChat = signal(false);

  eButtonType = BUTTON_TYPES;

  fullName = computed(() => {
    const u = this.master()?.user;
    if (!u) return '';
    return `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.username;
  });

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
    try {
      const data = await this.masterService.getMasterById(id);
      this.master.set(data);
      if (!data) this.error.set('Majstor nije pronađen.');
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
      const { id: conversationId } = await this.chatService.startConversationWithMaster(m.user.id);
      await this.router.navigate(['/chat'], { queryParams: { open: conversationId } });
    } catch {
      this.error.set('Nije moguće otvoriti chat.');
    } finally {
      this.isStartingChat.set(false);
    }
  }
}
