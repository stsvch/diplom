// scheduling.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ScheduleSlotDto,
  UpdateSlotRequest,
  BookingDto,
  TeacherAvailabilityDto,
  CalendarSlotDto,
  CreateAvailabilityRequest,
  BookSlotRequest,
  TeacherWithScheduleDto,
} from '../models/scheduling.model';

// Сервис инкапсулирует бизнес-операции, состояние и обращения к API своего модуля.
@Injectable({ providedIn: 'root' })
export class SchedulingService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/schedule`;

  // ---- Учитель: правила расписания ----
  getMyAvailability(): Observable<TeacherAvailabilityDto[]> {
    // HTTP-вызов делегирует обмен с backend API и возвращает Observable вызывающему коду.
    return this.http.get<TeacherAvailabilityDto[]>(`${this.base}/availability/my`);
  }

  createAvailability(data: CreateAvailabilityRequest): Observable<TeacherAvailabilityDto> {
    return this.http.post<TeacherAvailabilityDto>(`${this.base}/availability`, data);
  }

  deleteAvailability(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/availability/${id}`);
  }

  // ---- Учитель: материализованные слоты ----
  getMySlots(status?: string): Observable<ScheduleSlotDto[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<ScheduleSlotDto[]>(`${this.base}/slots/my`, { params });
  }

  updateSlot(id: string, data: UpdateSlotRequest): Observable<ScheduleSlotDto> {
    return this.http.put<ScheduleSlotDto>(`${this.base}/slots/${id}`, data);
  }

  cancelSlot(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/slots/${id}/cancel`, {});
  }

  completeSlot(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/slots/${id}/complete`, {});
  }

  getSlotById(id: string): Observable<ScheduleSlotDto> {
    return this.http.get<ScheduleSlotDto>(`${this.base}/slots/${id}`);
  }

  getSlotBookings(id: string): Observable<BookingDto[]> {
    return this.http.get<BookingDto[]>(`${this.base}/slots/${id}/bookings`);
  }

  // ---- Студент: календарь учителя и бронирование ----
  getTeachersWithSchedule(): Observable<TeacherWithScheduleDto[]> {
    return this.http.get<TeacherWithScheduleDto[]>(`${this.base}/teachers`);
  }

  getTeacherCalendar(teacherId: string, from?: string, to?: string): Observable<CalendarSlotDto[]> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<CalendarSlotDto[]>(`${this.base}/teachers/${teacherId}/calendar`, { params });
  }

  bookSlot(data: BookSlotRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/book`, data);
  }

  cancelBooking(slotId: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/slots/${slotId}/book`);
  }

  getMyBookings(): Observable<ScheduleSlotDto[]> {
    return this.http.get<ScheduleSlotDto[]>(`${this.base}/my-bookings`);
  }
}
