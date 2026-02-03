import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';
import { AuthResponse, LoginRequest, RegisterRequest } from '../interfaces';

export interface UserResponse {
  user: User;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly API_URL = 'http://localhost:5187/api';

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, request);
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/register`, request);
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/auth/logout`, {});
  }

  getUser(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.API_URL}/user/getUser`);
  }

  // Čuva token u localStorage
  saveToken(token: string): void {
    localStorage.setItem('auth_token', token);
  }

  // Uzima token iz localStorage
  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  // Briše token iz localStorage
  removeToken(): void {
    localStorage.removeItem('auth_token');
  }

  // Proverava da li je korisnik ulogovan
  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
