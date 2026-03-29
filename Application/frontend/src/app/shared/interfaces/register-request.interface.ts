import { UserRole } from '../enums';

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  password: string;
  role: UserRole;
  phone?: string | null;
  deliveryAddress?: string | null; // street
  city: string;
}
