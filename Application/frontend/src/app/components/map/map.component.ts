import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';

export type Coordinates = { lat: number; lng: number };

export interface MapMarkerData {
  position: Coordinates;
  title?: string;
  color?: string;
}

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map.component.html',
  styleUrl: './map.component.scss',
})
export class MapComponent implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('mapContainer', { static: false }) private mapContainer?: ElementRef<HTMLDivElement>;

  @Input() markers: MapMarkerData[] = [];
  @Input() height = '100%';
  @Input() width = '100%';
  @Input() center: Coordinates = { lat: 45.2671, lng: 19.8335 };
  @Input() zoom = 11;

  private map?: L.Map;
  private tileLayer?: L.TileLayer;
  private markerLayer = L.layerGroup();

  public ngAfterViewInit(): void {
    if (!this.mapContainer) {
      return;
    }

    this.map = L.map(this.mapContainer.nativeElement, {
      center: [this.center.lat, this.center.lng],
      zoom: this.zoom,
      zoomControl: true,
      preferCanvas: true,
    });

    this.tileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
    });

    this.tileLayer.addTo(this.map);
    this.markerLayer.addTo(this.map);

    const tileContainer = this.tileLayer.getContainer?.();
    if (tileContainer) {
      tileContainer.classList.add('leaflet-grayscale');
    }

    this.updateMarkers();
  }

  public ngOnChanges(changes: SimpleChanges): void {
    if (!this.map) {
      return;
    }

    if ('center' in changes || 'zoom' in changes) {
      this.updateView();
    }

    if ('markers' in changes) {
      this.updateMarkers();
    }
  }

  public ngOnDestroy(): void {
    this.markerLayer.clearLayers();
    this.map?.remove();
    this.map = undefined;
    this.tileLayer = undefined;
  }

  private updateView(): void {
    if (!this.map) {
      return;
    }

    this.map.setView([this.center.lat, this.center.lng], this.zoom);
  }

  private updateMarkers(): void {
    if (!this.map) {
      return;
    }

    this.markerLayer.clearLayers();

    this.markers.forEach((marker) => {
      const options: L.MarkerOptions = {
        icon: this.createMarkerIcon(marker.color),
        title: marker.title,
      };

      const instance = L.marker([marker.position.lat, marker.position.lng], options).addTo(this.markerLayer);

    });

  }

  private createMarkerIcon(color = '#f04f4c'): L.Icon {
    return L.divIcon({
      className: 'custom-marker',
      html: `<span class="marker-dot" style="background:${color}"></span>`,
      iconSize: [20, 20],
      iconAnchor: [10, 10],
    }) as L.Icon;
  }
}

