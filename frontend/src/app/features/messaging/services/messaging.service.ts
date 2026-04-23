import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ChatDto,
  MessageDto,
  SendMessageRequest,
  CreateDirectChatRequest,
  CreateCourseChatRequest,
  AddParticipantRequest,
  EditMessageRequest,
  UserSummaryDto,
} from '../models/messaging.model';
import { ChatSignalRService } from '../../../core/services/chat-signalr.service';

@Injectable({
  providedIn: 'root',
})
export class MessagingService {
  private readonly http = inject(HttpClient);
  private readonly chatSignalRService = inject(ChatSignalRService);
  private readonly base = `${environment.apiUrl}/chats`;

  getChats(): Observable<ChatDto[]> {
    return this.http.get<ChatDto[]>(this.base);
  }

  getChatById(chatId: string): Observable<ChatDto> {
    return this.http.get<ChatDto>(`${this.base}/${chatId}`);
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.base}/unread-count`).pipe(
      tap((res) => this.chatSignalRService.setUnreadCount(res.count)),
    );
  }

  createDirectChat(request: CreateDirectChatRequest): Observable<ChatDto> {
    return this.http.post<ChatDto>(`${this.base}/direct`, request);
  }

  createCourseChat(request: CreateCourseChatRequest): Observable<ChatDto> {
    return this.http.post<ChatDto>(`${this.base}/course`, request);
  }

  getChatMessages(chatId: string, page = 1, pageSize = 50): Observable<MessageDto[]> {
    const params = new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
    return this.http.get<MessageDto[]>(`${this.base}/${chatId}/messages`, { params });
  }

  sendMessage(chatId: string, request: SendMessageRequest): Observable<MessageDto> {
    return this.http.post<MessageDto>(`${this.base}/${chatId}/messages`, request);
  }

  markAsRead(chatId: string): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/${chatId}/read`, {});
  }

  hideChat(chatId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/${chatId}/hide`, {});
  }

  deleteChat(chatId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/${chatId}`);
  }

  addParticipant(chatId: string, request: AddParticipantRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/${chatId}/participants`, request);
  }

  removeParticipant(chatId: string, participantId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/${chatId}/participants/${participantId}`);
  }

  editMessage(messageId: string, text: string): Observable<MessageDto> {
    const payload: EditMessageRequest = { text };
    return this.http.put<MessageDto>(`${environment.apiUrl}/messages/${messageId}`, payload);
  }

  deleteMessage(messageId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${environment.apiUrl}/messages/${messageId}`);
  }

  searchUsers(q: string, role?: string, limit = 20): Observable<UserSummaryDto[]> {
    let params = new HttpParams().set('limit', String(limit));
    if (q) params = params.set('q', q);
    if (role) params = params.set('role', role);
    return this.http.get<UserSummaryDto[]>(`${environment.apiUrl}/users/search`, { params });
  }
}
