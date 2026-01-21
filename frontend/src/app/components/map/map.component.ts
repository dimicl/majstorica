import { AfterViewInit, Component, OnDestroy } from '@angular/core';
import * as L from 'leaflet';

@Component({
  selector: 'app-map',
  standalone: true,
  templateUrl: './map.component.html',
  styleUrl: './map.component.scss',
})
export class MapComponent implements AfterViewInit, OnDestroy {
  private map?: L.Map;

  ngAfterViewInit(): void {
    // Novi Sad (primer) — promeni po potrebi
    const center: L.LatLngExpression = [45.2671, 19.8335];

    this.map = L.map('main-map', {
      center,
      zoom: 13,
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution:
        '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19,
    }).addTo(this.map);

    // Primer: "majstor" kao veća plava tačka (circle marker)
    const majstorLokacija: L.LatLngExpression = [45.2682, 19.8352];
    L.circleMarker(majstorLokacija, {
      radius: 10,
      color: '#1e88e5',
      weight: 3,
      fillColor: '#1e88e5',
      fillOpacity: 0.55,
    })
      .addTo(this.map)
      .bindTooltip('Majstor (primer)', { permanent: false, direction: 'top' })
      .bindPopup('<strong>Majstor (primer)</strong><br/>Neka adresa, Novi Sad');
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }
}

