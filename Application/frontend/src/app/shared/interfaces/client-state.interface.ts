import { ClientProfile } from '../models';

export interface ClientState {
  profile: ClientProfile | null;
  loading: boolean;
  error: string | null;
}
