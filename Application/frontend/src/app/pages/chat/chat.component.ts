import { Component, computed, inject, NgZone, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ChatPanelComponent } from '../../components/chat-panel/chat-panel.component';
import { ButtonComponent } from '../../components/button/button.component';
import { ChatService } from '../../shared/services/chat.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { UserRole } from '../../shared/enums/user-role.enum';
import { BUTTON_TYPES, SIGNALR_STATUS } from '../../shared/types';
import { HUB_CHAT_URL } from '../../shared/constants/api.constants';
import type {
  ChatMessage,
  ChatThread,
  ReceiveMessagePayload,
} from '../../shared/interfaces';

@Component({
  selector: 'app-chat',
  imports: [CommonModule, ChatPanelComponent, ButtonComponent],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss',
})
export class ChatComponent {
  private readonly emptyJobId = '00000000-0000-0000-0000-000000000000';
  private chat = inject(ChatService);
  private signalr = inject(SignalrService);
  private auth = inject(AuthService);
  private authSelector = inject(AuthSelectorService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private ngZone = inject(NgZone);

  readonly hideExploreMastersButton = signal(false);
  realtimeError = this.signalr.lastError;
  eButtonType = BUTTON_TYPES;
  isLoadingThreads = signal(true);
  isLoadingMessages = signal(false);
  threads = signal<ChatThread[]>([]);
  messagesByThread = signal<Record<string, ChatMessage[]>>({});
  selectedThreadId = signal<string | null>(null);

  selectedThread = computed(() => {
    const id = this.selectedThreadId();
    return id ? this.threads().find((t) => t.id === id) ?? null : null;
  });

  threadInitials = computed(() => {
    const thread = this.selectedThread();
    return thread
      ? {
          firstName: thread.title.split(' ')[0],
          lastName: thread.title.split(' ')[1],
        }
      : null;
  });

  selectedMessages = computed(() => {
    const id = this.selectedThreadId();
    return id ? this.messagesByThread()[id] ?? null : null;
  });

  constructor() {
    const token = this.auth.getToken();
    if (token && this.signalr.status() !== SIGNALR_STATUS.CONNECTED) {
      const options = {
        accessTokenFactory: () => this.auth.getToken() ?? '',
      };
      void this.signalr.connect(HUB_CHAT_URL, options);
    }

    this.signalr.on<ReceiveMessagePayload>('ReceiveMessage', (payload) => {
      this.ngZone.run(() => this.handleReceiveMessage(payload));
    });

    this.authSelector.userSelector$.subscribe((user) => {
      this.hideExploreMastersButton.set(
        user?.role === UserRole.Master || user?.role === UserRole.CompanyWorker
      );
    });

    void this.loadThreads();
  }

  explore = (): void => {
    void this.router.navigate(['/masters']);
  };

  private handleReceiveMessage(payload: ReceiveMessagePayload): void {
    const id = payload.id;
    const convId = payload.conversationId;
    const senderId = payload.senderId;
    const content = payload.content;
    const sentAt = payload.sentAt;

    const currentUserId = this.auth.getUserIdFromStorage() ?? '';
    const isFromMe = senderId === currentUserId;
    const msg: ChatMessage = {
      id: id,
      from: isFromMe ? 'me' : 'them',
      text: content,
      time: this.formatTimeOnly(sentAt),
      sentAt,
    };
    let current = this.messagesByThread()[convId] ?? [];
    if (isFromMe) {
      current = current.filter(
        (m) => !(String(m.id).startsWith('opt-') && m.text === content)
      );
    }
    this.messagesByThread.set({
      ...this.messagesByThread(),
      [convId]: [...current, msg],
    });

    const threads = this.threads();
    const idx = threads.findIndex((t) => t.id === convId);
    if (idx !== -1) {
      const t = threads[idx];
      const isOtherConversation = this.selectedThreadId() !== convId;
      const addUnread = !isFromMe && isOtherConversation ? 1 : 0;
      const updated: ChatThread = {
        ...t,
        lastMessage: content,
        updatedAt: this.formatTimeOrDate(sentAt),
        unreadCount: t.unreadCount + addUnread,
      };
      const reordered = [updated, ...threads.filter((x) => x.id !== convId)];
      this.threads.set(reordered);
      if (addUnread > 0) {
        this.chat.setHasNewMessages(true);
      }
    } else if (convId) {
      void this.refreshThreads();
    }
  }

  private setThreadUnread(threadId: string, count: number): void {
    const threads = this.threads();
    const idx = threads.findIndex((t) => t.id === threadId);
    if (idx === -1) return;
    const updated: ChatThread = { ...threads[idx], unreadCount: count };
    this.threads.set(
      threads.slice(0, idx).concat(updated, threads.slice(idx + 1))
    );
  }

  private formatTimeOnly(iso: string): string {
    const d = new Date(iso);
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  }

  private formatLastSeen(iso: string): string {
    const d = new Date(iso);
    const now = new Date();
    const isToday =
      d.getDate() === now.getDate() &&
      d.getMonth() === now.getMonth() &&
      d.getFullYear() === now.getFullYear();
    const yesterday = new Date(now);
    yesterday.setDate(yesterday.getDate() - 1);
    const isYesterday =
      d.getDate() === yesterday.getDate() &&
      d.getMonth() === yesterday.getMonth() &&
      d.getFullYear() === yesterday.getFullYear();
    const time = `${String(d.getHours()).padStart(2, '0')}:${String(
      d.getMinutes()
    ).padStart(2, '0')}`;
    if (isToday) return `Poslednje aktivan: danas ${time}`;
    if (isYesterday) return `Poslednje aktivan: juče ${time}`;
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `Poslednje aktivan: ${day}.${month}.${year}. ${time}`;
  }

  private formatTimeOrDate(iso: string): string {
    const d = new Date(iso);
    const now = new Date();
    const isToday =
      d.getDate() === now.getDate() &&
      d.getMonth() === now.getMonth() &&
      d.getFullYear() === now.getFullYear();
    if (isToday) {
      const hh = String(d.getHours()).padStart(2, '0');
      const mm = String(d.getMinutes()).padStart(2, '0');
      return `${hh}:${mm}`;
    }
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}.${month}.${year}.`;
  }

  private threadPlaceholder(conversationId: string): ChatThread {
    return {
      id: conversationId,
      jobId: '00000000-0000-0000-0000-000000000000',
      title: 'Razgovor',
      subtitle: 'Razgovor',
      lastMessage: '',
      updatedAt: '',
      unreadCount: 0,
      presence: 'offline',
    };
  }

  async refreshThreads(): Promise<void> {
    const list = await this.chat.getThreads();
    this.threads.set(list);
  }

  private async applyConversationDetails(
    conversationId: string
  ): Promise<void> {
    const conv = await this.chat.getConversation(conversationId);
    if (!conv) return;
    const list = this.threads();
    const idx = list.findIndex((t) => t.id === conversationId);
    if (idx === -1) return;
    const isOnline = conv.isOnline ?? conv.IsOnline ?? false;
    const lastSeenIso =
      conv.otherPartyLastSeen ?? conv.OtherPartyLastSeen ?? null;
    const lastSeenText =
      !isOnline && lastSeenIso
        ? this.formatLastSeen(lastSeenIso)
        : undefined;
    const updated: ChatThread = {
      ...list[idx],
      title: conv.otherPartyName || 'Razgovor',
      lastMessage: conv.lastMessageText ?? '',
      updatedAt: conv.lastMessageAt
        ? this.formatTimeOrDate(conv.lastMessageAt)
        : '',
      unreadCount: conv.unreadCount ?? list[idx].unreadCount,
      presence: isOnline ? 'online' : 'offline',
      lastSeenText,
    };
    this.threads.set(list.slice(0, idx).concat(updated, list.slice(idx + 1)));
  }

  async loadThreads(): Promise<void> {
    this.isLoadingThreads.set(true);
    const openId = this.route.snapshot.queryParams['open'] ?? null;
    try {
      const list = await this.chat.getThreads();
      this.threads.set(list);
      if (openId && list.some((t) => t.id === openId)) {
        await this.selectThread(openId);
        await this.router.navigate([], {
          queryParams: { open: undefined },
          queryParamsHandling: 'merge',
        });
      } else if (openId) {
        const placeholder = this.threadPlaceholder(openId);
        const placeholders = [...list, placeholder];
        this.threads.set(placeholders);
        await this.selectThread(openId);
        await this.applyConversationDetails(openId);
        await this.router.navigate([], {
          queryParams: { open: undefined },
          queryParamsHandling: 'merge',
        });
      }
    } catch {
      if (openId) {
        this.threads.set([this.threadPlaceholder(openId)]);
        await this.selectThread(openId);
        await this.applyConversationDetails(openId);
        await this.router.navigate([], {
          queryParams: { open: undefined },
          queryParamsHandling: 'merge',
        });
      }
    } finally {
      this.isLoadingThreads.set(false);
    }
  }

  async selectThread(threadId: string): Promise<void> {
    this.selectedThreadId.set(threadId);
    await this.chat.markRead(threadId);
    this.setThreadUnread(threadId, 0);
    void this.chat.refreshUnreadIndicator();

    try {
      await this.signalr.invoke('JoinConversation', threadId);
    } catch {
      // hub može odbiti ako nisi autorizovan
    }

    if (this.messagesByThread()[threadId]) return;

    this.isLoadingMessages.set(true);
    try {
      const messages = await this.chat.getMessages(threadId);
      this.messagesByThread.set({
        ...this.messagesByThread(),
        [threadId]: messages,
      });
    } finally {
      this.isLoadingMessages.set(false);
    }
  }

  async onSendMessage(text: string): Promise<void> {
    const threadId = this.selectedThreadId();
    const thread = this.selectedThread();
    if (!threadId || !thread) return;

    if (this.signalr.status() !== SIGNALR_STATUS.CONNECTED) {
      console.error('Chat: SignalR nije povezan, ne može se poslati poruka.');
      this.realtimeError.set(
        this.signalr.lastError() ?? 'Niste povezani. Pokušajte ponovo.'
      );
      return;
    }

    const conversationId = threadId;
    const jobId = thread.jobId || this.emptyJobId;

    const optId = `opt-${Date.now()}`;
    const sentAt = new Date().toISOString();
    const optimisticMsg: ChatMessage = {
      id: optId,
      from: 'me',
      text,
      time: this.formatTimeOnly(sentAt),
      sentAt,
    };
    const current = this.messagesByThread()[conversationId] ?? [];
    this.messagesByThread.set({
      ...this.messagesByThread(),
      [conversationId]: [...current, optimisticMsg],
    });

    try {
      await this.signalr.invoke('SendMessage', conversationId, jobId, text);
      this.realtimeError.set(null);
    } catch (err) {
      console.error('Chat SendMessage failed:', err);
      this.realtimeError.set(
        err instanceof Error ? err.message : 'Poruka nije poslata.'
      );
    }
  }
}
