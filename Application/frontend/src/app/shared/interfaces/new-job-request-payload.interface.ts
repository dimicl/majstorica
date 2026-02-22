/** Payload od SignalR NewJobRequest (backend šalje camelCase). */
export interface NewJobRequestPayload {
  jobId?: string;
  conversationId?: string;
  jobTitle?: string;
  description?: string;
  date?: string;
  clientName?: string;
  clientId?: string;
  price?: number | null;
  isEmergency?: boolean;
}
