import {
  Component,
  ElementRef,
  input,
  Optional,
  output,
  Self,
  signal,
  ViewChild,
} from '@angular/core';
import {
  ControlValueAccessor,
  NgControl,
} from '@angular/forms';

@Component({
  selector: 'app-input',
  imports: [],
  templateUrl: './input.component.html',
  styleUrl: './input.component.scss',
})
export class InputComponent implements ControlValueAccessor {
  @ViewChild('inputRef') inputRef!: ElementRef<HTMLInputElement>;

  // Inputs
  id = input<string>('');
  type = input<string>('text');
  label = input<string>('');
  theme = input<'light' | 'dark'>('light');
  errorMessage = input<string>(''); // Error message to display

  // Signals
  disabled = signal<boolean>(false);
  value = signal<string>('');
  focused = signal<boolean>(false);

  // Outputs
  onBlur = output();
  onFocused = output();
  onValueChange = output<string | null>();

  constructor(
    @Optional() @Self() public ngControl: NgControl | null,
  ) {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
  }

  // Check if should show error
  get showError(): boolean {
    return !!(this.ngControl?.invalid && this.ngControl?.touched && this.errorMessage());
  }

  // Handle input event
  onInput(event: Event): void {
    const inputElement = event.target as HTMLInputElement;
    this.value.set(inputElement.value);
    this.dispatchOnChange(this.value());
  }

  // ControlValueAccessor methods
  onChange = (_: any) => {};
  onTouched = () => {};

  dispatchOnChange(value: string): void {
    if (value === '') {
      this.onChange(null);
      this.onValueChange.emit(null);
    } else {
      this.onChange(value);
      this.onValueChange.emit(value);
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  writeValue(value: string): void {
    this.value.set(value ?? '');
  }

  // Focus input
  focus(): void {
    this.inputRef?.nativeElement?.focus();
  }

  onFocus(): void {
    this.onFocused.emit();
    this.focused.set(true);
  }

  dispatchOnBlur(): void {
    this.onBlur.emit();
    this.onTouched();
    this.focused.set(false);
  }
}
