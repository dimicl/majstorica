import {
  Component,
  ElementRef,
  ViewChild,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import type { SignalrStatus, ChatPresence } from '../../shared/types';
import type { ChatMessage } from '../../shared/interfaces';
import { ButtonComponent } from '../button/button.component';
import { BUTTON_TYPES } from '../../shared/types';

@Component({
  selector: 'app-chat-panel',
  templateUrl: './chat-panel.component.html',
  styleUrl: './chat-panel.component.scss',
  imports: [ButtonComponent],
})
export class ChatPanelComponent {
  eButtonType = BUTTON_TYPES;
  title = input<string>('Chat sa majstorom');

  // Ako želiš da vežeš na SignalR status iz parent-a:
  signalrStatus = input<SignalrStatus | null>(null);

  // Lagan “presence” indikator za UI (nezavisno od SignalR-a)
  presence = input<ChatPresence>('online');

  // Ako parent prosledi poruke, komponenta radi kao “viewer” + emituje send.
  messages = input<ChatMessage[] | null>(null);
  send = output<string>();

  draft = signal<string>('');

  private internalMessages = signal<ChatMessage[]>([]);

  displayMessages = computed(() => this.messages() ?? this.internalMessages());

  /** Grupe poruka po datumu: iznad svake grupe prikaže se datum, ispod poruke sa vremenom */
  messageGroups = computed(() => {
    const list = this.displayMessages();
    if (!list.length) return [];
    const now = new Date();
    const todayKey = this.toDateKey(now);
    const groups = new Map<
      string,
      { dateKey: string; dateLabel: string; messages: ChatMessage[] }
    >();
    for (const m of list) {
      const iso = m.sentAt ?? new Date().toISOString();
      const d = new Date(iso);
      const dateKey = this.toDateKey(d);
      if (!groups.has(dateKey)) {
        groups.set(dateKey, {
          dateKey,
          dateLabel: this.formatDateLabel(d, now),
          messages: [],
        });
      }
      groups.get(dateKey)!.messages.push(m);
    }
    return Array.from(groups.values());
  });

  usingExternalMessages = computed(() => this.messages() !== null);

  @ViewChild('scrollViewport') scrollViewport?: ElementRef<HTMLElement>;

  onDraftInput(value: string): void {
    this.draft.set(value);
  }

  onKeyDown(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      this.onSend();
    }
  }

  onSend(): void {
    const text = this.draft().trim();
    if (!text) return;

    this.draft.set('');
    this.send.emit(text);

    if (!this.usingExternalMessages()) {
      const sentAt = new Date().toISOString();
      const msg: ChatMessage = {
        id: `m_${Date.now()}`,
        from: 'me',
        text,
        time: this.formatTimeOnly(sentAt),
        sentAt,
      };
      this.internalMessages.set([...this.internalMessages(), msg]);
    }
    this.scrollToBottomSoon();
  }

  private scrollToBottomSoon(): void {
    queueMicrotask(() => {
      const el = this.scrollViewport?.nativeElement;
      if (!el) return;
      el.scrollTop = el.scrollHeight;
    });
  }

  private toDateKey(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  /** Zaglavlje iznad grupe poruka: dd.MM.yyyy. */
  private formatDateLabel(d: Date, now: Date): string {
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}.${month}.${year}.`;
  }

  /** Samo vreme (HH:mm) na balonu poruke */
  private formatTimeOnly(iso: string): string {
    const d = new Date(iso);
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  }

  public onButtonClick(event: MouseEvent): void {
    this.onSend();
  }
}
