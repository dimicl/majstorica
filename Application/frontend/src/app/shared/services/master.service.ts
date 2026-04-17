import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  MasterProfile,
  MasterProfileResponse,
  type MasterReviewListItem,
  type UpdateMasterProfileStatsPayload,
} from '../models/master.model';
import {
  DEFAULT_MASTERS_LIST_PARAMS,
  MasterListItem,
  type MastersListPage,
  type MastersListParams,
} from '../interfaces';
import { UserResponse } from '../interfaces';
import { AuthService } from './auth.service';
import { API_BASE_URL } from '../constants/api.constants';

@Injectable({
  providedIn: 'root',
})
export class MasterService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private readonly API_URL = API_BASE_URL;

  getMaster(): Observable<MasterProfile> {
    return this.http.get<MasterProfile>(`${this.API_URL}/master/getMaster`);
  }

  /** Profil trenutnog majstora (user + kategorija, ocena). GET api/masters/profile */
  getMyMasterProfile(): Observable<MasterProfileResponse> {
    return this.http.get<MasterProfileResponse>(
      `${this.API_URL}/masters/profile`
    );
  }

  /** Recenzije na trenutnog majstora (Master / CompanyWorker). */
  getMyMasterReviews(): Observable<MasterReviewListItem[]> {
    return this.http.get<MasterReviewListItem[]>(
      `${this.API_URL}/masters/profile/reviews`
    );
  }

  updateCategory(category: string): Observable<void> {
    return this.http.patch<void>(`${this.API_URL}/masters/category`, {
      category,
    });
  }

  patchProfileStats(
    payload: UpdateMasterProfileStatsPayload
  ): Observable<void> {
    return this.http.patch<void>(
      `${this.API_URL}/masters/profile/stats`,
      payload
    );
  }

  /** Preporučeni majstori za klijenta (Neo4j – ista veština kao već angažovani). Za ne-klijente backend vraća praznu listu. */
  getRecommendedMasters(limit = 10): Observable<MasterListItem[]> {
    const url = `${this.API_URL}/masters/recommended?limit=${Math.min(
      20,
      Math.max(1, limit)
    )}`;
    return this.http.get<MasterListItem[]>(url);
  }

  /** Lista majstora i firmi – paginacija, filteri (keš na backendu). */
  getMasters(params?: MastersListParams): Observable<MastersListPage> {
    const p = params ?? DEFAULT_MASTERS_LIST_PARAMS;
    let url = `${this.API_URL}/masters`;
    const q = new URLSearchParams();
    if (p.search?.trim()) q.set('search', p.search.trim());
    if (p.sort) q.set('sort', p.sort);
    if (p.category?.trim()) q.set('category', p.category.trim());
    if (
      p.minRating != null &&
      p.minRating >= 1 &&
      p.minRating <= 5
    )
      q.set('minRating', String(p.minRating));
    if (p.entityType && p.entityType !== 'all')
      q.set('entityType', p.entityType);
    q.set('page', String(Math.max(1, p.page ?? 1)));
    q.set(
      'pageSize',
      String(Math.min(50, Math.max(1, p.pageSize ?? 12)))
    );
    const queryString = q.toString();
    if (queryString) url += '?' + queryString;
    return this.http.get<MastersListPage>(url).pipe(
      map((page) => ({
        ...page,
        items: page.items.map((item) => this.normalizeListItem(item)),
      }))
    );
  }

  private normalizeListItem(item: MasterListItem): MasterListItem {
    const kind = item.kind === 'company' ? 'company' : 'master';
    return { ...item, kind };
  }

  async getMasterById(id: string): Promise<UserResponse | null> {
    try {
      return await firstValueFrom(this.auth.getUserById(id));
    } catch {
      return null;
    }
  }
}
