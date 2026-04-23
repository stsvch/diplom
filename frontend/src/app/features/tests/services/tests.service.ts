import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  TestDetailDto,
  TestDto,
  QuestionDto,
  TestAttemptDto,
  TestAttemptDetailDto,
  TestAttemptStartDto,
} from '../models/test.model';

@Injectable({
  providedIn: 'root',
})
export class TestsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}`;

  // ── Tests (Teacher) ────────────────────────────────────────────────────────

  createTest(data: Partial<TestDto>): Observable<TestDetailDto> {
    return this.http.post<TestDetailDto>(`${this.base}/tests`, data);
  }

  getMyTests(): Observable<TestDto[]> {
    return this.http.get<TestDto[]>(`${this.base}/tests/my`);
  }

  getTest(id: string): Observable<TestDetailDto> {
    return this.http.get<TestDetailDto>(`${this.base}/tests/${id}`);
  }

  updateTest(id: string, data: Partial<TestDto>): Observable<TestDetailDto> {
    return this.http.put<TestDetailDto>(`${this.base}/tests/${id}`, data);
  }

  deleteTest(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/tests/${id}`);
  }

  getSubmissions(testId: string): Observable<TestAttemptDto[]> {
    return this.http.get<TestAttemptDto[]>(`${this.base}/tests/${testId}/submissions`);
  }

  // ── Questions (Teacher) ────────────────────────────────────────────────────

  createQuestion(testId: string, data: Partial<QuestionDto>): Observable<QuestionDto> {
    return this.http.post<QuestionDto>(`${this.base}/tests/${testId}/questions`, data);
  }

  updateQuestion(id: string, data: Partial<QuestionDto>): Observable<QuestionDto> {
    return this.http.put<QuestionDto>(`${this.base}/questions/${id}`, data);
  }

  deleteQuestion(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/questions/${id}`);
  }

  reorderQuestions(testId: string, orderedIds: string[]): Observable<void> {
    return this.http.post<void>(`${this.base}/tests/${testId}/questions/reorder`, { orderedIds });
  }

  // ── Attempts (Student) ─────────────────────────────────────────────────────

  startAttempt(testId: string): Observable<TestAttemptStartDto> {
    return this.http.post<TestAttemptStartDto>(`${this.base}/tests/${testId}/start`, {});
  }

  submitAnswer(
    attemptId: string,
    data: { questionId: string; selectedOptionIds?: string[]; textAnswer?: string },
  ): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.base}/attempts/${attemptId}/answer`, data);
  }

  submitAttempt(attemptId: string): Observable<TestAttemptDetailDto> {
    return this.http.post<TestAttemptDetailDto>(`${this.base}/attempts/${attemptId}/submit`, {});
  }

  getAttempt(attemptId: string): Observable<TestAttemptDetailDto> {
    return this.http.get<TestAttemptDetailDto>(`${this.base}/attempts/${attemptId}`);
  }

  getMyAttempts(testId: string): Observable<TestAttemptDto[]> {
    return this.http.get<TestAttemptDto[]>(`${this.base}/tests/${testId}/my-attempts`);
  }

  // ── Grading (Teacher) ──────────────────────────────────────────────────────

  gradeResponse(
    responseId: string,
    data: { points: number; comment?: string },
  ): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/responses/${responseId}/grade`, data);
  }
}
