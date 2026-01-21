import { Component, inject, signal } from '@angular/core';
import { MapComponent } from '../../components/map/map.component';
import { SignalrService } from '../../shared/services/signalr.service';

@Component({
  selector: 'app-main',
  imports: [MapComponent],
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
}
