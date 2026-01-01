import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../store/models/user.model';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
}

export interface AuthResponse {
  user: User;
  token: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private readonly API_URL = 'https://localhost:5001/api'; // Promeni prema tvom backend URL-u

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, {
      email,
      password,
    });
  }

  register(email: string, password: string, name: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/register`, {
      email,
      password,
      name,
    });
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/auth/logout`, {});
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

