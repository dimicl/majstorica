import { UserRole } from '../enums/user-role.enum';

/** Samo Master u UI (Zahtevi, majstorski profil). CompanyWorker privremeno isključen. */
export function isMasterLikeUserRole(role: unknown): boolean {
  if (role === null || role === undefined) return false;
  if (typeof role === 'number') return role === 2; // || role === 4; // CompanyWorker
  const s = String(role).trim();
  return s === UserRole.Master; // || s === UserRole.CompanyWorker;
}

export function isClientUserRole(role: unknown): boolean {
  if (role === null || role === undefined) return false;
  if (typeof role === 'number') return role === 1;
  const s = String(role).trim();
  return s === UserRole.Client;
}
