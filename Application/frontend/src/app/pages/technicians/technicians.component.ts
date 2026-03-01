import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MapComponent } from '../../components/map/map.component';
import type { Coordinates } from '../../shared/types';
import type { MapMarkerData } from '../../shared/interfaces';
import { firstValueFrom } from 'rxjs';
import { MasterService } from '../../shared/services/master.service';
import {
  MasterListItem,
  type MastersListParams,
  type MastersListSort,
  DEFAULT_MASTERS_LIST_PARAMS,
  MASTERS_LIST_SORT_OPTIONS,
  MASTERS_LIST_CATEGORY_OPTIONS,
  MASTERS_LIST_RATING_OPTIONS,
} from '../../shared/interfaces';
import { mastersListParamsToCacheKey } from '../../shared/helpers/masters-cache-key.helper';
import { TechnicianTab } from '../../shared/types';
import { TechnicianDetailModalComponent } from './technician-detail-modal/technician-detail-modal.component';
import {
  CreateJobModalComponent,
  type CreateJobMaster,
} from '../../components/create-job-modal/create-job-modal.component';
import { AvatarComponent } from '../../components/avatar/avatar.component';

@Component({
  selector: 'app-technicians',
  imports: [
    CommonModule,
    FormsModule,
    MapComponent,
    TechnicianDetailModalComponent,
    CreateJobModalComponent,
    AvatarComponent,
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
  /** Parametri pretrage i filtera – ista struktura za UI i za cache key (keš 5 min). */
  public listParams: MastersListParams = { ...DEFAULT_MASTERS_LIST_PARAMS };
  public readonly sortOptions = MASTERS_LIST_SORT_OPTIONS;
  public readonly categoryOptions = MASTERS_LIST_CATEGORY_OPTIONS;
  public readonly ratingOptions = MASTERS_LIST_RATING_OPTIONS;
  public technicians: MasterListItem[] = [];
  public isLoading = true;
  public loadError: string | null = null;

  /** Preporučeni majstori (Neo4j) – za klijente koji su već angažovali majstore. */
  public recommendedMasters: MasterListItem[] = [];
  public recommendedLoading = false;

  public get mastersListParams(): MastersListParams {
    return {
      search: this.listParams.search.trim(),
      sort: this.listParams.sort,
      category: this.listParams.category.trim(),
      minRating: this.listParams.minRating,
    };
  }

  public get mastersListCacheKey(): string {
    return mastersListParamsToCacheKey(this.mastersListParams);
  }

  public get filteredTechnicians(): MasterListItem[] {
    const { search, sort, category, minRating } = this.listParams;
    const q = search.trim().toLowerCase();
    let list = this.technicians;
    if (q) {
      list = list.filter((t) =>
        `${t.firstName} ${t.lastName} ${t.username}`.toLowerCase().includes(q)
      );
    }
    if (category) {
      list = list.filter((t) => (t.category ?? '') === category);
    }
    if (minRating != null && minRating >= 1 && minRating <= 5) {
      list = list.filter((t) => t.rating != null && t.rating >= minRating);
    }
    const asc = sort === 'name-asc';
    return [...list].sort((a, b) => {
      const nameA = `${a.firstName} ${a.lastName}`.trim() || a.username;
      const nameB = `${b.firstName} ${b.lastName}`.trim() || b.username;
      return asc
        ? nameA.localeCompare(nameB, 'sr')
        : nameB.localeCompare(nameA, 'sr');
    });
  }

  ngOnInit(): void {
    this.loadMasters();
    this.loadRecommended();
  }

  async loadMasters(): Promise<void> {
    this.isLoading = true;
    this.loadError = null;
    try {
      const raw = await firstValueFrom(
        this.masterService.getMasters(this.mastersListParams)
      );
      this.technicians = raw.map((t) => ({
        ...t,
        category: t.category ?? null,
        rating: t.rating ?? null,
      }));
    } catch {
      this.loadError = 'Nije moguće učitati listu majstora.';
      this.technicians = [];
    } finally {
      this.isLoading = false;
    }
  }

  async loadRecommended(): Promise<void> {
    this.recommendedLoading = true;
    try {
      const raw = await firstValueFrom(
        this.masterService.getRecommendedMasters(10)
      );
      this.recommendedMasters = raw.map((t) => ({
        ...t,
        category: t.category ?? null,
        rating: t.rating ?? null,
      }));
    } catch {
      this.recommendedMasters = [];
    } finally {
      this.recommendedLoading = false;
    }
  }

  public setTab(tab: TechnicianTab): void {
    this.activeTab = tab;
  }

  public onSearchInput(value: string): void {
    this.listParams = { ...this.listParams, search: value };
  }

  public setSort(sort: MastersListSort): void {
    this.listParams = { ...this.listParams, sort };
  }

  public setCategory(category: string): void {
    this.listParams = { ...this.listParams, category };
  }

  public setMinRating(rating: number | null): void {
    this.listParams = { ...this.listParams, minRating: rating };
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
