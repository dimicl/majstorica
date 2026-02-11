import { AsyncPipe, NgTemplateOutlet } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { UserRole } from '../../shared/enums/user-role.enum';

@Component({
  selector: 'app-profil',
  imports: [AsyncPipe, NgTemplateOutlet],
  templateUrl: './profil.component.html',
  styleUrl: './profil.component.scss',
})
export class ProfilComponent {
  private auth = inject(AuthSelectorService);
  user$ = this.auth.userSelector$;
  readonly userRole = UserRole;
}
