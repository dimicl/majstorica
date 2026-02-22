export interface ReceiveMessagePayload {
  id: string;
  conversationId: string;
  jobId: string;
  senderId: string;
  content: string;
  sentAt: string;
}
