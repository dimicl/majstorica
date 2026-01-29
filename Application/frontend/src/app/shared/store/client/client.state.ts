import { ClientProfile } from '../../models/client.model';

export interface ClientState {
  profile: ClientProfile | null;
  loading: boolean;
  error: string | null;
}

export const initialClientState: ClientState = {
  profile: null,
  loading: false,
  error: null,
};
