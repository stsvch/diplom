import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ChatDto,
  MessageDto,
  SendMessageRequest,
  CreateDirectChatRequest,
  CreateCourseChatRequest,
} from '../models/messaging.model';

@Injectable({
  providedIn: 'root',
})
export class MessagingService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/chats`;

  getChats(): Observable<ChatDto[]> {
    return this.http.get<ChatDto[]>(this.base);
  }

  getChatById(chatId: string): Observable<ChatDto> {
    return this.http.get<ChatDto>(`${this.base}/${chatId}`);
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.base}/unread-count`);
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

  deleteMessage(messageId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${environment.apiUrl}/messages/${messageId}`);
  }
}
