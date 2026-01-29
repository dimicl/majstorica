import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ClientProfile } from '../models/client.model';

@Injectable({
  providedIn: 'root',
})
export class ClientService {
  private http = inject(HttpClient);
  private readonly API_URL = 'https://localhost:5001/api';

  getClient(): Observable<ClientProfile> {
    return this.http.get<ClientProfile>(`${this.API_URL}/client/getClient`);
  }
}
