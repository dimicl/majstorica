import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { ServiceCardModel } from '../../shared/interfaces';


@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss',
  imports: [
    RouterLink,
    ButtonComponent
  ]
})
export class MainComponent {
  // Enums
  public eButtonType = BUTTON_TYPES;
  // SVG Icons
  public sharedSvgRoutes = SharedSvgRoutes;

  categories: ServiceCardModel[] = [
    {
      id: 1,
      icon: '⚡',
      title: 'Električar',
      description: 'Ugradnja, popravke, kratki spojevi, rasveta.'
    },
    {
      id: 2,
      icon: '🚰',
      title: 'Vodoinstalater',
      description: 'Curanje, sifoni, ventili, sanitarije.'
    },
    {
      id: 3,
      icon: '🧱',
      title: 'Keramičar',
      description: 'Kupatila, kuhinje, fugovanje i nivelacija.'
    },
    {
      id: 4,
      icon: '🛠️',
      title: 'Majstor za sve',
      description: 'Montaže, sitne popravke, “po kući”.'
    },
    {
      id: 5,
      icon: '🎨',
      title: 'Moler',
      description: 'Krečenje, gletovanje, priprema zidova.'
    },
    {
      id: 6,
      icon: '🪚',
      title: 'Stolar',
      description: 'Nameštaj po meri, popravke, vrata.'
    }
  ];

  public onButtonClick(event: MouseEvent): void {
    console.log('Button clicked', event);
  }
}
