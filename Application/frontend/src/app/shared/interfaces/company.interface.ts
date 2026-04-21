export interface CompanyDto {
  id: string;
  name: string;
  description?: string | null;
  phoneNumber: string;
  email: string;
  ownerUserId: string;
}

/** GET api/companies/{id}/public */
export interface CompanyPublicDto {
  id: string;
  /** Vlasnik firme – za chat i kreiranje posla. */
  ownerUserId: string;
  name: string;
  description?: string | null;
  phoneNumber: string;
  email: string;
  city?: string | null;
  serviceCategories: string[];
}

export interface CreateCompanyPayload {
  name: string;
  phoneNumber: string;
  email: string;
  street?: string | null;
  city?: string | null;
}

/** Rezultat pretrage majstora za poziv u firmu (vlasnik firme). */
export interface MasterSearchForInviteItem {
  userId: string;
  firstName: string;
  lastName: string;
  username: string;
  headline?: string | null;
}

/** Pozivnica u firmu — prikaz u zahtevima majstora. */
export interface CompanyInvitationPending {
  invitationId: string;
  companyId: string;
  companyName: string;
  createdAtUtc: string;
}

/** Majstor u firmi (prihvatio poziv, uloga CompanyWorker). */
export interface CompanyWorkerMember {
  userId: string;
  firstName: string;
  lastName: string;
  username: string;
  email: string;
  phoneNumber?: string | null;
  headline?: string | null;
  description?: string | null;
  yearsOfExperience: number;
  hourlyRateAmount: number;
  hourlyRateCurrency: string;
  isAvailable: boolean;
  serviceCategories: string[];
  serviceZones: string[];
  averageRating?: number | null;
  totalJobsCompleted: number;
  totalReviews: number;
}
