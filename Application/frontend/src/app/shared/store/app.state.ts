import { AuthState } from '../interfaces';
import { ClientState } from '../interfaces/client-state.interface';

export interface AppState {
  auth: AuthState;
  client: ClientState;
}
