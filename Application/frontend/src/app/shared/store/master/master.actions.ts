import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { MasterProfile } from '../../models/master.model';

export const MasterActions = createActionGroup({
  source: 'Master',
  events: {
    'Load Profile': emptyProps(),
    'Load Profile Success': props<{ profile: MasterProfile }>(),
    'Load Profile Failure': props<{ error: string }>(),
  },
});
