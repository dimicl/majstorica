import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { MasterProfile } from '../models/master.model';

@Injectable({
  providedIn: 'root',
})
export class MasterService {
  private http = inject(HttpClient);
  private readonly API_URL = 'http://localhost:5187/api';

  getMaster(): Observable<MasterProfile> {
    return this.http.get<MasterProfile>(`${this.API_URL}/master/getMaster`);
  }
}
