import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types';
import {
  SERVICES_QUICK_FILTERS,
  SERVICES_HERO_STATS,
  SERVICES_FEATURED,
  SERVICES_PROCESS_STEPS,
} from '../../shared/constants/services-page.constants';

@Component({
  selector: 'app-services',
  imports: [
    CommonModule,
    RouterLink,
    ButtonComponent
  ],
  templateUrl: './services.component.html',
  styleUrl: './services.component.scss'
})
export class ServicesComponent {
  public eButtonType = BUTTON_TYPES;

  public quickFilters = [...SERVICES_QUICK_FILTERS];
  public heroStats = [...SERVICES_HERO_STATS];
  public featuredServices = [...SERVICES_FEATURED];
  public processSteps = [...SERVICES_PROCESS_STEPS];

  public onHeroAction(event: MouseEvent): void {
    console.log('Hero action', event);
  }
}
