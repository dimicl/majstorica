import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MapComponent } from '../../components/map/map.component';
import type { Coordinates } from '../../shared/types';
import type { MapMarkerData } from '../../shared/interfaces';
import { firstValueFrom } from 'rxjs';
import { MasterService } from '../../shared/services/master.service';
import { MasterListItem } from '../../shared/interfaces';
import { TechnicianTab } from '../../shared/types';
import { TechnicianDetailModalComponent } from './technician-detail-modal/technician-detail-modal.component';
import { CreateJobModalComponent, type CreateJobMaster } from '../../components/create-job-modal/create-job-modal.component';

@Component({
  selector: 'app-technicians',
  imports: [
    CommonModule,
    MapComponent,
    TechnicianDetailModalComponent,
    CreateJobModalComponent,
  ],
  templateUrl: './technicians.component.html',
  styleUrl: './technicians.component.scss',
})
export class TechniciansComponent implements OnInit {
  private masterService = inject(MasterService);

  selectedMasterIdForDetail: string | null = null;
  showCreateJobModal = false;
  createJobMaster: CreateJobMaster | null = null;

  public tabs: Record<'LIST' | 'MAP', TechnicianTab> = {
    LIST: 'list',
    MAP: 'map',
  };
  private readonly defaultCenter: Coordinates = { lat: 45.2671, lng: 19.8335 };

  public activeTab: TechnicianTab = this.tabs.LIST;
  public searchQuery = '';
  public technicians: MasterListItem[] = [];
  public isLoading = true;
  public loadError: string | null = null;

  public get filteredTechnicians(): MasterListItem[] {
    const q = this.searchQuery.trim().toLowerCase();
    if (!q) return this.technicians;
    return this.technicians.filter((t) =>
      `${t.firstName} ${t.lastName} ${t.username}`.toLowerCase().includes(q)
    );
  }

  ngOnInit(): void {
    this.loadMasters();
  }

  async loadMasters(): Promise<void> {
    this.isLoading = true;
    this.loadError = null;
    try {
      this.technicians = await firstValueFrom(this.masterService.getMasters());
    } catch {
      this.loadError = 'Nije moguće učitati listu majstora.';
      this.technicians = [];
    } finally {
      this.isLoading = false;
    }
  }

  public setTab(tab: TechnicianTab): void {
    this.activeTab = tab;
  }

  public onSearchInput(value: string): void {
    this.searchQuery = value;
  }

  public get isListActive(): boolean {
    return this.activeTab === this.tabs.LIST;
  }

  public get mapMarkers(): MapMarkerData[] {
    return this.filteredTechnicians.map((_, i) => ({
      position: this.defaultCenter,
      title: this.filteredTechnicians[i]
        ? `${this.filteredTechnicians[i].firstName} ${this.filteredTechnicians[i].lastName}`
        : '',
      color: '#f04f4c',
    }));
  }

  public get mapCenter(): Coordinates {
    return this.defaultCenter;
  }

  fullName(t: MasterListItem): string {
    return `${t.firstName} ${t.lastName}`.trim() || t.username;
  }

  atUsername(t: MasterListItem): string {
    return '@' + t.username;
  }

  openDetail(masterId: string): void {
    this.selectedMasterIdForDetail = masterId;
  }

  closeDetail(): void {
    this.selectedMasterIdForDetail = null;
  }

  onOpenCreateJob(event: { master: CreateJobMaster }): void {
    this.closeDetail();
    setTimeout(() => {
      this.createJobMaster = event.master;
      this.showCreateJobModal = true;
    }, 0);
  }

  closeCreateJobModal(): void {
    this.showCreateJobModal = false;
    this.createJobMaster = null;
  }

  onJobCreated(_event: { jobId: string }): void {
    this.closeCreateJobModal();
  }
}
