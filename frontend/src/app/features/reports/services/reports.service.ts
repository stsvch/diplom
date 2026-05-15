// reports.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  StudentDashboardDto,
  TeacherCourseReportDto,
  TeacherDashboardDto,
} from '../models/reports.model';

// Сервис инкапсулирует бизнес-операции, состояние и обращения к API своего модуля.
@Injectable({ providedIn: 'root' })
export class ReportsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/reports`;

  getStudentDashboard(): Observable<StudentDashboardDto> {
    // HTTP-вызов делегирует обмен с backend API и возвращает Observable вызывающему коду.
    return this.http.get<StudentDashboardDto>(`${this.base}/student/dashboard`);
  }

  getTeacherDashboard(): Observable<TeacherDashboardDto> {
    return this.http.get<TeacherDashboardDto>(`${this.base}/teacher/dashboard`);
  }

  getTeacherCourseReport(courseId: string): Observable<TeacherCourseReportDto> {
    return this.http.get<TeacherCourseReportDto>(`${this.base}/teacher/courses/${courseId}`);
  }
}
