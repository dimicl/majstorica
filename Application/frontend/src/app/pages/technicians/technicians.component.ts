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
  type MastersEntityFilter,
  type MastersListParams,
  type MastersListSort,
  DEFAULT_MASTERS_LIST_PARAMS,
  MASTERS_LIST_SORT_OPTIONS,
  MASTERS_LIST_CATEGORY_OPTIONS,
  MASTERS_LIST_RATING_OPTIONS,
  MASTERS_ENTITY_FILTER_OPTIONS,
} from '../../shared/interfaces';
import { BUTTON_TYPES, TechnicianTab } from '../../shared/types';
import { TechnicianDetailModalComponent } from './technician-detail-modal/technician-detail-modal.component';
import { CompanyDetailModalComponent } from './company-detail-modal/company-detail-modal.component';
import {
  CreateJobModalComponent,
  type CreateJobMaster,
} from '../../components/create-job-modal/create-job-modal.component';
import { AvatarComponent } from '../../components/avatar/avatar.component';
import { ButtonComponent } from '../../components/button/button.component';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { SvgIconComponent } from 'angular-svg-icon';

@Component({
  selector: 'app-technicians',
  imports: [
    CommonModule,
    FormsModule,
    MapComponent,
    TechnicianDetailModalComponent,
    CompanyDetailModalComponent,
    CreateJobModalComponent,
    AvatarComponent,
    ButtonComponent,
    SvgIconComponent,
  ],
  templateUrl: './technicians.component.html',
  styleUrl: './technicians.component.scss',
})
export class TechniciansComponent implements OnInit {
  private masterService = inject(MasterService);

  private searchTimeoutId: ReturnType<typeof setTimeout> | null = null;

  selectedMasterIdForDetail: string | null = null;
  selectedCompanyIdForDetail: string | null = null;
  showCreateJobModal = false;
  createJobMaster: CreateJobMaster | null = null;

  public eButtonType = BUTTON_TYPES;
  public sharedSvgRoutes = SharedSvgRoutes;

  public tabs: Record<'LIST' | 'MAP', TechnicianTab> = {
    LIST: 'list',
    MAP: 'map',
  };
  private readonly defaultCenter: Coordinates = { lat: 45.2671, lng: 19.8335 };

  public activeTab: TechnicianTab = this.tabs.LIST;
  public listParams: MastersListParams = { ...DEFAULT_MASTERS_LIST_PARAMS };
  public readonly sortOptions = MASTERS_LIST_SORT_OPTIONS;
  public readonly categoryOptions = MASTERS_LIST_CATEGORY_OPTIONS;
  public readonly ratingOptions = MASTERS_LIST_RATING_OPTIONS;
  public readonly entityFilterOptions = MASTERS_ENTITY_FILTER_OPTIONS;

  public technicians: MasterListItem[] = [];
  public totalCount = 0;
  public totalPages = 0;
  public isLoading = true;
  public loadError: string | null = null;

  public recommendedMasters: MasterListItem[] = [];
  public recommendedLoading = false;

  public get mastersListParams(): MastersListParams {
    return {
      search: this.listParams.search.trim(),
      sort: this.listParams.sort,
      category: this.listParams.category.trim(),
      minRating: this.listParams.minRating,
      entityType: this.listParams.entityType,
      page: this.listParams.page,
      pageSize: this.listParams.pageSize,
    };
  }

  public get filteredTechnicians(): MasterListItem[] {
    return this.technicians;
  }

  ngOnInit(): void {
    this.loadMasters();
    this.loadRecommended();
  }

  async loadMasters(): Promise<void> {
    this.isLoading = true;
    this.loadError = null;
    try {
      const page = await firstValueFrom(
        this.masterService.getMasters(this.mastersListParams)
      );
      this.technicians = page.items.map((t) => ({
        ...t,
        kind: t.kind === 'company' ? 'company' : 'master',
        category: t.category ?? null,
        rating: t.rating ?? null,
      }));
      this.totalCount = page.totalCount;
      this.totalPages = page.totalPages;
    } catch {
      this.loadError = 'Nije moguće učitati listu majstora.';
      this.technicians = [];
      this.totalCount = 0;
      this.totalPages = 0;
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
        kind: 'master' as const,
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

  public async onSearchInput(value: string): Promise<void> {
    this.listParams = { ...this.listParams, search: value, page: 1 };
    if (this.searchTimeoutId) {
      clearTimeout(this.searchTimeoutId);
    }
    this.searchTimeoutId = setTimeout(() => {
      void this.loadMasters();
    }, 300);
  }

  public async setSort(sort: MastersListSort): Promise<void> {
    this.listParams = { ...this.listParams, sort, page: 1 };
    await this.loadMasters();
  }

  public async setCategory(category: string): Promise<void> {
    this.listParams = { ...this.listParams, category, page: 1 };
    await this.loadMasters();
  }

  public async setMinRating(rating: number | null): Promise<void> {
    this.listParams = { ...this.listParams, minRating: rating, page: 1 };
    await this.loadMasters();
  }

  public async setEntityType(entityType: MastersEntityFilter): Promise<void> {
    this.listParams = { ...this.listParams, entityType, page: 1 };
    await this.loadMasters();
  }

  public async goPrevPage(): Promise<void> {
    if (this.listParams.page <= 1) return;
    this.listParams = {
      ...this.listParams,
      page: this.listParams.page - 1,
    };
    await this.loadMasters();
  }

  public async goNextPage(): Promise<void> {
    if (this.listParams.page >= this.totalPages) return;
    this.listParams = {
      ...this.listParams,
      page: this.listParams.page + 1,
    };
    await this.loadMasters();
  }

  public get isListActive(): boolean {
    return this.activeTab === this.tabs.LIST;
  }

  public get mapMarkers(): MapMarkerData[] {
    return this.filteredTechnicians.map((t) => ({
      position: this.defaultCenter,
      title: this.displayName(t),
      color: t.kind === 'company' ? '#3e7dd7' : '#f04f4c',
    }));
  }

  public get mapCenter(): Coordinates {
    return this.defaultCenter;
  }

  displayName(t: MasterListItem): string {
    if (t.kind === 'company') {
      return (t.companyName ?? '').trim() || 'Firma';
    }
    return `${t.firstName} ${t.lastName}`.trim() || t.username;
  }

  companySubtitle(t: MasterListItem): string {
    const parts: string[] = [];
    if (t.city?.trim()) parts.push(t.city.trim());
    if (t.email?.trim()) parts.push(t.email.trim());
    return parts.join(' · ');
  }

  companyDescriptionPreview(t: MasterListItem): string | null {
    const d = t.description?.trim();
    if (!d) return null;
    return d.length > 120 ? d.slice(0, 117) + '…' : d;
  }

  atUsername(t: MasterListItem): string {
    return '@' + t.username;
  }

  openCard(item: MasterListItem): void {
    if (item.kind === 'company') {
      this.selectedMasterIdForDetail = null;
      this.selectedCompanyIdForDetail = item.id;
      return;
    }
    this.selectedCompanyIdForDetail = null;
    this.selectedMasterIdForDetail = item.id;
  }

  closeDetail(): void {
    this.selectedMasterIdForDetail = null;
  }

  closeCompanyDetail(): void {
    this.selectedCompanyIdForDetail = null;
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
