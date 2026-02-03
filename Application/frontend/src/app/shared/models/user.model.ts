import { UserRole } from '../enums';

export interface User {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}
