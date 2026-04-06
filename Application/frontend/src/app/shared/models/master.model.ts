export interface MasterProfile {
  id: string;
  name: string;
  email: string;
  phone?: string;
  address?: string;
  registeredAt?: string;
}

/** Odgovor GET api/masters/profile – profil majstora (kategorija, ocena). */
export interface MasterProfileResponse {
  user: {
    id: string;
    email: string;
    username: string;
    firstName: string;
    lastName: string;
    role: string;
    phone?: string;
    address?: {
      street: string;
      city: string;
      zone?: string | null;
      postalCode?: string | null;
      country?: string | null;
    } | null;
  };
  category: string | null;
  rating: number | null;
  employerCompanyId?: string | null;
  employerCompanyName?: string | null;
  yearsOfExperience: number;
  hourlyRateAmount: number;
  hourlyRateCurrency: string;
  totalReviews: number;
}

export interface UpdateMasterProfileStatsPayload {
  yearsOfExperience?: number;
  hourlyRateAmount?: number;
  hourlyRateCurrency?: string;
}

/** Stavka GET api/masters/profile/reviews */
export interface MasterReviewListItem {
  id: string;
  jobId: string;
  rating: number;
  comment: string | null;
  createdAtUtc: string;
  reviewerName: string;
  reviewerUsername: string | null;
}
