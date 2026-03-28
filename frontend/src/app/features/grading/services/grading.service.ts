import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  GradeDto,
  GradebookDto,
  GradebookStatsDto,
  CreateGradeDto,
  UpdateGradeDto,
} from '../models/grading.model';

@Injectable({
  providedIn: 'root',
})
export class GradingService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/grades`;

  createGrade(data: CreateGradeDto): Observable<GradeDto> {
    return this.http.post<GradeDto>(this.base, data);
  }

  updateGrade(id: string, data: UpdateGradeDto): Observable<GradeDto> {
    return this.http.put<GradeDto>(`${this.base}/${id}`, data);
  }

  deleteGrade(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  getCourseGradebook(courseId: string): Observable<GradebookDto> {
    return this.http.get<GradebookDto>(`${this.base}/course/${courseId}`);
  }

  getGradebookStats(courseId: string): Observable<GradebookStatsDto> {
    return this.http.get<GradebookStatsDto>(`${this.base}/course/${courseId}/stats`);
  }

  getStudentGrades(studentId: string): Observable<GradeDto[]> {
    return this.http.get<GradeDto[]>(`${this.base}/student/${studentId}`);
  }

  getMyGrades(): Observable<GradeDto[]> {
    return this.http.get<GradeDto[]>(`${this.base}/my`);
  }

  exportExcel(courseId: string): void {
    this.downloadFile(`${this.base}/course/${courseId}/export/excel`, `gradebook_${courseId}.xlsx`);
  }

  exportPdf(courseId: string): void {
    this.downloadFile(`${this.base}/course/${courseId}/export/pdf`, `gradebook_${courseId}.pdf`);
  }

  private downloadFile(url: string, filename: string): void {
    this.http.get(url, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        a.click();
        URL.revokeObjectURL(a.href);
      },
    });
  }
}
