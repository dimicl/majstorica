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
}
