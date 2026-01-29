import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { ClientProfile } from '../../models/client.model';

export const ClientActions = createActionGroup({
  source: 'Client',
  events: {
    'Load Profile': emptyProps(),
    'Load Profile Success': props<{ profile: ClientProfile }>(),
    'Load Profile Failure': props<{ error: string }>(),
  },
});
