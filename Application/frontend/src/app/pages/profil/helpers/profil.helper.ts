import { UserRole } from '../../../shared/enums/user-role.enum';
import type { NewJobRequestPayload } from '../../../shared/interfaces';
import type { CompanyWorkerMember } from '../../../shared/interfaces/company.interface';
import type { MasterProfileResponse } from '../../../shared/models/master.model';
import type { JobListItem } from '../../../shared/services/job.service';

export const PROFIL_SIGNALR_HUB_URL =
  'http://localhost:5187/hubs/document';

export type MasterStatKind = 'experience' | 'hourly';

export type MasterProfileVm = {
  category: string | null;
  rating: number | null;
  yearsOfExperience: number;
  hourlyRateAmount: number;
  hourlyRateCurrency: string;
  totalReviews: number;
};

export type AuthPatchFromProfile = {
  category: string | null;
  employerCompanyName: string | null;
};

const JOB_STATUS_LABELS: Record<string, string> = {
  Created: 'Kreiran',
  Pending: 'Na čekanju',
  Accepted: 'Prihvaćen',
  InProgress: 'U toku',
  Completed: 'Završen',
};

export function normalizeMasterUserId(id: string): string {
  return (id ?? '').trim().toLowerCase();
}

export function jobStatusLabel(status: string): string {
  return JOB_STATUS_LABELS[status] ?? status;
}

export function workerCategoriesLine(w: CompanyWorkerMember): string {
  const c = w.serviceCategories;
  return c?.length ? c.join(', ') : '—';
}

export function workerZonesLine(w: CompanyWorkerMember): string {
  const z = w.serviceZones;
  return z?.length ? z.join(', ') : '—';
}

export function masterProfileVmFromResponse(
  res: MasterProfileResponse
): MasterProfileVm {
  return {
    category: res.category ?? null,
    rating: res.rating ?? null,
    yearsOfExperience: res.yearsOfExperience ?? 0,
    hourlyRateAmount: res.hourlyRateAmount ?? 0,
    hourlyRateCurrency: (res.hourlyRateCurrency ?? 'RSD').trim() || 'RSD',
    totalReviews: res.totalReviews ?? 0,
  };
}

export function authPatchFromMasterProfile(
  res: MasterProfileResponse
): AuthPatchFromProfile {
  return {
    category: res.category ?? null,
    employerCompanyName: res.employerCompanyName?.trim()
      ? res.employerCompanyName
      : null,
  };
}

export const AUTH_PATCH_CLEARED: AuthPatchFromProfile = {
  category: null,
  employerCompanyName: null,
};

export function buildPendingJobFromNewJobRequest(
  p: NewJobRequestPayload
): JobListItem | null {
  const jobId = p.jobId ?? '';
  const conversationId = p.conversationId ?? '';
  if (!jobId || !conversationId) return null;
  const jobTitle = p.jobTitle ?? '';
  const description = p.description ?? '';
  const date = p.date ?? new Date().toISOString();
  const clientName = p.clientName ?? 'Klijent';
  const clientId = p.clientId ?? '';
  const price = p.price ?? null;
  const isEmergency = p.isEmergency ?? false;
  const now = new Date().toISOString();
  return {
    jobId,
    conversationId,
    jobTitle,
    description,
    serviceCategory: p.serviceCategory ?? null,
    clientName,
    masterName: null,
    date,
    clientId,
    price,
    isEmergency,
    status: 'Pending',
    createdAt: now,
    updatedAt: now,
  };
}

export type ProfileRoleHandlers = {
  onMaster: () => void;
  onCompanyWorker: () => void;
  onClient: () => void;
  onCompanyOwner: () => void;
  onDefault: () => void;
};

export function runProfileRoleActions(
  role: UserRole | undefined,
  h: ProfileRoleHandlers
): void {
  switch (role) {
    case UserRole.Master:
      h.onMaster();
      return;
    case UserRole.CompanyWorker:
      h.onCompanyWorker();
      return;
    case UserRole.Client:
      h.onClient();
      return;
    case UserRole.CompanyOwner:
      h.onCompanyOwner();
      return;
    default:
      h.onDefault();
  }
}
