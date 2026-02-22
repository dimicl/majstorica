import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { take } from 'rxjs/operators';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { SERVICE_CATEGORIES } from '../../shared/constants/service-categories.constants';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { ClientActions } from '../../shared/store/client/client.actions';
import { MasterActions } from '../../shared/store/master/master.actions';
import { UserRole } from '../../shared/enums';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss',
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent
  ]
})
export class MainComponent implements OnInit {
  readonly auth = inject(AuthSelectorService);
  private store = inject(Store);

  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;

  ngOnInit(): void {
    // Iz auth state izvučeš user (id, role) i na osnovu toga dispatch-uješ get za klijenta ili majstora
    this.auth.userSelector$
      .pipe(take(1))
      .subscribe((user) => {
        if (!user) return;
        if (user.role === UserRole.Client) {
          this.store.dispatch(ClientActions.loadProfile());
        } else if (user.role === UserRole.Master) {
          this.store.dispatch(MasterActions.loadProfile());
        }
      });
  }

  categories = SERVICE_CATEGORIES;

  public onButtonClick(event: MouseEvent): void {
    console.log('Button clicked', event);
  }
}
