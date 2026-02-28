import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const API_URL = 'http://localhost:5187/api';

export interface CreateJobPayload {
  title: string;
  description: string;
  /** ISO date (YYYY-MM-DD) kada klijent želi da majstor dođe, ili null. */
  scheduledDate: string | null;
  price?: number | null;
  isEmergency: boolean;
}

/** Jedan posao u listi (za majstora i klijenta). Status: Pending, Accepted, InProgress, Completed. */
export interface JobListItem {
  jobId: string;
  conversationId: string;
  jobTitle: string;
  description: string;
  clientName: string;
  masterName: string | null;
  date: string;
  clientId: string;
  price: number | null;
  isEmergency: boolean;
  status: string;
  createdAt: string;
  updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class JobService {
  private http = inject(HttpClient);

  /** Da li postoji job (zahtev) između trenutnog klijenta i datog majstora. */
  async hasSentRequestToMaster(masterId: string): Promise<boolean> {
    const res = await firstValueFrom(
      this.http.get<{ hasSentRequest: boolean }>(
        `${API_URL}/jobs/has-sent-request-to/${masterId}`
      )
    );
    return res?.hasSentRequest ?? false;
  }

  /** Kreira posao; vraća jobId. */
  async createJob(payload: CreateJobPayload): Promise<string> {
    const res = await firstValueFrom(
      this.http.post<string>(`${API_URL}/jobs`, {
        title: payload.title,
        description: payload.description,
        scheduledDate: payload.scheduledDate ?? null,
        price: payload.price ?? null,
        isEmergency: payload.isEmergency,
      })
    );
    return res ?? '';
  }

  /** Šalje zahtev izabranim majstorima (otvara konverzacije). */
  async sendRequests(jobId: string, masterIds: string[]): Promise<void> {
    await firstValueFrom(
      this.http.post(`${API_URL}/jobs/${jobId}/send-requests`, { masterIds })
    );
  }

  /** Svi poslovi za trenutnog korisnika (majstor: zahtevi na čekanju + dodeljeni, klijent: kreirani). */
  async getJobs(): Promise<JobListItem[]> {
    const list = await firstValueFrom(
      this.http.get<JobListItem[]>(`${API_URL}/jobs/list`)
    );
    return list ?? [];
  }

  /** Majstor prihvata posao. */
  async acceptJob(jobId: string): Promise<void> {
    await firstValueFrom(this.http.post(`${API_URL}/jobs/${jobId}/accept`, {}));
  }

  /** Majstor odbija zahtev (zatvara konverzaciju). */
  async declineRequest(conversationId: string): Promise<void> {
    await firstValueFrom(
      this.http.post(`${API_URL}/conversations/${conversationId}/decline`, {})
    );
  }
}
