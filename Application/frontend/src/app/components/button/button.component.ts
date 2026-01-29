import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BUTTON_TYPES, type ButtonType } from '../../shared/types';
import { CommonModule } from '@angular/common';
import { SvgIconComponent } from 'angular-svg-icon';


@Component({
  selector: 'app-button',
  templateUrl: './button.component.html',
  styleUrl: './button.component.scss',
  imports: [
    CommonModule,
    SvgIconComponent,
  ],
})
export class ButtonComponent {
  //inputs
  @Input() label = '';
  @Input() buttonType: ButtonType = BUTTON_TYPES.NEUTRAL;
  @Input() hasIcon = false;
  @Input() iconSrc = '';
  @Input() disabled = false;

  //outputs
  @Output() onButtonClick = new EventEmitter<MouseEvent>();

  onClick(event: MouseEvent): void {
    this.onButtonClick.emit(event);
  }
}
