import { Injectable, inject, signal, effect } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { NotificationDto } from '../../features/notifications/models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  private readonly authService = inject(AuthService);
  private hubConnection: signalR.HubConnection | null = null;

  readonly unreadCount = signal<number>(0);
  readonly lastNotification = signal<NotificationDto | null>(null);

  constructor() {
    effect(() => {
      const isAuthenticated = this.authService.isAuthenticated();
      if (isAuthenticated) {
        this.startConnection();
      } else {
        this.stopConnection();
      }
    });
  }

  private startConnection(): void {
    if (this.hubConnection && this.hubConnection.state !== signalR.HubConnectionState.Disconnected) {
      return;
    }

    const baseUrl = environment.apiUrl.replace('/api', '');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/notifications`, {
        accessTokenFactory: () => this.authService.getAccessToken()!,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: NotificationDto) => {
      this.lastNotification.set(notification);
      this.adjustUnreadCount(1);
    });

    this.hubConnection.start().catch((err) => console.error('SignalR connection error:', err));
  }

  private stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
    this.unreadCount.set(0);
    this.lastNotification.set(null);
  }

  setUnreadCount(count: number): void {
    this.unreadCount.set(count);
  }

  adjustUnreadCount(delta: number): void {
    this.unreadCount.update((count) => Math.max(0, count + delta));
  }

  decrementUnreadCount(): void {
    this.adjustUnreadCount(-1);
  }
}
