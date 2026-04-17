export interface ReceiveMessagePayload {
  id: string;
  conversationId: string;
  jobId: string | null;
  senderId: string;
  content: string;
  sentAt: string;
}
