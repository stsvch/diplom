import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AdminUserDto,
  AdminCourseDto,
  PlatformSettingsDto,
  DashboardStatsDto,
  CreateUserRequest,
  ChangeRoleRequest,
  ForceArchiveRequest,
} from '../models/admin.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/admin`;

  // Users
  getUsers(params: { search?: string; role?: string; onlyBlocked?: boolean; page?: number; pageSize?: number }): Observable<PagedResult<AdminUserDto>> {
    let httpParams = new HttpParams();
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.role) httpParams = httpParams.set('role', params.role);
    if (params.onlyBlocked !== undefined) httpParams = httpParams.set('onlyBlocked', String(params.onlyBlocked));
    if (params.page !== undefined) httpParams = httpParams.set('page', String(params.page));
    if (params.pageSize !== undefined) httpParams = httpParams.set('pageSize', String(params.pageSize));
    return this.http.get<PagedResult<AdminUserDto>>(`${this.base}/users`, { params: httpParams });
  }

  createUser(request: CreateUserRequest): Observable<AdminUserDto> {
    return this.http.post<AdminUserDto>(`${this.base}/users`, request);
  }

  blockUser(userId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/users/${userId}/block`, {});
  }

  unblockUser(userId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/users/${userId}/unblock`, {});
  }

  changeUserRole(userId: string, request: ChangeRoleRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/users/${userId}/role`, request);
  }

  deleteUser(userId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/users/${userId}`);
  }

  // Courses
  getCourses(params: { search?: string; status?: string; disciplineId?: string; page?: number; pageSize?: number }): Observable<PagedResult<AdminCourseDto>> {
    let httpParams = new HttpParams();
    if (params.search) httpParams = httpParams.set('search', params.search);
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.disciplineId) httpParams = httpParams.set('disciplineId', params.disciplineId);
    if (params.page !== undefined) httpParams = httpParams.set('page', String(params.page));
    if (params.pageSize !== undefined) httpParams = httpParams.set('pageSize', String(params.pageSize));
    return this.http.get<PagedResult<AdminCourseDto>>(`${this.base}/courses`, { params: httpParams });
  }

  forceArchiveCourse(id: string, request: ForceArchiveRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/courses/${id}/force-archive`, request);
  }

  // Settings
  getSettings(): Observable<PlatformSettingsDto> {
    return this.http.get<PlatformSettingsDto>(`${environment.apiUrl}/platform-settings`);
  }

  updateSettings(settings: PlatformSettingsDto): Observable<PlatformSettingsDto> {
    return this.http.put<PlatformSettingsDto>(`${environment.apiUrl}/platform-settings`, settings);
  }

  // Stats
  getDashboard(): Observable<DashboardStatsDto> {
    return this.http.get<DashboardStatsDto>(`${this.base}/stats/dashboard`);
  }
}
