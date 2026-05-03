import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../../environments/environment';
import {
  CourseBuilderDto,
  CourseItemBackfillDto,
  CourseItemDto,
  CreateStandaloneCourseItemRequest,
  MoveCourseItemRequest,
  ReorderCourseItemsRequest,
  UpdateCourseItemMetadataRequest,
  UpdateStandaloneCourseItemRequest,
} from '../models/course-builder.model';

@Injectable({ providedIn: 'root' })
export class CourseBuilderService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  /** GET /api/courses/{id}/builder */
  getBuilder(courseId: string): Observable<CourseBuilderDto> {
    return this.http.get<CourseBuilderDto>(`${this.base}/courses/${courseId}/builder`);
  }

  /** POST /api/courses/{id}/builder/backfill — создаёт CourseItems для существующих source-сущностей */
  backfill(courseId: string): Observable<CourseItemBackfillDto> {
    return this.http.post<CourseItemBackfillDto>(
      `${this.base}/courses/${courseId}/builder/backfill`,
      {},
    );
  }

  /** POST /api/courses/{id}/builder/items — создать standalone Resource/ExternalLink */
  createStandaloneItem(
    courseId: string,
    request: CreateStandaloneCourseItemRequest,
  ): Observable<CourseItemDto> {
    return this.http.post<CourseItemDto>(
      `${this.base}/courses/${courseId}/builder/items`,
      request,
    );
  }

  /** PUT /api/courses/{id}/builder/items/{itemId} — title/description/url для standalone */
  updateStandaloneItem(
    courseId: string,
    itemId: string,
    request: UpdateStandaloneCourseItemRequest,
  ): Observable<CourseItemDto> {
    return this.http.put<CourseItemDto>(
      `${this.base}/courses/${courseId}/builder/items/${itemId}`,
      request,
    );
  }

  /** PUT /api/courses/{id}/builder/items/{itemId}/metadata — isRequired/points/dates/status для любого */
  updateItemMetadata(
    courseId: string,
    itemId: string,
    request: UpdateCourseItemMetadataRequest,
  ): Observable<CourseItemDto> {
    return this.http.put<CourseItemDto>(
      `${this.base}/courses/${courseId}/builder/items/${itemId}/metadata`,
      request,
    );
  }

  /** DELETE /api/courses/{id}/builder/items/{itemId} — только для Resource/ExternalLink */
  deleteStandaloneItem(courseId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/courses/${courseId}/builder/items/${itemId}`);
  }

  /** POST /api/courses/{id}/builder/items/{itemId}/move — переместить item между разделами */
  moveItem(
    courseId: string,
    itemId: string,
    request: MoveCourseItemRequest,
  ): Observable<CourseItemDto> {
    return this.http.post<CourseItemDto>(
      `${this.base}/courses/${courseId}/builder/items/${itemId}/move`,
      request,
    );
  }

  /** POST /api/courses/{id}/builder/items/reorder — пересортировать items в разделе */
  reorderItems(
    courseId: string,
    request: ReorderCourseItemsRequest,
  ): Observable<CourseItemDto[]> {
    return this.http.post<CourseItemDto[]>(
      `${this.base}/courses/${courseId}/builder/items/reorder`,
      request,
    );
  }
}
