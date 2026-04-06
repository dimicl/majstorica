import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { InputComponent } from '../../components/input/input.component';
import { UserRole } from '../../shared/enums';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { LoginRequest, RegisterRequest } from '../../shared/interfaces';
import { AuthHelper } from './helpers/auth.helper';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss',
  imports: [CommonModule, ReactiveFormsModule, InputComponent],
})
export class AuthComponent implements OnInit {
  readonly auth = inject(AuthSelectorService);
  private fb = inject(FormBuilder);

  isLoginMode = true;

  loginForm!: FormGroup;
  registerForm!: FormGroup;
  roleOptions: { value: UserRole; label: string }[] = [];

  ngOnInit(): void {
    const { loginForm, registerForm, roleOptions } = AuthHelper.setupAuthForms(
      this.fb
    );
    this.loginForm = loginForm;
    this.registerForm = registerForm;
    this.roleOptions = roleOptions;
  }

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

  public onRegister(): void {
    if (this.registerForm.valid) {
      const {
        firstName,
        lastName,
        email,
        username,
        phone,
        deliveryAddress,
        city,
        password,
        role,
      } = this.registerForm.value;
      const roleValue = AuthHelper.normalizeRegisterRole(role);
      const registerRequest: RegisterRequest = {
        firstName,
        lastName,
        email,
        username,
        password,
        role: roleValue,
        phone: phone || null,
        deliveryAddress: deliveryAddress || null,
        city,
      };

      this.auth.dispatchRegister(registerRequest);
    }
  }
}
