import {
  Component,
  ElementRef,
  ViewChild,
  computed,
  input,
  output,
  signal,
} from '@angular/core';
import type { SignalrStatus } from '../../shared/types';
import { ButtonComponent } from '../button/button.component';
import { BUTTON_TYPES } from '../../shared/types';

type ChatPresence = 'online' | 'offline' | 'typing';

export type ChatMessage = {
  id: string;
  from: 'me' | 'them' | 'system';
  text: string;
  time: string;
};

@Component({
  selector: 'app-chat-panel',
  templateUrl: './chat-panel.component.html',
  styleUrl: './chat-panel.component.scss',
  imports: [ButtonComponent],
})
export class ChatPanelComponent {
  eButtonType = BUTTON_TYPES;
  title = input<string>('Chat sa majstorom');
  subtitle = input<string>('Dogovorite termin i cenu u par poruka.');

  // Ako želiš da vežeš na SignalR status iz parent-a:
  signalrStatus = input<SignalrStatus | null>(null);

  // Lagan “presence” indikator za UI (nezavisno od SignalR-a)
  presence = input<ChatPresence>('online');

  // Ako parent prosledi poruke, komponenta radi kao “viewer” + emituje send.
  messages = input<ChatMessage[] | null>(null);
  send = output<string>();

  draft = signal<string>('');

  private internalMessages = signal<ChatMessage[]>([
    {
      id: 'm1',
      from: 'them',
      text: 'Zdravo! Šta treba da se popravi?',
      time: '12:18',
    },
    {
      id: 'm2',
      from: 'me',
      text: 'Treba da zamenim utičnicu, stalno varniči.',
      time: '12:19',
    },
    {
      id: 'm3',
      from: 'them',
      text: 'Može. Pošalji sliku i adresu — mogu danas posle 17h.',
      time: '12:20',
    },
  ]);

  displayMessages = computed(() => this.messages() ?? this.internalMessages());
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
      const msg: ChatMessage = {
        id: `m_${Date.now()}`,
        from: 'me',
        text,
        time: this.formatTime(new Date()),
      };
      this.internalMessages.set([...this.internalMessages(), msg]);
      this.scrollToBottomSoon();

      // Demo “reply” da UI deluje življe (možeš da obrišeš kad povežeš backend)
      window.setTimeout(() => {
        const reply: ChatMessage = {
          id: `m_${Date.now() + 1}`,
          from: 'them',
          text: 'Važi — mogu da donesem novu utičnicu, cena zavisi od modela.',
          time: this.formatTime(new Date()),
        };
        this.internalMessages.set([...this.internalMessages(), reply]);
        this.scrollToBottomSoon();
      }, 650);
    } else {
      this.scrollToBottomSoon();
    }
  }

  private scrollToBottomSoon(): void {
    queueMicrotask(() => {
      const el = this.scrollViewport?.nativeElement;
      if (!el) return;
      el.scrollTop = el.scrollHeight;
    });
  }

  private formatTime(d: Date): string {
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  }

  public onButtonClick(event: MouseEvent): void {
    this.onSend();
  }
}
