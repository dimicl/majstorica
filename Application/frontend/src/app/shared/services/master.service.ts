import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
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
    if (p.entityType === 'masters') {
      return this.getMastersByGraphSearch(p).pipe(
        switchMap((graphPage) => {
          if (graphPage.items.length > 0) return of(graphPage);
          return this.getMastersFromDefaultEndpoint(p);
        }),
        catchError(() => this.getMastersFromDefaultEndpoint(p))
      );
    }

    return this.getMastersFromDefaultEndpoint(p);
  }

  private getMastersFromDefaultEndpoint(
    params: MastersListParams
  ): Observable<MastersListPage> {
    let url = `${this.API_URL}/masters`;
    const q = new URLSearchParams();
    if (params.search?.trim()) q.set('search', params.search.trim());
    if (params.sort) q.set('sort', params.sort);
    if (params.category?.trim()) q.set('category', params.category.trim());
    if (
      params.minRating != null &&
      params.minRating >= 1 &&
      params.minRating <= 5
    )
      q.set('minRating', String(params.minRating));
    if (params.entityType && params.entityType !== 'all')
      q.set('entityType', params.entityType);
    q.set('page', String(Math.max(1, params.page ?? 1)));
    q.set('pageSize', String(Math.min(50, Math.max(1, params.pageSize ?? 12))));
    const queryString = q.toString();
    if (queryString) url += '?' + queryString;

    return this.http.get<MastersListPage>(url).pipe(
      map((page) => ({
        ...page,
        items: page.items.map((item) => this.normalizeListItem(item)),
      }))
    );
  }

  private getMastersByGraphSearch(
    params: MastersListParams
  ): Observable<MastersListPage> {
    const page = Math.max(1, params.page ?? 1);
    const pageSize = Math.min(50, Math.max(1, params.pageSize ?? 12));

    const q = new URLSearchParams();
    if (params.category?.trim()) q.set('categoryNames', params.category.trim());
    if (
      params.minRating != null &&
      params.minRating >= 1 &&
      params.minRating <= 5
    )
      q.set('minRating', String(params.minRating));
    q.set('limit', '50');

    const queryString = q.toString();
    const url = queryString
      ? `${this.API_URL}/masters/search?${queryString}`
      : `${this.API_URL}/masters/search`;

    return this.http.get<MasterListItem[]>(url).pipe(
      map((items) => {
        const normalized = (items ?? []).map((item) =>
          this.normalizeListItem({ ...item, kind: 'master' })
        );
        const search = (params.search ?? '').trim().toLowerCase();

        let filtered = normalized;
        if (search) {
          filtered = filtered.filter((m) =>
            [m.firstName, m.lastName, m.username]
              .filter(Boolean)
              .join(' ')
              .toLowerCase()
              .includes(search)
          );
        }

        filtered = filtered.sort((a, b) => {
          const aName = `${a.firstName} ${a.lastName}`.trim().toLowerCase();
          const bName = `${b.firstName} ${b.lastName}`.trim().toLowerCase();
          const cmp = aName.localeCompare(bName);
          return params.sort === 'name-desc' ? -cmp : cmp;
        });

        const totalCount = filtered.length;
        const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
        const pageItems = filtered.slice(
          (page - 1) * pageSize,
          page * pageSize
        );

        return {
          items: pageItems,
          totalCount,
          page,
          pageSize,
          totalPages,
        };
      })
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
