import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MapComponent, MapMarkerData, Coordinates } from '../../components/map/map.component';

interface TechnicianProfile {
  name: string;
  city: string;
  rating: string;
  responseTime: string;
  jobsCompleted: string;
  skills: string[];
  tag: string;
  availability: string;
}

type TechnicianTab = 'list' | 'map';

@Component({
  selector: 'app-technicians',
  imports: [CommonModule, MapComponent],
  templateUrl: './technicians.component.html',
  styleUrl: './technicians.component.scss'
})
export class TechniciansComponent {
  public tabs: Record<'LIST' | 'MAP', TechnicianTab> = {
    LIST: 'list',
    MAP: 'map'
  };
  private readonly cityCoordinates: Record<string, Coordinates> = {
    'Novi Sad': { lat: 45.2671, lng: 19.8335 },
    'Beograd': { lat: 44.7866, lng: 20.4489 },
    'Niš': { lat: 43.3209, lng: 21.8958 },
    'Kragujevac': { lat: 44.0120, lng: 20.9110 },
    'Subotica': { lat: 46.1000, lng: 19.6667 }
  };
  private readonly defaultCenter: Coordinates = { lat: 45.2671, lng: 19.8335 };
  public activeTab: TechnicianTab = this.tabs.LIST;
  public searchQuery = '';
  public activeSpecialty = '';
  public specialtyFilters = [
    'Električar',
    'Vodoinstalater',
    'Keramičar',
    'Moler',
    'Grejanje',
    'Hitni pozivi'
  ];

  public technicianProfiles: TechnicianProfile[] = [
    {
      name: 'Nikola Jović',
      city: 'Novi Sad',
      rating: '4.9',
      responseTime: '22m',
      jobsCompleted: '340 poslova',
      skills: ['Elektrika', 'Pametni sistemi', 'Hitne intervencije'],
      tag: 'Top izbor',
      availability: 'Danima 08:00 – 22:00'
    },
    {
      name: 'Maja Petrović',
      city: 'Beograd',
      rating: '4.8',
      responseTime: '30m',
      jobsCompleted: '215 poslova',
      skills: ['Vodoinstalacije', 'Sanitarije', 'Hitni popravci'],
      tag: 'Hitni pozivi',
      availability: 'Danas do 18:00'
    },
    {
      name: 'Luka Šarić',
      city: 'Niš',
      rating: '4.7',
      responseTime: '45m',
      jobsCompleted: '180 poslova',
      skills: ['Keramičarski radovi', 'Nivelacija', 'Kupatila'],
      tag: 'Preporučeno',
      availability: 'Slobodan vikendom'
    },
    {
      name: 'Tatjana Dimić',
      city: 'Kragujevac',
      rating: '4.8',
      responseTime: '18m',
      jobsCompleted: '410 poslova',
      skills: ['Moleraj', 'Gletovanje', 'Spoljna fasada'],
      tag: 'Master',
      availability: 'Dostupna svakog dana'
    },
    {
      name: 'Marko Ilić',
      city: 'Subotica',
      rating: '4.6',
      responseTime: '52m',
      jobsCompleted: '165 poslova',
      skills: ['Grejanje', 'Boileri', 'Klimatske instalacije'],
      tag: 'Sezonski rad',
      availability: 'Radnim danima popodne'
    }
  ];

  public get filteredTechnicians(): TechnicianProfile[] {
    const query = this.searchQuery.trim().toLowerCase();
    return this.technicianProfiles.filter((tech) => {
      const matchesSpecialty = this.activeSpecialty
        ? tech.skills.some((skill) => skill.toLowerCase().includes(this.activeSpecialty.toLowerCase()))
        : true;
      const matchesQuery = query
        ? `${tech.name} ${tech.city} ${tech.skills.join(' ')} ${tech.tag}`
            .toLowerCase()
            .includes(query)
        : true;
      return matchesSpecialty && matchesQuery;
    });
  }

  public setTab(tab: TechnicianTab): void {
    this.activeTab = tab;
  }

  public onSearchInput(value: string): void {
    this.searchQuery = value;
  }

  public onToggleSpecialty(value: string): void {
    this.activeSpecialty = this.activeSpecialty === value ? '' : value;
  }

  public get isListActive(): boolean {
    return this.activeTab === this.tabs.LIST;
  }

  public get mapMarkers(): MapMarkerData[] {
    return this.filteredTechnicians.map((tech) => ({
      position: this.cityCoordinates[tech.city] ?? this.defaultCenter,
      title: `${tech.name} • ${tech.tag}`,
      color: '#f04f4c'
    }));
  }

  public get mapCenter(): Coordinates {
    if (!this.filteredTechnicians.length) {
      return this.defaultCenter;
    }

    return this.cityCoordinates[this.filteredTechnicians[0].city] ?? this.defaultCenter;
  }
}
