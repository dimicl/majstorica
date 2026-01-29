import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from '../../components/button/button.component';
import { BUTTON_TYPES } from '../../shared/types';

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

  public quickFilters = [
    'Električar',
    'Vodoinstalater',
    'Keramičar',
    'Klimatske instalacije',
    'Stolar',
    'Moler'
  ];

  public heroStats = [
    { value: '1.200+', label: 'zahteva mesečno' },
    { value: '97%', label: 'zadovoljstvo klijenata' },
    { value: '45 min', label: 'prosečan odgovor' }
  ];

  public featuredServices = [
    {
      title: 'Električar',
      description: 'Hitne intervencije, osvetljenje, pametni sistemi i prepravke.',
      tag: 'Hitno',
      rating: '★ 4.9',
      icon: '⚡'
    },
    {
      title: 'Vodoinstalater',
      description: 'Sanitarije, cevovodi, grejni sistemi i zamena bojlera.',
      tag: 'Popularno',
      rating: '★ 4.8',
      icon: '🚰'
    },
    {
      title: 'Keramičar',
      description: 'Postavljanje pločica, fugovanje i nivelacija podova i zidova.',
      tag: 'Preporučeno',
      rating: '★ 4.7',
      icon: '🧱'
    },
    {
      title: 'Servis klima',
      description: 'Sezonsko održavanje, punjenje gasa i čišćenje filtera.',
      tag: 'Sezonski',
      rating: '★ 4.8',
      icon: '❄️'
    }
  ];

  public processSteps = [
    {
      index: '01',
      title: 'Napiši šta ti treba',
      text: 'Izaberi kategoriju, opiši problem i dodaj sliku ako možeš.'
    },
    {
      index: '02',
      title: 'Uporedi ponude',
      text: 'Majstori ti šalju procene, rokove i dostupnost. Ti biraš kome veruješ.'
    },
    {
      index: '03',
      title: 'Prati posao',
      text: 'Dogovoreni datum se beleži u kalendaru, a komunikacija ostaje na platformi.'
    }
  ];

  public onHeroAction(event: MouseEvent): void {
    console.log('Hero action', event);
  }
}
