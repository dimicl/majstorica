import { MasterProfile } from '../../models/master.model';

export interface MasterState {
  profile: MasterProfile | null;
  loading: boolean;
  error: string | null;
}

export const initialMasterState: MasterState = {
  profile: null,
  loading: false,
  error: null,
};
