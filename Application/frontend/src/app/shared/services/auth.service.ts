import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';
import { AuthResponse, LoginRequest, RegisterRequest } from '../interfaces';

const AUTH_TOKEN_KEY = 'auth_token';
const AUTH_USER_ID_KEY = 'auth_user_id';

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
    return this.http.post<AuthResponse>(
      `${this.API_URL}/auth/register`,
      request
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/auth/logout`, {});
  }

  getUserById(id: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.API_URL}/user/${id}`);
  }

  saveToken(token: string): void {
    localStorage.setItem(AUTH_TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(AUTH_TOKEN_KEY);
  }

  removeToken(): void {
    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(AUTH_USER_ID_KEY);
  }

  /** Čuva samo id – da možeš proveriti "postoji token + id → zovi /me, setuj user" */
  saveUserId(userId: string): void {
    localStorage.setItem(AUTH_USER_ID_KEY, userId);
  }

  getUserIdFromStorage(): string | null {
    return localStorage.getItem(AUTH_USER_ID_KEY);
  }

  // Proverava da li je korisnik ulogovan
  isLoggedIn(): boolean {
    return !!this.getToken();
  }
}
