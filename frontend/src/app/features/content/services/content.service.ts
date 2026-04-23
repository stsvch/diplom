import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  LessonBlockDto,
  LessonBlockData,
  LessonBlockSettings,
  LessonBlockType,
} from '../models';

export interface CreateLessonBlockPayload {
  lessonId: string;
  type: LessonBlockType;
  data: LessonBlockData;
  settings?: LessonBlockSettings;
}

export interface UpdateLessonBlockPayload {
  data: LessonBlockData;
  settings?: LessonBlockSettings;
}

@Injectable({ providedIn: 'root' })
export class ContentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/lesson-blocks`;

  getByLesson(lessonId: string): Observable<LessonBlockDto[]> {
    return this.http.get<LessonBlockDto[]>(`${this.base}/by-lesson/${lessonId}`);
  }

  create(payload: CreateLessonBlockPayload): Observable<LessonBlockDto> {
    return this.http.post<LessonBlockDto>(this.base, payload);
  }

  update(id: string, payload: UpdateLessonBlockPayload): Observable<LessonBlockDto> {
    return this.http.put<LessonBlockDto>(`${this.base}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  reorder(lessonId: string, orderedIds: string[]): Observable<void> {
    return this.http.post<void>(`${this.base}/reorder`, { lessonId, orderedIds });
  }

  executeCode(blockId: string, code: string): Observable<CodeExecutionResponse> {
    return this.http.post<CodeExecutionResponse>(`${this.base}/${blockId}/execute-code`, { code });
  }
}

export interface CodeExecutionCaseResult {
  input: string;
  expectedOutput: string;
  actualOutput: string;
  passed: boolean;
  isHidden: boolean;
  error?: string | null;
}

export interface CodeExecutionResponse {
  ok: boolean;
  results: CodeExecutionCaseResult[];
  globalError?: string | null;
}
