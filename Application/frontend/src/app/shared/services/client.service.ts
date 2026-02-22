import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClientProfile } from '../models/client.model';
import { API_BASE_URL } from '../constants/api.constants';

@Injectable({
  providedIn: 'root',
})
export class ClientService {
  private http = inject(HttpClient);
  private readonly API_URL = API_BASE_URL;

  getClient(): Observable<ClientProfile> {
    return this.http.get<ClientProfile>(`${this.API_URL}/client/getClient`);
  }
}
