import { Injectable, inject, NgZone, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { API_BASE_URL } from '../constants/api.constants';
import type { ChatMessage } from '../interfaces/chat-message.interface';
import type { ChatThread } from '../interfaces/chat-thread.interface';
import type { ChatPresence } from '../types/chat-presence.type';
import type { ConversationListItemApi } from '../interfaces/conversation-list-item-api.interface';
import type { ChatMessageApi } from '../interfaces/chat-message-api.interface';
import type { ReceiveMessagePayload } from '../interfaces/receive-message-payload.interface';
import { SignalrService } from './signalr.service';

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private signalr = inject(SignalrService);
  private ngZone = inject(NgZone);
  private realtimeHandlersRegistered = false;

  async getThreads(): Promise<ChatThread[]> {
    const list = await firstValueFrom(
      this.http.get<ConversationListItemApi[]>(`${API_BASE_URL}/conversations`)
    );
    const emptyJobId = '00000000-0000-0000-0000-000000000000';
    return list.map((c) => ({
      id: c.id,
      jobId: c.jobId,
      title: c.otherPartyName,
      subtitle:
        c.jobId === emptyJobId ? 'Razgovor' : c.jobDescription ?? 'Posao',
      lastMessage: c.lastMessageText ?? '',
      updatedAt: this.formatTimeOrDate(c.lastMessageAt),
      unreadCount: c.unreadCount ?? c.UnreadCount ?? 0,
      presence:
        c.isOnline ?? c.IsOnline ?? false
          ? ('online' as ChatPresence)
          : ('offline' as ChatPresence),
    }));
  }

  /** Da li korisnik ima bar jednu nepročitanu poruku (za indikator u navbar-u). */
  async hasUnreadMessages(): Promise<boolean> {
    try {
      const threads = await this.getThreads();
      return threads.some((t) => t.unreadCount > 0);
    } catch {
      return false;
    }
  }

  /** Signal za navbar – ima li nepročitanih poruka. Pozovi refreshUnreadIndicator() da osvežiš. */
  hasNewMessages = signal<boolean>(false);

  /** Osveži indikator nepročitanih (pozovi npr. nakon markRead ili pri navigaciji). */
  async refreshUnreadIndicator(): Promise<void> {
    if (!this.auth.getToken()) {
      this.hasNewMessages.set(false);
      return;
    }
    const has = await this.hasUnreadMessages();
    this.hasNewMessages.set(has);
  }

  /** Postavi indikator nepročitanih (npr. kad stigne nova poruka preko SignalR – bez API poziva). */
  setHasNewMessages(value: boolean): void {
    this.hasNewMessages.set(value);
  }

  /**
   * Registruje handler za ReceiveMessage da crveni krug (navbar) prikaže nepročitano
   * čim stigne poruka od nekog drugog – radi i kad korisnik nije na chat stranici.
   * Poziva se iz auth effects nakon SignalR connect.
   */
  registerRealtimeHandlers(): void {
    if (this.realtimeHandlersRegistered) return;
    this.realtimeHandlersRegistered = true;
    this.signalr.on<ReceiveMessagePayload>('ReceiveMessage', (payload) => {
      const me = this.auth.getUserIdFromStorage() ?? '';
      const senderId = payload.senderId ?? payload.SenderId ?? '';
      if (senderId && senderId !== me) {
        this.ngZone.run(() => this.setHasNewMessages(true));
      }
    });
  }

  /** Poziva se pri logout da se handler ponovo registruje pri sledećem login-u. */
  clearRealtimeHandlers(): void {
    this.realtimeHandlersRegistered = false;
  }

  /** Jedna konverzacija po id (za ime/prezime kada otvaramo po open=). */
  async getConversation(
    conversationId: string
  ): Promise<ConversationListItemApi | null> {
    try {
      return await firstValueFrom(
        this.http.get<ConversationListItemApi>(`${API_BASE_URL}/conversations/${conversationId}`)
      );
    } catch {
      return null;
    }
  }

  /** Otvori ili nastavi slobodan chat sa majstorom (bez posla). Vraća id konverzacije. */
  async startConversationWithMaster(masterId: string): Promise<{ id: string }> {
    const res = await firstValueFrom(
      this.http.post<{ id: string }>(
        `${API_BASE_URL}/conversations/with-master/${masterId}`,
        {}
      )
    );
    return res;
  }

  async getMessages(conversationId: string): Promise<ChatMessage[]> {
    const currentUserId = this.auth.getUserIdFromStorage() ?? '';
    const list = await firstValueFrom(
      this.http.get<ChatMessageApi[]>(
        `${API_BASE_URL}/conversations/${conversationId}/messages`
      )
    );
    return list.map((m) => ({
      id: m.id,
      from: m.isSystemMessage
        ? ('system' as const)
        : m.senderId === currentUserId
          ? ('me' as const)
          : ('them' as const),
      text: m.content,
      time: this.formatTimeOnly(m.sentAt),
      sentAt: m.sentAt,
    }));
  }

  async markRead(conversationId: string): Promise<void> {
    try {
      await firstValueFrom(
        this.http.post<void>(`${API_BASE_URL}/conversations/${conversationId}/read`, {})
      );
    } catch {
      // ignorišemo greške (npr. offline)
    }
  }

  /** Samo vreme (HH:mm) za poruke u chatu */
  private formatTimeOnly(iso: string): string {
    const d = new Date(iso);
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  }

  /** Danas: vreme (HH:mm), inače: datum (dd.MM.yyyy.) – za thread listu */
  private formatTimeOrDate(isoOrNull: string | null): string {
    if (!isoOrNull) return '--:--';
    const d = new Date(isoOrNull);
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
}
