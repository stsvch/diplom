import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DisciplineDto } from '../models/course.model';

@Injectable({
  providedIn: 'root',
})
export class DisciplinesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/disciplines`;

  getAll(): Observable<DisciplineDto[]> {
    return this.http.get<DisciplineDto[]>(this.base);
  }

  create(data: { name: string; description?: string; imageUrl?: string }): Observable<DisciplineDto> {
    return this.http.post<DisciplineDto>(this.base, data);
  }

  update(id: string, data: { name: string; description?: string; imageUrl?: string }): Observable<DisciplineDto> {
    return this.http.put<DisciplineDto>(`${this.base}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
