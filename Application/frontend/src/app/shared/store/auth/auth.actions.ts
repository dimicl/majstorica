import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { User } from '../../models';
import { LoginRequest, RegisterRequest } from '../../interfaces';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    // Login
    Login: props<{ request: LoginRequest }>(),
    'Login Success': props<{ user: User; token: string }>(),
    'Login Failure': props<{ error: string }>(),

    // Logout
    Logout: emptyProps(),
    'Logout Success': emptyProps(),

    // Register
    Register: props<{
      request: RegisterRequest;
    }>(),
    'Register Success': props<{ user: User; token: string }>(),
    'Register Failure': props<{ error: string }>(),

    // Load User from Token
    'Load User': emptyProps(),
    'Load User Success': props<{ user: User; token: string }>(),
    'Load User Failure': emptyProps(),

    // Clear Error
    'Clear Error': emptyProps(),

    /** Delimično ažuriranje korisnika u sesiji (npr. kategorija majstora). */
    'Patch User': props<{ partial: Partial<User> }>(),
  },
});
