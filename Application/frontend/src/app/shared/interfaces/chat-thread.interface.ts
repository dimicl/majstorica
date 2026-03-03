import { ChatPresence } from '../types/chat-presence.type';

export interface ChatThread {
  id: string;
  jobId: string;
  title: string;
  subtitle: string;
  lastMessage: string;
  updatedAt: string;
  unreadCount: number;
  presence: ChatPresence;
  /** Formatiran tekst za prikaz "Poslednje aktivan: ..." kada je offline. */
  lastSeenText?: string;
}
