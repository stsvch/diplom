import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  DictionaryReviewOutcome,
  DictionaryWordDto,
  GlossaryFilters,
  UpsertDictionaryWordRequest,
} from '../models/glossary.model';

@Injectable({
  providedIn: 'root',
})
export class GlossaryService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/glossary`;

  getWords(filters: GlossaryFilters = {}): Observable<DictionaryWordDto[]> {
    let params = new HttpParams();

    if (filters.courseId) {
      params = params.set('courseId', filters.courseId);
    }

    if (filters.search) {
      params = params.set('search', filters.search);
    }

    if (filters.knownOnly !== undefined) {
      params = params.set('knownOnly', String(filters.knownOnly));
    }

    return this.http.get<DictionaryWordDto[]>(`${this.base}/words`, { params });
  }

  createWord(payload: UpsertDictionaryWordRequest): Observable<DictionaryWordDto> {
    return this.http.post<DictionaryWordDto>(`${this.base}/words`, payload);
  }

  updateWord(id: string, payload: UpsertDictionaryWordRequest): Observable<DictionaryWordDto> {
    return this.http.put<DictionaryWordDto>(`${this.base}/words/${id}`, payload);
  }

  deleteWord(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/words/${id}`);
  }

  setProgress(id: string, isKnown: boolean): Observable<DictionaryWordDto> {
    return this.http.post<DictionaryWordDto>(`${this.base}/words/${id}/progress`, { isKnown });
  }

  getReviewSession(courseId?: string, take = 12, excludeWordIds: string[] = []): Observable<DictionaryWordDto[]> {
    return this.http.post<DictionaryWordDto[]>(`${this.base}/review-session`, {
      courseId: courseId || null,
      take,
      excludeWordIds,
    });
  }

  reviewWord(id: string, outcome: DictionaryReviewOutcome): Observable<DictionaryWordDto> {
    return this.http.post<DictionaryWordDto>(`${this.base}/words/${id}/review`, { outcome });
  }
}
