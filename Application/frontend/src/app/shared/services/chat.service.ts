import { Injectable } from '@angular/core';
import type { ChatMessage } from '../../components/chat-panel/chat-panel.component';

export type ChatPresence = 'online' | 'offline' | 'typing';

export type ChatThread = {
  id: string;
  title: string; // npr: "Milan • Električar"
  subtitle: string; // npr: "Novi Sad • dostupan danas posle 17h"
  lastMessage: string;
  updatedAt: string; // "12:41"
  unreadCount: number;
  presence: ChatPresence;
};

@Injectable({ providedIn: 'root' })
export class ChatService {
  // Mock “baza” – kasnije ovo zamenjuješ HTTP pozivima ka backend-u.
  private threads: ChatThread[] = [
    {
      id: 't1',
      title: 'Milan • Električar',
      subtitle: 'Novi Sad • dostupan danas posle 17h',
      lastMessage: 'Pošalji sliku i adresu — mogu danas posle 17h.',
      updatedAt: '12:20',
      unreadCount: 2,
      presence: 'typing',
    },
    {
      id: 't2',
      title: 'Jelena • Vodoinstalater',
      subtitle: 'Beograd • sutra ujutru',
      lastMessage: 'Može, dođem sutra između 9-11h.',
      updatedAt: '09:12',
      unreadCount: 0,
      presence: 'online',
    },
  ];

  private messagesByThread: Record<string, ChatMessage[]> = {
    t1: [
      { id: 'm1', from: 'them', text: 'Zdravo! Šta treba da se popravi?', time: '12:18' },
      { id: 'm2', from: 'me', text: 'Treba da zamenim utičnicu, stalno varniči.', time: '12:19' },
      { id: 'm3', from: 'them', text: 'Može. Pošalji sliku i adresu — mogu danas posle 17h.', time: '12:20' },
    ],
    t2: [
      { id: 'm10', from: 'me', text: 'Curi slavina u kuhinji.', time: '09:10' },
      { id: 'm11', from: 'them', text: 'Može, dođem sutra između 9-11h.', time: '09:12' },
    ],
  };

  async getThreads(): Promise<ChatThread[]> {
    await this.simulateLatency(220);
    // realno: GET /threads
    return [...this.threads];
  }

  async getMessages(threadId: string): Promise<ChatMessage[]> {
    await this.simulateLatency(220);
    // realno: GET /threads/:id/messages
    return [...(this.messagesByThread[threadId] ?? [])];
  }

  async sendMessage(threadId: string, text: string): Promise<ChatMessage> {
    await this.simulateLatency(120);
    // realno: POST /threads/:id/messages
    const msg: ChatMessage = {
      id: `m_${Date.now()}`,
      from: 'me',
      text,
      time: this.formatTime(new Date()),
    };

    const list = this.messagesByThread[threadId] ?? [];
    this.messagesByThread[threadId] = [...list, msg];

    // update preview
    this.threads = this.threads.map((t) =>
      t.id === threadId ? { ...t, lastMessage: text, updatedAt: msg.time, unreadCount: 0 } : t,
    );

    return msg;
  }

  markRead(threadId: string): void {
    this.threads = this.threads.map((t) => (t.id === threadId ? { ...t, unreadCount: 0 } : t));
  }

  // Samo helperi (simulacija)
  private async simulateLatency(ms: number): Promise<void> {
    await new Promise((r) => setTimeout(r, ms));
  }

  private formatTime(d: Date): string {
    const hh = String(d.getHours()).padStart(2, '0');
    const mm = String(d.getMinutes()).padStart(2, '0');
    return `${hh}:${mm}`;
  }
}

