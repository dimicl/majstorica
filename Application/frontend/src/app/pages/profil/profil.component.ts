import { AsyncPipe, NgIf } from '@angular/common';
import { Component, inject } from '@angular/core';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';

@Component({
  selector: 'app-profil',
  imports: [NgIf, AsyncPipe],
  templateUrl: './profil.component.html',
  styleUrl: './profil.component.scss',
})
export class ProfilComponent {
  private auth = inject(AuthSelectorService);
  user$ = this.auth.userSelector$;
}
