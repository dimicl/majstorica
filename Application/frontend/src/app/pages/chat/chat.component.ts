import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  ChatPanelComponent,
  type ChatMessage,
} from '../../components/chat-panel/chat-panel.component';
import {
  ChatService,
  type ChatThread,
} from '../../shared/services/chat.service';
import { SignalrService } from '../../shared/services/signalr.service';
import { AuthService } from '../../shared/services/auth.service';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES, SIGNALR_STATUS } from '../../shared/types';
import { CommonModule } from '@angular/common';

const HUB_URL = 'http://localhost:5187/hubs/document';

interface ReceiveMessagePayload {
  id: string;
  conversationId: string;
  jobId: string;
  senderId: string;
  content: string;
  sentAt: string;
}

@Component({
  selector: 'app-chat',
  imports: [RouterLink, ChatPanelComponent, ButtonComponent, CommonModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss',
})
export class ChatComponent {
  private chat = inject(ChatService);
  private signalr = inject(SignalrService);
  private auth = inject(AuthService);
  private destroyRef = inject(DestroyRef);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  realtimeError = this.signalr.lastError;

  //enums
  public eButtonType = BUTTON_TYPES;
  isLoadingThreads = signal<boolean>(true);
  threads = signal<ChatThread[]>([]);
  selectedThreadId = signal<string | null>(null);

  isLoadingMessages = signal<boolean>(false);
  messagesByThread = signal<Record<string, ChatMessage[]>>({});

  selectedThread = computed(() => {
    const id = this.selectedThreadId();
    if (!id) return null;
    return this.threads().find((t) => t.id === id) ?? null;
  });

  selectedMessages = computed(() => {
    const id = this.selectedThreadId();
    if (!id) return null;
    return this.messagesByThread()[id] ?? null;
  });

  constructor() {
    const token = this.auth.getToken();
    // accessTokenFactory je obavezan: WebSocketTransport dodaje access_token u URL samo ako postoji factory.
    // Bez njega negotiate može proći (Authorization header), ali WebSocket se otvara bez tokena → Unauthorized.
    const options = token
      ? { accessTokenFactory: () => this.auth.getToken() ?? '' }
      : undefined;
    void this.signalr.connect(HUB_URL, options);
    this.destroyRef.onDestroy(() => {
      void this.signalr.disconnect();
    });

    this.signalr.on<ReceiveMessagePayload>('ReceiveMessage', (payload) => {
      this.handleReceiveMessage(payload);
    });

    void this.loadThreads();
  }

  private handleReceiveMessage(payload: ReceiveMessagePayload): void {
    const currentUserId = this.auth.getUserIdFromStorage() ?? '';
    const isFromMe = payload.senderId === currentUserId;
    const msg: ChatMessage = {
      id: payload.id,
      from: isFromMe ? 'me' : 'them',
      text: payload.content,
      time: this.formatTime(payload.sentAt),
    };
    const convId = payload.conversationId;
    let current = this.messagesByThread()[convId] ?? [];
    // Ako stigne naša poruka sa servera, ukloni optimističku sa istim tekstom da ne bude duplikat
    if (isFromMe) {
      current = current.filter(
        (m) => !(String(m.id).startsWith('opt-') && m.text === payload.content)
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
        lastMessage: payload.content,
        updatedAt: this.formatTime(payload.sentAt),
        unreadCount: t.unreadCount + addUnread,
      };
      this.threads.set(
        threads.slice(0, idx).concat(updated, threads.slice(idx + 1))
      );
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

  private formatTime(iso: string): string {
    const d = new Date(iso);
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  }

  /** Minimalni thread kada otvaramo samo po open= u URL (nema u listi). */
  private threadPlaceholder(conversationId: string): ChatThread {
    return {
      id: conversationId,
      jobId: '00000000-0000-0000-0000-000000000000',
      title: 'Razgovor',
      subtitle: 'Razgovor',
      lastMessage: '',
      updatedAt: '--:--',
      unreadCount: 0,
      presence: 'offline',
    };
  }

  /** Osveži listu threadova (npr. da se ažurira online/offline). */
  async refreshThreads(): Promise<void> {
    const list = await this.chat.getThreads();
    this.threads.set(list);
  }

  /** Ažuriraj thread sa imenom/prezimenom iz GET /api/conversations/:id. */
  private async applyConversationDetails(
    conversationId: string
  ): Promise<void> {
    const conv = await this.chat.getConversation(conversationId);
    if (!conv) return;
    const emptyJobId = '00000000-0000-0000-0000-000000000000';
    const list = this.threads();
    const idx = list.findIndex((t) => t.id === conversationId);
    if (idx === -1) return;
    const updated: ChatThread = {
      ...list[idx],
      title: conv.otherPartyName || 'Razgovor',
      subtitle:
        conv.jobId === emptyJobId ? 'Razgovor' : conv.jobDescription ?? 'Posao',
      lastMessage: conv.lastMessageText ?? '',
      updatedAt: conv.lastMessageAt
        ? this.formatTime(conv.lastMessageAt)
        : '--:--',
      unreadCount: conv.unreadCount ?? list[idx].unreadCount,
      presence: conv.isOnline ?? conv.IsOnline ?? false ? 'online' : 'offline',
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
      // Bez open= ne biramo nijedan chat – korisnik vidi listu sa brojem nepročitanih i bira sam
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
    // Lokalno resetuj badge odmah
    this.setThreadUnread(threadId, 0);
    void this.chat.refreshUnreadIndicator();

    try {
      await this.signalr.invoke('JoinConversation', threadId);
    } catch {
      // Hub može da odbije ako nisi autorizovan
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
    const jobId = thread.jobId;

    // Optimistički prikaži poruku odmah
    const optId = `opt-${Date.now()}`;
    const optimisticMsg: ChatMessage = {
      id: optId,
      from: 'me',
      text,
      time: this.formatTime(new Date().toISOString()),
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
      // Poruku ostavljamo u listi da korisnik vidi šta nije poslato (može ponovo da pošalje)
    }
  }
}
