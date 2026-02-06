import { AuthState } from '../../interfaces';

const AUTH_TOKEN_KEY = 'auth_token';

/** Samo token iz localStorage; user se uvek uzima iz API-ja (/me) i setuje u store. */
function loadAuthFromStorage(): AuthState {
  const token =
    typeof localStorage !== 'undefined' ? localStorage.getItem(AUTH_TOKEN_KEY) : null;
  return {
    user: null,
    token,
    isAuthenticated: false,
    loading: false,
    error: null,
  };
}

export const initialAuthState: AuthState = loadAuthFromStorage();
