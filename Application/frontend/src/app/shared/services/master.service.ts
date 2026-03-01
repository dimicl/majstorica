import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { MasterProfile, MasterProfileResponse } from '../models/master.model';
import { MasterListItem, type MastersListParams } from '../interfaces';
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
    return this.http.get<MasterProfileResponse>(`${this.API_URL}/masters/profile`);
  }

  /** Ažurira kategoriju majstora. category = prikazno ime (npr. "Električar") ili null da ukloni. */
  updateMyCategory(category: string | null): Observable<void> {
    return this.http.patch<void>(`${this.API_URL}/masters/category`, {
      category: category ?? null,
    });
  }

  /** Preporučeni majstori za klijenta (Neo4j – ista veština kao već angažovani). Za ne-klijente backend vraća praznu listu. */
  getRecommendedMasters(limit = 10): Observable<MasterListItem[]> {
    const url = `${this.API_URL}/masters/recommended?limit=${Math.min(20, Math.max(1, limit))}`;
    return this.http.get<MasterListItem[]>(url);
  }

  /** Lista majstora – opciono sa parametrima za filter/sort (keš na backendu). */
  getMasters(params?: MastersListParams): Observable<MasterListItem[]> {
    let url = `${this.API_URL}/masters`;
    if (params) {
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
      const queryString = q.toString();
      if (queryString) url += '?' + queryString;
    }
    return this.http.get<MasterListItem[]>(url);
  }

  async getMasterById(id: string): Promise<UserResponse | null> {
    try {
      return await firstValueFrom(this.auth.getUserById(id));
    } catch {
      return null;
    }
  }
}
