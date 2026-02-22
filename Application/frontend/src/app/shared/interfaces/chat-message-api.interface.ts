/** Odgovor API-ja za poruke u konverzaciji. */
export interface ChatMessageApi {
  id: string;
  conversationId: string;
  jobId: string;
  senderId: string;
  content: string;
  sentAt: string;
  isSystemMessage?: boolean;
}
