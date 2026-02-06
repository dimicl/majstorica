import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ChatPanelComponent,
  type ChatMessage,
} from '../../components/chat-panel/chat-panel.component';
import {
  ChatService,
  type ChatThread,
} from '../../shared/services/chat.service';
import { SignalrService } from '../../shared/services/signalr.service';

@Component({
  selector: 'app-chat',
  imports: [RouterLink, ChatPanelComponent],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss',
})
export class ChatComponent {
  private chat = inject(ChatService);
  private signalr = inject(SignalrService);
  private destroyRef = inject(DestroyRef);

  // Ne prikazujemo “SignalR” korisniku — samo generičan indikator ako chat nije dostupan
  realtimeError = this.signalr.lastError;

  // Hub URL ka backend DocumentHub
  hubUrl = signal<string>('http://localhost:5187/hubs/document');

  // ============================
  // “Backend-like” state (mock)
  // ============================
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
    // Konekcija “ispod haube” — UI ne prikazuje kontrole
    void this.signalr.connect(this.hubUrl());
    this.destroyRef.onDestroy(() => {
      void this.signalr.disconnect();
    });

    void this.loadThreads();
  }

  async loadThreads(): Promise<void> {
    this.isLoadingThreads.set(true);
    try {
      const threads = await this.chat.getThreads();
      this.threads.set(threads);

      // Auto-select prvi thread ako postoji (UX: odmah vidi chat)
      if (!this.selectedThreadId() && threads.length > 0) {
        await this.selectThread(threads[0].id);
      }
    } finally {
      this.isLoadingThreads.set(false);
    }
  }

  async selectThread(threadId: string): Promise<void> {
    this.selectedThreadId.set(threadId);
    this.chat.markRead(threadId);
    this.threads.set([...this.threads()]); // refresh unreadCount u UI

    // Ako poruke već imamo, ne vučemo opet
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
    if (!threadId) return;

    const msg = await this.chat.sendMessage(threadId, text);
    const current = this.messagesByThread()[threadId] ?? [];
    this.messagesByThread.set({
      ...this.messagesByThread(),
      [threadId]: [...current, msg],
    });

    // refresh preview (lastMessage/updatedAt) iz mock “backend-a”
    const fresh = await this.chat.getThreads();
    this.threads.set(fresh);
  }
}
