import type { JobListItem } from '../services/job.service';
import type { NewJobRequestPayload } from '../interfaces/new-job-request-payload.interface';

// -----------------------------------------------------------------------------
// Types
// -----------------------------------------------------------------------------

export interface CalendarDay {
  date: Date;
  dayKey: string;
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
  count: number;
}

export interface ChartBar {
  label: string;
  value: number;
  dayKey: string;
}

export interface RequestsStats {
  total: number;
  thisMonth: number;
  emergency: number;
}

// -----------------------------------------------------------------------------
// Constants
// -----------------------------------------------------------------------------

export const REQUESTS_WEEKDAYS = [
  'Ned',
  'Pon',
  'Uto',
  'Sre',
  'Čet',
  'Pet',
  'Sub',
];

const MONTH_NAMES = [
  'Januar',
  'Februar',
  'Mart',
  'April',
  'Maj',
  'Jun',
  'Jul',
  'Avgust',
  'Septembar',
  'Oktobar',
  'Novembar',
  'Decembar',
];

export function toDayKey(d: string | Date): string {
  if (typeof d === 'string') {
    const match = d.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (match) {
      const [, y, m, day] = match;
      return `${y}-${m}-${day}`;
    }
    const date = new Date(d);
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

export function getMonthLabel(date: Date): string {
  return `${MONTH_NAMES[date.getMonth()]} ${date.getFullYear()}`;
}

export function prevMonthDate(date: Date): Date {
  const d = new Date(date);
  d.setMonth(d.getMonth() - 1);
  return d;
}

export function nextMonthDate(date: Date): Date {
  const d = new Date(date);
  d.setMonth(d.getMonth() + 1);
  return d;
}

/** Dan u mesecu za prikaz u kalendaru (sa padding danima pre/posle). */
export function buildCalendarDays(
  month: Date,
  requests: JobListItem[]
): CalendarDay[] {
  const dayKeyToCount = new Map<string, number>();
  for (const r of requests) {
    const key = toDayKey(r.date);
    dayKeyToCount.set(key, (dayKeyToCount.get(key) ?? 0) + 1);
  }

  const year = month.getFullYear();
  const m = month.getMonth();
  const first = new Date(year, m, 1);
  const last = new Date(year, m + 1, 0);
  const startPad = first.getDay();
  const daysInMonth = last.getDate();
  const totalCells =
    startPad + daysInMonth + ((42 - ((startPad + daysInMonth) % 7)) % 7) || 42;
  const result: CalendarDay[] = [];
  const todayKey = toDayKey(new Date());

  for (let i = 0; i < totalCells; i++) {
    const dayOffset = i - startPad;
    const date = new Date(year, m, 1 + dayOffset);
    const dayKey = toDayKey(date);
    const isCurrentMonth = date.getMonth() === m;
    result.push({
      date,
      dayKey,
      day: date.getDate(),
      isCurrentMonth,
      isToday: dayKey === todayKey,
      count: isCurrentMonth ? dayKeyToCount.get(dayKey) ?? 0 : 0,
    });
  }
  return result;
}

/** Za toggle izbora dana: ako je već izabran taj dan, vrati null, inače dayKey. */
export function toggleDaySelection(
  currentSelected: string | null,
  dayKey: string
): string | null {
  return currentSelected === dayKey ? null : dayKey;
}

// -----------------------------------------------------------------------------
// Requests grouping & stats
// -----------------------------------------------------------------------------

export function groupRequestsByDayKey(
  requests: JobListItem[]
): Map<string, JobListItem[]> {
  const map = new Map<string, JobListItem[]>();
  for (const r of requests) {
    const key = toDayKey(r.date);
    if (!map.has(key)) map.set(key, []);
    map.get(key)!.push(r);
  }
  return map;
}

/** Zahtevi za izabrani dan ili svi ako selectedDate null. */
export function getSelectedDayRequests(
  requests: JobListItem[],
  byDay: Map<string, JobListItem[]>,
  selectedDate: string | null
): JobListItem[] {
  if (!selectedDate) return requests;
  return byDay.get(selectedDate) ?? [];
}

export function computeStats(requests: JobListItem[]): RequestsStats {
  const now = new Date();
  const monthStart = new Date(now.getFullYear(), now.getMonth(), 1);
  const monthEnd = new Date(now.getFullYear(), now.getMonth() + 1, 0);
  let thisMonth = 0;
  let emergency = 0;
  for (const r of requests) {
    const d = new Date(r.date);
    if (d >= monthStart && d <= monthEnd) thisMonth++;
    if (r.isEmergency) emergency++;
  }
  return {
    total: requests.length,
    thisMonth,
    emergency,
  };
}

export function buildChartData(
  byDay: Map<string, JobListItem[]>,
  daysRange = 3
): ChartBar[] {
  const result: ChartBar[] = [];
  const today = new Date();
  for (let i = -daysRange; i <= daysRange; i++) {
    const d = new Date(today);
    d.setDate(d.getDate() + i);
    const dayKey = toDayKey(d);
    const count = byDay.get(dayKey)?.length ?? 0;
    const label = d.getDate() + '.' + (d.getMonth() + 1) + '.';
    result.push({ label, value: count, dayKey });
  }
  return result;
}

// -----------------------------------------------------------------------------
// Payload & list updates
// -----------------------------------------------------------------------------

export function jobListItemFromPayload(
  p: NewJobRequestPayload
): JobListItem | null {
  const jobId = p.jobId ?? '';
  const conversationId = p.conversationId ?? '';
  if (!jobId || !conversationId) return null;

  const now = new Date().toISOString();
  return {
    jobId,
    conversationId,
    jobTitle: p.jobTitle ?? '',
    description: p.description ?? '',
    clientName: p.clientName ?? 'Klijent',
    masterName: null,
    date: p.date ?? now,
    clientId: p.clientId ?? '',
    price: p.price ?? null,
    isEmergency: p.isEmergency ?? false,
    status: 'Pending',
    createdAt: now,
    updatedAt: now,
  };
}

/** Dodaje novi zahtev iz payload-a u listu ako ga već nema. Vraća novu listu. */
export function mergeRequestFromPayload(
  list: JobListItem[],
  payload: NewJobRequestPayload
): JobListItem[] {
  const item = jobListItemFromPayload(payload);
  if (!item) return list;
  if (list.some((r) => r.conversationId === item.conversationId)) return list;
  return [...list, item];
}

export function removeRequestByJobId(
  list: JobListItem[],
  jobId: string
): JobListItem[] {
  return list.filter((r) => r.jobId !== jobId);
}

export function removeRequestByConversationId(
  list: JobListItem[],
  conversationId: string
): JobListItem[] {
  return list.filter((r) => r.conversationId !== conversationId);
}
