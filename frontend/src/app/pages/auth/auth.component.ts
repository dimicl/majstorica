import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { AuthActions, selectAuthError, selectAuthLoading } from '../../store';
import { InputComponent } from '../../components/input/input.component';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss',
  imports: [CommonModule, ReactiveFormsModule, InputComponent],
})
export class AuthComponent {
  private store = inject(Store);
  private fb = inject(FormBuilder);

  // Selektori iz store-a
  loading$ = this.store.select(selectAuthLoading);
  error$ = this.store.select(selectAuthError);

  // Toggle između login i register
  isLoginMode = true;

  // Login forma
  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  // Register forma sa validacijom za password match
  registerForm: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.pattern(/^[0-9+\-\s]+$/)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],
  }, { validators: this.passwordMatchValidator });

  // Custom validator za proveru da li se lozinke poklapaju
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  // Postavlja mod (login/register)
  setMode(isLogin: boolean): void {
    this.isLoginMode = isLogin;
    this.store.dispatch(AuthActions.clearError());
  }

  // Submit login forme
  onLogin(): void {
    if (this.loginForm.valid) {
      const { email, password } = this.loginForm.value;
      this.store.dispatch(AuthActions.login({ email, password }));
    }
  }

  // Submit register forme
  onRegister(): void {
    if (this.registerForm.valid) {
      const { name, lastName, email, phone, password } = this.registerForm.value;
      
      this.store.dispatch(AuthActions.register({ 
        email, 
        password, 
        name: `${name} ${lastName}` 
      }));
    }
  }
}
