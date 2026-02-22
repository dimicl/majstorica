export interface ChatMessage {
  id: string;
  from: 'me' | 'them' | 'system';
  text: string;
  time: string;
}
