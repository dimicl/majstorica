import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { MasterProfile } from '../models/master.model';

export interface MasterListItem {
  id: string;
  firstName: string;
  lastName: string;
  username: string;
}

export interface UserResponse {
  user: {
    id: string;
    username: string;
    firstName: string;
    lastName: string;
    role: string;
    phone?: string | null;
    deliveryAddress?: string | null;
    description?: string | null;
  };
}

@Injectable({
  providedIn: 'root',
})
export class MasterService {
  private http = inject(HttpClient);
  private readonly API_URL = 'http://localhost:5187/api';

  getMaster(): Observable<MasterProfile> {
    return this.http.get<MasterProfile>(`${this.API_URL}/master/getMaster`);
  }

  getMasters(): Observable<MasterListItem[]> {
    return this.http.get<MasterListItem[]>(`${this.API_URL}/masters`);
  }

  async getMasterById(id: string): Promise<UserResponse | null> {
    try {
      return await firstValueFrom(
        this.http.get<UserResponse>(`${this.API_URL}/user/${id}`)
      );
    } catch {
      return null;
    }
  }
}
