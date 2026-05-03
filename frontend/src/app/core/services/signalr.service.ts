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
  private readonly unloadHandler = () => this.disposeForUnload();

  readonly unreadCount = signal<number>(0);
  readonly lastNotification = signal<NotificationDto | null>(null);

  constructor() {
    if (typeof window !== 'undefined') {
      window.addEventListener('beforeunload', this.unloadHandler);
    }

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
    const transport = environment.production
      ? signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      : signalR.HttpTransportType.LongPolling;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/notifications`, {
        accessTokenFactory: () => this.authService.getAccessToken() ?? '',
        transport,
      })
      .configureLogging(signalR.LogLevel.None)
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
      void this.hubConnection.stop().catch(() => undefined);
      this.hubConnection = null;
    }
    this.unreadCount.set(0);
    this.lastNotification.set(null);
  }

  private disposeForUnload(): void {
    if (!this.hubConnection) {
      return;
    }

    const connection = this.hubConnection;
    this.hubConnection = null;
    void connection.stop().catch(() => undefined);
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
