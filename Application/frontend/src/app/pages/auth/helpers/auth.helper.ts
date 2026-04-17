import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { UserRole } from '../../../shared/enums/user-role.enum';

export class AuthHelper {
  private static readonly roleOptionsConst = [
    { value: UserRole.Client, label: 'Klijent' },
    { value: UserRole.Master, label: 'Majstor' },
    { value: UserRole.CompanyOwner, label: 'Vlasnik' },
  ];

  private static passwordMatchValidator(
    control: AbstractControl
  ): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (
      password &&
      confirmPassword &&
      password.value !== confirmPassword.value
    ) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    return null;
  }

  static setupAuthForms(fb: FormBuilder) {
    const loginForm = fb.group({
      usernameOrEmail: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(8)]],
    });

    const registerForm = fb.group(
      {
        firstName: [
          '',
          [
            Validators.required,
            Validators.minLength(2),
            Validators.maxLength(20),
          ],
        ],
        lastName: [
          '',
          [
            Validators.required,
            Validators.minLength(2),
            Validators.maxLength(20),
          ],
        ],
        email: ['', [Validators.required, Validators.email]],
        username: [
          '',
          [
            Validators.required,
            Validators.minLength(3),
            Validators.maxLength(30),
          ],
        ],
        phone: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
        deliveryAddress: ['', [Validators.maxLength(200)]],
        city: [
          '',
          [
            Validators.required,
            Validators.minLength(2),
            Validators.maxLength(20),
          ],
        ],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.maxLength(100),
          ],
        ],
        confirmPassword: ['', [Validators.required]],
        role: [UserRole.Client, [Validators.required]],
      },
      { validators: AuthHelper.passwordMatchValidator }
    );

    return {
      loginForm,
      registerForm,
      roleOptions: AuthHelper.roleOptionsConst,
    };
  }

  public static normalizeRegisterRole(role: unknown): UserRole {
    const allowed = new Set<string>(
      Object.values(UserRole).filter((v) => typeof v === 'string')
    );
    if (typeof role === 'string' && allowed.has(role)) {
      return role as UserRole;
    }
    if (typeof role === 'number' && role >= 1 && role <= 5) {
      const legacy: UserRole[] = [
        UserRole.Client,
        UserRole.Master,
        UserRole.CompanyOwner,
        UserRole.CompanyWorker,
        UserRole.Admin,
      ];
      return legacy[role - 1] ?? UserRole.Client;
    }
    return UserRole.Client;
  }
}
