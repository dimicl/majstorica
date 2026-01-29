import { AuthState } from './auth/auth.state';
import { ClientState } from './client/client.state';

export interface AppState {
  auth: AuthState;
  client: ClientState;
}

