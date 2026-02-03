import { AuthState } from '../../interfaces';

export const initialAuthState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  loading: false,
  error: null,
};
