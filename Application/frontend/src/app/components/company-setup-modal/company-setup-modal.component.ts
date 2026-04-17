import { Component, inject, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { InputComponent } from '../input/input.component';
import { CompanyService } from '../../shared/services/company.service';
import { ButtonComponent } from '../button/button.component';
import { BUTTON_TYPES } from '../../shared/types';

function addressPairValidator(
  control: AbstractControl
): ValidationErrors | null {
  const street = (control.get('street')?.value ?? '').toString().trim();
  const city = (control.get('city')?.value ?? '').toString().trim();
  if ((street && !city) || (!street && city)) {
    return { addressIncomplete: true };
  }
  return null;
}

@Component({
  selector: 'app-company-setup-modal',
  templateUrl: './company-setup-modal.component.html',
  styleUrl: './company-setup-modal.component.scss',
  imports: [CommonModule, ReactiveFormsModule, InputComponent, ButtonComponent],
  host: {
    '(document:keydown)': 'onDocumentKeydown($event)',
  },
})
export class CompanySetupModalComponent {
  private fb = inject(FormBuilder);
  private companyService = inject(CompanyService);

  public eButtonType = BUTTON_TYPES;

  completed = output<void>();

  submitting = false;
  apiError: string | null = null;

  form: FormGroup = this.fb.group(
    {
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(150),
        ],
      ],
      phoneNumber: ['', [Validators.required, Validators.pattern(/^\d{10}$/)]],
      email: ['', [Validators.required, Validators.email]],
      street: ['', [Validators.maxLength(200)]],
      city: ['', [Validators.maxLength(100)]],
    },
    { validators: addressPairValidator }
  );

  onDocumentKeydown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      event.stopPropagation();
    }
  }

  public onSubmit(): void {
    this.apiError = null;
    if (this.form.invalid || this.submitting) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    const street = (v.street ?? '').toString().trim();
    const city = (v.city ?? '').toString().trim();

    this.submitting = true;
    this.companyService
      .createCompany({
        name: v.name.trim(),
        phoneNumber: v.phoneNumber,
        email: v.email.trim(),
        street: street || null,
        city: city || null,
      })
      .pipe(finalize(() => (this.submitting = false)))
      .subscribe({
        next: () => this.completed.emit(),
        error: (err: HttpErrorResponse) => {
          this.apiError = CompanyService.mapApiError(err);
        },
      });
  }
}
