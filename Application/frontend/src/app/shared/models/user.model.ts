import { UserRole } from '../enums';

export interface User {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  phone?: string | null;
  deliveryAddress?: string | null;
  email?: string | null;
}
