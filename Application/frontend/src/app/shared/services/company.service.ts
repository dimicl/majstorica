import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
  HttpStatusCode,
} from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, Observable, of, throwError } from 'rxjs';
import { API_BASE_URL } from '../constants/api.constants';
import { AuthResponse } from '../interfaces/auth-response.interface';
import {
  CompanyDto,
  CompanyInvitationPending,
  CompanyPublicDto,
  CompanyWorkerMember,
  CreateCompanyPayload,
  MasterSearchForInviteItem,
} from '../interfaces/company.interface';

@Injectable({
  providedIn: 'root',
})
export class CompanyService {
  private http = inject(HttpClient);
  private readonly base = `${API_BASE_URL}/companies`;

  getPublicCompany(id: string): Observable<CompanyPublicDto> {
    return this.http.get<CompanyPublicDto>(`${this.base}/${id}/public`);
  }

  getMyCompany(): Observable<CompanyDto | null> {
    return this.http.get<CompanyDto>(`${this.base}/mine`).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === HttpStatusCode.NotFound) return of(null);
        return throwError(() => err);
      })
    );
  }

  createCompany(payload: CreateCompanyPayload): Observable<CompanyDto> {
    return this.http.post<CompanyDto>(this.base, payload);
  }

  searchMastersForInvite(
    q: string,
    limit = 15
  ): Observable<MasterSearchForInviteItem[]> {
    const params = new HttpParams()
      .set('q', (q ?? '').trim())
      .set('limit', String(limit));
    return this.http.get<MasterSearchForInviteItem[]>(
      `${this.base}/mine/masters/search`,
      { params }
    );
  }

  inviteMaster(masterUserId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/mine/invitations`, {
      masterUserId,
    });
  }

  /** Majstori kojima je firma poslala poziv koji još čeka (vlasnik firme). */
  getPendingOutboundInviteRecipients(): Observable<string[]> {
    return this.http.get<string[]>(
      `${this.base}/mine/invitations/pending-recipients`
    );
  }

  getMyCompanyWorkers(): Observable<CompanyWorkerMember[]> {
    return this.http.get<CompanyWorkerMember[]>(`${this.base}/mine/workers`);
  }

  getMyPendingCompanyInvitations(): Observable<CompanyInvitationPending[]> {
    return this.http.get<CompanyInvitationPending[]>(
      `${this.base}/invitations/mine-pending`
    );
  }

  acceptCompanyInvitation(invitationId: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(
      `${this.base}/invitations/${invitationId}/accept`,
      {}
    );
  }

  declineCompanyInvitation(invitationId: string): Observable<void> {
    return this.http.post<void>(
      `${this.base}/invitations/${invitationId}/decline`,
      {}
    );
  }

  static mapApiError(err: HttpErrorResponse): string {
    const body = err.error as { message?: string } | undefined;
    if (body && typeof body.message === 'string') return body.message;
    return 'Došlo je do greške. Pokušaj ponovo.';
  }
}
