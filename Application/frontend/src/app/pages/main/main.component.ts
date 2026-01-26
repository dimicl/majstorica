import { Component, inject, signal } from '@angular/core';
import { MapComponent } from '../../components/map/map.component';
import { SignalrService } from '../../shared/services/signalr.service';
import { RouterLink } from '@angular/router';


@Component({
  selector: 'app-main',
  imports: [MapComponent, RouterLink],
  templateUrl: './main.component.html',
  styleUrl: './main.component.scss'
})
export class MainComponent {
  private signalr = inject(SignalrService);

  // Prikaži status konekcije u UI-ju (ne ruši app ako backend/hub ne radi)
  signalrStatus = this.signalr.status;
  signalrError = this.signalr.lastError;

  // Hub URL (primer) — prilagodi ako ti je druga ruta na backend-u
  hubUrl = signal<string>('https://localhost:5001/hubs/notifications');

  connectSignalr(): void {
    void this.signalr.connect(this.hubUrl());
  }

  disconnectSignalr(): void {
    void this.signalr.disconnect();
  }
  categories = [
    {
      icon: '⚡',
      title: 'Električar',
      desc: 'Ugradnja, popravke, kratki spojevi, rasveta.'
    },
    {
      icon: '🚰',
      title: 'Vodoinstalater',
      desc: 'Curanje, sifoni, ventili, sanitarije.'
    },
    {
      icon: '🧱',
      title: 'Keramičar',
      desc: 'Kupatila, kuhinje, fugovanje i nivelacija.'
    },
    {
      icon: '🛠️',
      title: 'Majstor za sve',
      desc: 'Montaže, sitne popravke, “po kući”.'
    },
    {
      icon: '🎨',
      title: 'Moler',
      desc: 'Krečenje, gletovanje, priprema zidova.'
    },
    {
      icon: '🪚',
      title: 'Stolar',
      desc: 'Nameštaj po meri, popravke, vrata.'
    }
  ];
}
