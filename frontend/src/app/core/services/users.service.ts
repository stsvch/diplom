// users.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User } from '../models/user.model';

export interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  avatarUrl?: string | null;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

// Сервис инкапсулирует бизнес-операции, состояние и обращения к API своего модуля.
@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/users`;

  updateProfile(payload: UpdateProfilePayload): Observable<User> {
    // HTTP-вызов делегирует обмен с backend API и возвращает Observable вызывающему коду.
    return this.http.put<User>(`${this.base}/me`, payload);
  }

  changePassword(payload: ChangePasswordPayload): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/me/change-password`, payload);
  }
}
