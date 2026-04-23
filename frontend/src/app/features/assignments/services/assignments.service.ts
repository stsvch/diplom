import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AssignmentDto,
  AssignmentDetailDto,
  SubmissionDto,
  CreateAssignmentDto,
  GradeSubmissionDto,
} from '../models/assignment.model';

@Injectable({
  providedIn: 'root',
})
export class AssignmentsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}`;

  // ── Assignments (Teacher) ─────────────────────────────────────────────────

  createAssignment(data: CreateAssignmentDto): Observable<AssignmentDto> {
    return this.http.post<AssignmentDto>(`${this.base}/assignments`, data);
  }

  getMyAssignments(): Observable<AssignmentDto[]> {
    return this.http.get<AssignmentDto[]>(`${this.base}/assignments/my`);
  }

  getAssignment(id: string): Observable<AssignmentDetailDto> {
    return this.http.get<AssignmentDetailDto>(`${this.base}/assignments/${id}`);
  }

  updateAssignment(id: string, data: Partial<CreateAssignmentDto>): Observable<AssignmentDto> {
    return this.http.put<AssignmentDto>(`${this.base}/assignments/${id}`, data);
  }

  deleteAssignment(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/assignments/${id}`);
  }

  getSubmissions(assignmentId: string): Observable<SubmissionDto[]> {
    return this.http.get<SubmissionDto[]>(`${this.base}/assignments/${assignmentId}/submissions`);
  }

  getMySubmissions(assignmentId: string): Observable<SubmissionDto[]> {
    return this.http.get<SubmissionDto[]>(`${this.base}/assignments/${assignmentId}/my-submissions`);
  }

  getPendingSubmissions(): Observable<SubmissionDto[]> {
    return this.http.get<SubmissionDto[]>(`${this.base}/assignments/pending`);
  }

  // ── Submission (Student) ─────────────────────────────────────────────────

  submitAssignment(assignmentId: string, content?: string): Observable<SubmissionDto> {
    return this.http.post<SubmissionDto>(`${this.base}/assignments/${assignmentId}/submit`, { content });
  }

  // ── Grading (Teacher) ────────────────────────────────────────────────────

  gradeSubmission(submissionId: string, data: GradeSubmissionDto): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.base}/submissions/${submissionId}/grade`, data);
  }
}
