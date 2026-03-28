import { Injectable, inject, signal, effect } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { MessageDto, AttachmentDto } from '../../features/messaging/models/messaging.model';

@Injectable({
  providedIn: 'root',
})
export class ChatSignalRService {
  private readonly authService = inject(AuthService);
  private hubConnection: signalR.HubConnection | null = null;

  readonly lastMessage = signal<MessageDto | null>(null);
  readonly messagesRead = signal<{ chatId: string; userId: string } | null>(null);

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
    if (
      this.hubConnection &&
      this.hubConnection.state !== signalR.HubConnectionState.Disconnected
    ) {
      return;
    }

    const baseUrl = environment.apiUrl.replace('/api', '');

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/chat`, {
        accessTokenFactory: () => this.authService.getAccessToken()!,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveMessage', (message: MessageDto) => {
      this.lastMessage.set(message);
    });

    this.hubConnection.on('MessagesRead', (chatId: string, userId: string) => {
      this.messagesRead.set({ chatId, userId });
    });

    this.hubConnection
      .start()
      .catch((err) => console.error('ChatHub connection error:', err));
  }

  private stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
    }
  }

  sendMessage(chatId: string, text: string, attachments?: AttachmentDto[]): void {
    if (!this.hubConnection) return;
    this.hubConnection
      .invoke('SendMessage', chatId, text, attachments ?? null)
      .catch((err) => console.error('SendMessage error:', err));
  }

  joinChat(chatId: string): void {
    if (!this.hubConnection) return;
    this.hubConnection
      .invoke('JoinChat', chatId)
      .catch((err) => console.error('JoinChat error:', err));
  }

  markAsRead(chatId: string): void {
    if (!this.hubConnection) return;
    this.hubConnection
      .invoke('MarkAsRead', chatId)
      .catch((err) => console.error('MarkAsRead error:', err));
  }
}
