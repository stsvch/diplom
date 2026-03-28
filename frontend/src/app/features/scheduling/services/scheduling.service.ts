import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ScheduleSlotDto, CreateSlotRequest, UpdateSlotRequest, BookingDto } from '../models/scheduling.model';

@Injectable({
  providedIn: 'root',
})
export class SchedulingService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/schedule`;

  // Teacher endpoints
  createSlot(data: CreateSlotRequest): Observable<ScheduleSlotDto> {
    return this.http.post<ScheduleSlotDto>(`${this.base}/slots`, data);
  }

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

  // Student endpoints
  getAvailableSlots(): Observable<ScheduleSlotDto[]> {
    return this.http.get<ScheduleSlotDto[]>(`${this.base}/available`);
  }

  bookSlot(id: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/slots/${id}/book`, {});
  }

  cancelBooking(id: string): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.base}/slots/${id}/book`);
  }

  getMyBookings(): Observable<ScheduleSlotDto[]> {
    return this.http.get<ScheduleSlotDto[]>(`${this.base}/my-bookings`);
  }
}
