import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { User } from '../models/user.model';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    // Login
    'Login': props<{ email: string; password: string }>(),
    'Login Success': props<{ user: User }>(),
    'Login Failure': props<{ error: string }>(),

    // Logout
    'Logout': emptyProps(),
    'Logout Success': emptyProps(),

    // Register
    'Register': props<{ email: string; password: string; name: string }>(),
    'Register Success': props<{ user: User }>(),
    'Register Failure': props<{ error: string }>(),

    // Clear Error
    'Clear Error': emptyProps(),
  },
});

