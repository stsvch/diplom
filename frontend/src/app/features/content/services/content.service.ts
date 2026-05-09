import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
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
    return this.http.get<LessonBlockDto[]>(`${this.base}/by-lesson/${lessonId}`).pipe(
      map((blocks) => blocks.map((block) => this.normalizeBlock(block))),
    );
  }

  create(payload: CreateLessonBlockPayload): Observable<LessonBlockDto> {
    return this.http.post<LessonBlockDto>(this.base, {
      ...payload,
      data: this.serializeBlockData(payload.data, payload.type),
    }).pipe(
      map((block) => this.normalizeBlock(block)),
    );
  }

  update(id: string, payload: UpdateLessonBlockPayload, fallbackType?: LessonBlockType): Observable<LessonBlockDto> {
    return this.http.put<LessonBlockDto>(`${this.base}/${id}`, {
      ...payload,
      data: this.serializeBlockData(payload.data, fallbackType),
    }).pipe(
      map((block) => this.normalizeBlock(block)),
    );
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

  private serializeBlockData(
    data: LessonBlockData,
    fallbackType?: LessonBlockType,
  ): Record<string, unknown> {
    const source = data as unknown as Record<string, unknown>;
    const { type, ...rest } = source;
    const discriminator = (type as LessonBlockType | undefined) ?? fallbackType;
    if (!discriminator) {
      throw new Error('LessonBlockData is missing a type discriminator');
    }
    // Important: System.Text.Json requires $type to appear first in the object
    // (polymorphic discriminator must precede other properties).
    return {
      $type: discriminator,
      ...rest,
    };
  }

  private normalizeBlock(block: LessonBlockDto): LessonBlockDto {
    const source = block.data as unknown as Record<string, unknown> | null;
    if (!source) {
      return block;
    }

    const normalizedType = typeof source['$type'] === 'string'
      ? source['$type']
      : source['type'] ?? block.type;

    const normalizedData = {
      ...source,
      type: normalizedType,
    } as LessonBlockData & Record<string, unknown>;

    delete normalizedData['$type'];

    return {
      ...block,
      data: normalizedData,
    };
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
