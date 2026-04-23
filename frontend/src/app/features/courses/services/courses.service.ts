import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CourseListDto,
  CourseDetailDto,
  CourseFilters,
  PagedResult,
  CourseModuleDto,
  LessonDto,
  LessonLayout,
} from '../models/course.model';

export interface PublishIssue {
  type: 'error' | 'warning';
  path: string;
  code: string;
  message: string;
}

export interface PublishValidationResponse {
  success: boolean;
  message: string;
  issues: PublishIssue[];
  hasErrors: boolean;
  hasWarnings: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class CoursesService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}`;

  // ── Courses ──────────────────────────────────────────────────────────────

  getCourses(filters: CourseFilters = {}): Observable<PagedResult<CourseListDto>> {
    let params = new HttpParams();
    if (filters.disciplineId) params = params.set('disciplineId', filters.disciplineId);
    if (filters.isFree !== undefined) params = params.set('isFree', String(filters.isFree));
    if (filters.level) params = params.set('level', filters.level);
    if (filters.search) params = params.set('search', filters.search);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.page !== undefined) params = params.set('page', String(filters.page));
    if (filters.pageSize !== undefined) params = params.set('pageSize', String(filters.pageSize));
    return this.http.get<PagedResult<CourseListDto>>(`${this.base}/courses`, { params });
  }

  getCourseById(id: string): Observable<CourseDetailDto> {
    return this.http.get<CourseDetailDto>(`${this.base}/courses/${id}`);
  }

  getMyCourses(): Observable<CourseListDto[]> {
    return this.http.get<CourseListDto[]>(`${this.base}/courses/my`);
  }

  createCourse(data: Partial<CourseDetailDto>): Observable<CourseDetailDto> {
    return this.http.post<CourseDetailDto>(`${this.base}/courses`, data);
  }

  updateCourse(id: string, data: Partial<CourseDetailDto>): Observable<CourseDetailDto> {
    return this.http.put<CourseDetailDto>(`${this.base}/courses/${id}`, data);
  }

  publishCourse(id: string, force = false): Observable<PublishValidationResponse> {
    const params = force ? '?force=true' : '';
    return this.http.post<PublishValidationResponse>(`${this.base}/courses/${id}/publish${params}`, {});
  }

  archiveCourse(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/courses/${id}/archive`, {});
  }

  deleteCourse(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/courses/${id}`);
  }

  enrollCourse(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/courses/${id}/enroll`, {});
  }

  unenrollCourse(id: string): Observable<void> {
    return this.http.post<void>(`${this.base}/courses/${id}/unenroll`, {});
  }

  // ── Modules ───────────────────────────────────────────────────────────────

  getModules(courseId: string): Observable<CourseModuleDto[]> {
    return this.http.get<CourseModuleDto[]>(`${this.base}/modules/by-course/${courseId}`);
  }

  createModule(courseId: string, data: { title: string; description?: string }): Observable<CourseModuleDto> {
    return this.http.post<CourseModuleDto>(`${this.base}/modules`, { ...data, courseId });
  }

  updateModule(id: string, data: { title: string; description?: string }): Observable<CourseModuleDto> {
    return this.http.put<CourseModuleDto>(`${this.base}/modules/${id}`, data);
  }

  deleteModule(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/modules/${id}`);
  }

  reorderModules(courseId: string, ids: string[]): Observable<void> {
    return this.http.post<void>(`${this.base}/modules/reorder`, { courseId, orderedIds: ids });
  }

  // ── Lessons ───────────────────────────────────────────────────────────────

  getLessons(moduleId: string): Observable<LessonDto[]> {
    return this.http.get<LessonDto[]>(`${this.base}/lessons/by-module/${moduleId}`);
  }

  getLessonById(id: string): Observable<LessonDto> {
    return this.http.get<LessonDto>(`${this.base}/lessons/${id}`);
  }

  createLesson(moduleId: string, data: { title: string; description?: string; duration?: number; layout?: LessonLayout }): Observable<LessonDto> {
    return this.http.post<LessonDto>(`${this.base}/lessons`, { ...data, moduleId });
  }

  updateLesson(id: string, data: { title?: string; description?: string; duration?: number; layout?: LessonLayout; moduleId?: string }): Observable<LessonDto> {
    return this.http.put<LessonDto>(`${this.base}/lessons/${id}`, data);
  }

  deleteLesson(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/lessons/${id}`);
  }

  reorderLessons(moduleId: string, orderedIds: string[]): Observable<void> {
    return this.http.post<void>(`${this.base}/lessons/reorder`, { moduleId, orderedIds });
  }
}
