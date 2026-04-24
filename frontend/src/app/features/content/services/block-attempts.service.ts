import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CodeExerciseRunDto,
  LessonBlockAnswer,
  LessonBlockAttemptDto,
  LessonProgressDto,
  SubmitAttemptResult,
} from '../models';

@Injectable({ providedIn: 'root' })
export class BlockAttemptsService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  submitAttempt(blockId: string, answers: LessonBlockAnswer): Observable<SubmitAttemptResult> {
    return this.http.post<SubmitAttemptResult>(
      `${this.base}/lesson-blocks/${blockId}/attempts`,
      { answers },
    );
  }

  getMyAttempt(blockId: string): Observable<LessonBlockAttemptDto | null> {
    return this.http.get<LessonBlockAttemptDto | null>(
      `${this.base}/lesson-blocks/${blockId}/my-attempt`,
    );
  }

  getMyCodeRuns(blockId: string, take = 10): Observable<CodeExerciseRunDto[]> {
    const params = new HttpParams().set('take', take);
    return this.http.get<CodeExerciseRunDto[]>(
      `${this.base}/lesson-blocks/${blockId}/my-code-runs`,
      { params },
    );
  }

  getMyLessonProgress(lessonId: string): Observable<LessonProgressDto> {
    return this.http.get<LessonProgressDto>(
      `${this.base}/lessons/${lessonId}/my-progress`,
    );
  }

  getLessonAttempts(lessonId: string, userId?: string): Observable<LessonBlockAttemptDto[]> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId);
    return this.http.get<LessonBlockAttemptDto[]>(
      `${this.base}/lessons/${lessonId}/attempts`,
      { params },
    );
  }

  getLessonCodeRuns(
    lessonId: string,
    filters: { blockId?: string; userId?: string; take?: number } = {},
  ): Observable<CodeExerciseRunDto[]> {
    let params = new HttpParams();
    if (filters.blockId) params = params.set('blockId', filters.blockId);
    if (filters.userId) params = params.set('userId', filters.userId);
    if (filters.take) params = params.set('take', filters.take);
    return this.http.get<CodeExerciseRunDto[]>(
      `${this.base}/lessons/${lessonId}/code-runs`,
      { params },
    );
  }

  reviewAttempt(attemptId: string, score: number, comment?: string): Observable<LessonBlockAttemptDto> {
    return this.http.post<LessonBlockAttemptDto>(
      `${this.base}/lesson-block-attempts/${attemptId}/review`,
      { score, comment },
    );
  }
}
