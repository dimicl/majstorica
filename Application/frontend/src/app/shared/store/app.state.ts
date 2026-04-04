import { AuthState } from '../interfaces';
import { ClientState } from '../interfaces/client-state.interface';
import { MarketplaceState } from './marketplace/marketplace.state';
import { MasterState } from './master/master.state';

export interface AppState {
  auth: AuthState;
  client: ClientState;
  master: MasterState;
  marketplace: MarketplaceState;
}
