import { Injectable, inject, signal, effect } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { MessageDto, AttachmentDto, ParticipantDto } from '../../features/messaging/models/messaging.model';

@Injectable({
  providedIn: 'root',
})
export class ChatSignalRService {
  private readonly authService = inject(AuthService);
  private hubConnection: signalR.HubConnection | null = null;

  readonly unreadCount = signal(0);
  readonly lastMessage = signal<MessageDto | null>(null);
  readonly messageEdited = signal<MessageDto | null>(null);
  readonly messageDeleted = signal<{ chatId: string; messageId: string } | null>(null);
  readonly messagesRead = signal<{ chatId: string; userId: string } | null>(null);
  readonly participantAdded = signal<{ chatId: string; participant: ParticipantDto } | null>(null);
  readonly participantRemoved = signal<{ chatId: string; userId: string } | null>(null);
  readonly removedFromChat = signal<string | null>(null);
  readonly chatArchived = signal<string | null>(null);
  readonly chatDeleted = signal<string | null>(null);

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
      if (message.senderId !== this.authService.currentUser()?.id) {
        this.adjustUnreadCount(1);
      }
    });

    this.hubConnection.on('ReceiveMessageEdited', (message: MessageDto) => {
      this.messageEdited.set(message);
    });

    this.hubConnection.on('ReceiveMessageDeleted', (chatId: string, messageId: string) => {
      this.messageDeleted.set({ chatId, messageId });
    });

    this.hubConnection.on('MessagesRead', (chatId: string, userId: string) => {
      this.messagesRead.set({ chatId, userId });
    });

    this.hubConnection.on('ParticipantAdded', (chatId: string, participant: ParticipantDto) => {
      this.participantAdded.set({ chatId, participant });
    });

    this.hubConnection.on('ParticipantRemoved', (chatId: string, userId: string) => {
      this.participantRemoved.set({ chatId, userId });
    });

    this.hubConnection.on('RemovedFromChat', (chatId: string) => {
      this.removedFromChat.set(chatId);
    });

    this.hubConnection.on('ChatArchived', (chatId: string) => {
      this.chatArchived.set(chatId);
    });

    this.hubConnection.on('ChatDeleted', (chatId: string) => {
      this.chatDeleted.set(chatId);
    });

    this.hubConnection.on('JoinChatInstruction', (chatId: string) => {
      this.joinChat(chatId);
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
    this.unreadCount.set(0);
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
