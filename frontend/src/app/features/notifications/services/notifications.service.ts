import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { NotificationDto, NotificationType, PagedResult } from '../models/notification.model';
import { SignalRService } from '../../../core/services/signalr.service';

@Injectable({
  providedIn: 'root',
})
export class NotificationsService {
  private readonly http = inject(HttpClient);
  private readonly signalRService = inject(SignalRService);
  private readonly base = `${environment.apiUrl}/notifications`;

  getNotifications(params: {
    type?: NotificationType;
    isRead?: boolean;
    page?: number;
    pageSize?: number;
  }): Observable<PagedResult<NotificationDto>> {
    let httpParams = new HttpParams();
    if (params.type !== undefined) httpParams = httpParams.set('type', params.type);
    if (params.isRead !== undefined) httpParams = httpParams.set('isRead', String(params.isRead));
    if (params.page !== undefined) httpParams = httpParams.set('page', String(params.page));
    if (params.pageSize !== undefined) httpParams = httpParams.set('pageSize', String(params.pageSize));

    return this.http.get<PagedResult<NotificationDto>>(this.base, { params: httpParams });
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.base}/unread-count`).pipe(
      tap((res) => this.signalRService.setUnreadCount(res.count)),
    );
  }

  markAsRead(id: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/${id}/read`, {}).pipe(
      tap(() => this.signalRService.decrementUnreadCount()),
    );
  }

  markAllAsRead(): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/read-all`, {}).pipe(
      tap(() => this.signalRService.setUnreadCount(0)),
    );
  }

  deleteNotification(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/${id}`);
  }
}
