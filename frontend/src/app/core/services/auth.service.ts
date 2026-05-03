import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, tap, throwError } from 'rxjs';
import { User, UserRole } from '../models/user.model';
import { environment } from '../../../environments/environment';

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}

export interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
}

const TOKEN_KEY = 'access_token';
const USER_KEY = 'current_user';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  readonly currentUser = signal<User | null>(null);
  readonly accessToken = signal<string | null>(null);

  readonly isAuthenticated = computed(() => this.accessToken() !== null && this.currentUser() !== null);

  readonly userRole = computed<UserRole | null>(() => {
    const user = this.currentUser();
    return user ? user.role : null;
  });

  constructor() {
    this.restoreSession();
  }

  private restoreSession(): void {
    if (this.ensureSessionRestored()) {
      // Validate token by fetching fresh profile
      this.fetchProfile().subscribe({
        error: (error: unknown) => {
          if (error instanceof HttpErrorResponse && (error.status === 401 || error.status === 403)) {
            this.clearSession();
          }
        },
      });
    }
  }

  ensureSessionRestored(): boolean {
    if (this.isAuthenticated()) {
      return true;
    }

    const token = localStorage.getItem(TOKEN_KEY);
    const user = this.readStoredUser();
    if (!token || !user) {
      return false;
    }

    this.accessToken.set(token);
    this.currentUser.set(user);
    return true;
  }

  getStoredUserRole(): UserRole | null {
    return this.readStoredUser()?.role ?? null;
  }

  private readStoredUser(): User | null {
    const userJson = localStorage.getItem(USER_KEY);
    if (!userJson) {
      return null;
    }

    try {
      return JSON.parse(userJson) as User;
    } catch {
      this.clearSession();
      return null;
    }
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password }, { withCredentials: true })
      .pipe(
        tap((res) => {
          this.accessToken.set(res.accessToken);
          localStorage.setItem(TOKEN_KEY, res.accessToken);
          this.fetchProfile().subscribe({
            next: (user) => {
              this.navigateToDashboard(user.role);
            },
          });
        }),
        catchError((err) => throwError(() => err)),
      );
  }

  register(data: RegisterData): Observable<{ message: string }> {
    return this.http
      .post<{ message: string }>(`${environment.apiUrl}/auth/register`, data)
      .pipe(catchError((err) => throwError(() => err)));
  }

  confirmEmail(userId: string, token: string): Observable<{ message: string }> {
    return this.http
      .get<{ message: string }>(`${environment.apiUrl}/auth/confirm-email`, {
        params: { userId, token },
      })
      .pipe(catchError((err) => throwError(() => err)));
  }

  forgotPassword(email: string): Observable<{ message: string }> {
    return this.http
      .post<{ message: string }>(`${environment.apiUrl}/auth/forgot-password`, { email })
      .pipe(catchError((err) => throwError(() => err)));
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<{ message: string }> {
    return this.http
      .post<{ message: string }>(`${environment.apiUrl}/auth/reset-password`, { email, token, newPassword })
      .pipe(catchError((err) => throwError(() => err)));
  }

  refreshToken(): Observable<LoginResponse> {
    const expiredToken = this.accessToken() || localStorage.getItem(TOKEN_KEY) || '';
    return this.http
      .post<LoginResponse>(
        `${environment.apiUrl}/auth/refresh`,
        { accessToken: expiredToken },
        { withCredentials: true },
      )
      .pipe(
        tap((res) => {
          this.accessToken.set(res.accessToken);
          localStorage.setItem(TOKEN_KEY, res.accessToken);
        }),
        catchError((err) => throwError(() => err)),
      );
  }

  fetchProfile(): Observable<User> {
    return this.http.get<User>(`${environment.apiUrl}/users/me`).pipe(
      tap((user) => {
        this.currentUser.set(user);
        localStorage.setItem(USER_KEY, JSON.stringify(user));
      }),
      catchError((err) => throwError(() => err)),
    );
  }

  logout(): void {
    this.http
      .post(`${environment.apiUrl}/auth/logout`, {}, { withCredentials: true })
      .pipe(catchError(() => []))
      .subscribe({
        next: () => {
          this.clearSession();
          this.router.navigate(['/login']);
        },
        error: () => {
          this.clearSession();
          this.router.navigate(['/login']);
        },
        complete: () => {
          this.clearSession();
          this.router.navigate(['/login']);
        },
      });
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  private navigateToDashboard(role: UserRole): void {
    switch (role) {
      case UserRole.Student:
        this.router.navigate(['/student/dashboard']);
        break;
      case UserRole.Teacher:
        this.router.navigate(['/teacher/dashboard']);
        break;
      case UserRole.Admin:
        this.router.navigate(['/admin/dashboard']);
        break;
      default:
        this.router.navigate(['/']);
    }
  }

  private clearSession(): void {
    this.accessToken.set(null);
    this.currentUser.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }
}
