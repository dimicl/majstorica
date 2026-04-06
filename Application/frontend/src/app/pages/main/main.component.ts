import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Store } from '@ngrx/store';
import { catchError, map, of, switchMap, take } from 'rxjs';
import { ButtonComponent } from '../../components/button/button.component';
import { CompanySetupModalComponent } from '../../components/company-setup-modal/company-setup-modal.component';
import { BUTTON_TYPES } from '../../shared/types';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { SERVICE_CATEGORIES } from '../../shared/constants/service-categories.constants';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { CompanyService } from '../../shared/services/company.service';
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
    ButtonComponent,
    CompanySetupModalComponent,
  ],
})
export class MainComponent implements OnInit {
  readonly auth = inject(AuthSelectorService);
  private store = inject(Store);
  private companyService = inject(CompanyService);
  readonly userRole = UserRole;

  readonly showCompanySetupModal = signal(false);

  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;

  ngOnInit(): void {
    this.auth.userSelector$
      .pipe(
        take(1),
        switchMap((user) => {
          if (!user) return of(false);
          if (user.role === UserRole.Client) {
            this.store.dispatch(ClientActions.loadProfile());
          } else if (user.role === UserRole.Master) {
            this.store.dispatch(MasterActions.loadProfile());
          }
          if (user.role === UserRole.CompanyOwner) {
            return this.companyService.getMyCompany().pipe(
              map((company) => company === null),
              catchError(() => of(false))
            );
          }
          return of(false);
        })
      )
      .subscribe((needsCompanySetup) => {
        this.showCompanySetupModal.set(needsCompanySetup);
      });
  }

  onCompanySetupCompleted(): void {
    this.showCompanySetupModal.set(false);
  }

  categories = SERVICE_CATEGORIES;

  public onButtonClick(event: MouseEvent): void {
    console.log('Button clicked', event);
  }
}
