import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  LessonProgressDto,
  CourseProgressDto,
  MyProgressDto,
} from '../models/progress.model';

@Injectable({
  providedIn: 'root',
})
export class ProgressService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/progress`;

  completeLesson(lessonId: string): Observable<LessonProgressDto> {
    return this.http.post<LessonProgressDto>(`${this.base}/lessons/${lessonId}/complete`, {});
  }

  uncompleteLesson(lessonId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/lessons/${lessonId}/complete`);
  }

  getCourseProgress(courseId: string): Observable<CourseProgressDto> {
    return this.http.get<CourseProgressDto>(`${this.base}/courses/${courseId}`);
  }

  getMyProgress(): Observable<MyProgressDto> {
    return this.http.get<MyProgressDto>(`${this.base}/my`);
  }

  getLessonProgress(lessonId: string): Observable<LessonProgressDto> {
    return this.http.get<LessonProgressDto>(`${this.base}/lessons/${lessonId}`);
  }
}
