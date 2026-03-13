import { Component, Input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-avatar',
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.scss',
  standalone: true,
  imports: [CommonModule],
})
export class AvatarComponent {
  @Input() firstName = '';
  @Input() lastName = '';
  @Input() width = 36;
  @Input() height = 36;

  initials = computed(() => {
    const first = (this.firstName?.trim()?.[0] ?? '').toUpperCase();
    const last = (this.lastName?.trim()?.[0] ?? '').toUpperCase();
    return first + last || '?';
  });

  showPlaceholder = computed(() => {
    const init = this.initials();
    return init === '' || init === '?';
  });
}
