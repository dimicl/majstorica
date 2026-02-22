import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { MasterProfile } from '../models/master.model';
import { MasterListItem, UserResponse } from '../interfaces';
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

  getMasters(): Observable<MasterListItem[]> {
    return this.http.get<MasterListItem[]>(`${this.API_URL}/masters`);
  }

  /** Podaci o korisniku (majstoru) po id – koristi zajednički user API preko AuthService. */
  async getMasterById(id: string): Promise<UserResponse | null> {
    try {
      return await firstValueFrom(this.auth.getUserById(id));
    } catch {
      return null;
    }
  }
}
