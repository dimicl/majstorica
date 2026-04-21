import { UserRole } from '../enums';

export interface User {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  phone?: string | null;
  email?: string | null;
  address?: {
    street: string;
    city: string;
    zone?: string | null;
    postalCode?: string | null;
    country?: string | null;
  } | null;
  /** Majstor: sinhrono sa GET masters/profile (kategorija usluge). */
  category?: string | null;
  /** CompanyWorker: naziv firme iz GET masters/profile. */
  employerCompanyName?: string | null;
}
