import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CalendarEventDto, CalendarEventType } from '../models/calendar.model';

export interface CreateCalendarEventDto {
  courseId?: string;
  title: string;
  description?: string;
  eventDate: string;
  eventTime?: string;
  type: CalendarEventType;
  sourceType?: string;
  sourceId?: string;
}

@Injectable({
  providedIn: 'root',
})
export class CalendarService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/calendar`;

  getMonthEvents(year: number, month: number): Observable<CalendarEventDto[]> {
    const params = new HttpParams().set('year', String(year)).set('month', String(month));
    return this.http.get<CalendarEventDto[]>(`${this.base}/events`, { params });
  }

  getUpcomingEvents(count = 10): Observable<CalendarEventDto[]> {
    const params = new HttpParams().set('count', String(count));
    return this.http.get<CalendarEventDto[]>(`${this.base}/upcoming`, { params });
  }

  createEvent(data: CreateCalendarEventDto): Observable<CalendarEventDto> {
    return this.http.post<CalendarEventDto>(`${this.base}/events`, data);
  }

  deleteEvent(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/events/${id}`);
  }
}
