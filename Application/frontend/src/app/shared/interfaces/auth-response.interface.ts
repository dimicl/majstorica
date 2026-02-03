import { User } from '../models';

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}
