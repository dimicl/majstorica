/** Odgovor API-ja za listu konverzacija / jednu konverzaciju. */
export interface ConversationListItemApi {
  id: string;
  jobId: string;
  jobDescription: string | null;
  otherPartyName: string;
  otherPartyId: string;
  lastMessageText: string | null;
  lastMessageAt: string | null;
  isActive: boolean;
  /** Backend šalje camelCase (unreadCount); podrška i za PascalCase (UnreadCount) */
  unreadCount?: number;
  UnreadCount?: number;
  isOnline?: boolean;
  IsOnline?: boolean;
  /** ISO string – poslednja aktivnost drugog učesnika (za "Poslednje aktivan: ..."). */
  otherPartyLastSeen?: string | null;
  OtherPartyLastSeen?: string | null;
}
