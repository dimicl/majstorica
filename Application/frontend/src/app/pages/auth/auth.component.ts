import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { InputComponent } from '../../components/input/input.component';
import { UserRole } from '../../shared/enums';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { LoginRequest, RegisterRequest } from '../../shared/interfaces';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss',
  imports: [CommonModule, ReactiveFormsModule, InputComponent],
})
export class AuthComponent {
  readonly auth = inject(AuthSelectorService);
  private fb = inject(FormBuilder);

  // Toggle između login i register
  isLoginMode = true;

  // Login forma
  loginForm: FormGroup = this.fb.group({
    usernameOrEmail: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  // Register forma sa validacijom za password match
  registerForm: FormGroup = this.fb.group(
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
      phone: [
        '',
        [
          Validators.required,
          Validators.pattern(/^\d{10}$/),
        ],
      ],
      deliveryAddress: [
        '',
        [
          Validators.maxLength(200),
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
    { validators: this.passwordMatchValidator }
  );

  // Role opcije za dropdown
  roleOptions = [
    { value: UserRole.Client, label: 'Klijent' },
    { value: UserRole.Master, label: 'Majstor' },
  ];

  // Custom validator za proveru da li se lozinke poklapaju
  private passwordMatchValidator(
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

  // Postavlja mod (login/register)
  public setMode(isLogin: boolean): void {
    this.isLoginMode = isLogin;
    this.auth.dispatchClearError();

    this.loginForm.reset();
    this.registerForm.reset(
      {
        role: UserRole.Client,
      },
      { emitEvent: false }
    );
  }

  // Submit login forme
  public onLogin(): void {
    if (this.loginForm.valid) {
      const { usernameOrEmail, password } = this.loginForm.value;
      const loginRequest: LoginRequest = {
        usernameOrEmail,
        password,
      };
      this.auth.dispatchLogin(loginRequest);
    }
  }

  // Submit register forme
  public onRegister(): void {
    if (this.registerForm.valid) {
      const {
        firstName,
        lastName,
        email,
        username,
        phone,
        deliveryAddress,
        password,
        role,
      } = this.registerForm.value;
      // role iz selecta dolazi kao string ili number – backend očekuje 1 (Client), 2 (Master) ili 3 (CompanyOwner)
      const raw = typeof role === 'string' ? Number(role) : (role as UserRole);
      const roleValue =
        Number.isFinite(raw) && raw >= 1 && raw <= 5 ? raw : UserRole.Client;
      const registerRequest: RegisterRequest = {
        firstName,
        lastName,
        email,
        username,
        password,
        role: roleValue,
        phone: phone || null,
        deliveryAddress: deliveryAddress || null,
      };

      this.auth.dispatchRegister(registerRequest);
    }
  }
}
