export interface ChatMessage {
  id: string;
  from: 'me' | 'them' | 'system';
  text: string;
  time: string;
  /** ISO string – opciono, za sortiranje ili prikaz */
  sentAt?: string;
}
