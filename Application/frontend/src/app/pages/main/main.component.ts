import { Component, inject, OnInit, signal } from '@angular/core';
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
import {
  isClientUserRole,
  isMasterLikeUserRole,
} from '../../shared/utils/user-role.util';
import { CompanySetupModalComponent } from '../../components/company-setup-modal/company-setup-modal.component';
import { CompanyService } from '../../shared/services/company.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss',
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent,
    CompanySetupModalComponent
  ]
})
export class MainComponent implements OnInit {
  readonly auth = inject(AuthSelectorService);
  private store = inject(Store);
  private companyService = inject(CompanyService);
  readonly userRole = UserRole;

  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;
  showCompanySetupModal = signal(false);

  ngOnInit(): void {
    // Iz auth state izvučeš user (id, role) i na osnovu toga dispatch-uješ get za klijenta ili majstora
    this.auth.userSelector$
      .pipe(take(1))
      .subscribe((user) => {
        if (!user) return;
        if (isClientUserRole(user.role)) {
          this.store.dispatch(ClientActions.loadProfile());
        } else if (isMasterLikeUserRole(user.role)) {
          this.store.dispatch(MasterActions.loadProfile());
        } else if (user.role === UserRole.CompanyOwner) {
          void this.evaluateCompanySetupModal();
        }
      });
  }

  categories = SERVICE_CATEGORIES;

  public onButtonClick(event: MouseEvent): void {
    console.log('Button clicked', event);
  }

  async onCompanySetupCompleted(): Promise<void> {
    this.showCompanySetupModal.set(false);
  }

  private async evaluateCompanySetupModal(): Promise<void> {
    try {
      const company = await firstValueFrom(this.companyService.getMyCompany());
      this.showCompanySetupModal.set(!company);
    } catch {
      this.showCompanySetupModal.set(false);
    }
  }
}
