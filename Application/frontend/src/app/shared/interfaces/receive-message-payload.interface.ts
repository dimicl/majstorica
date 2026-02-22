/** Payload od SignalR – backend može slati camelCase ili PascalCase */
export interface ReceiveMessagePayload {
  id?: string;
  Id?: string;
  conversationId?: string;
  ConversationId?: string;
  jobId?: string;
  JobId?: string;
  senderId?: string;
  SenderId?: string;
  content?: string;
  Content?: string;
  sentAt?: string;
  SentAt?: string;
}
