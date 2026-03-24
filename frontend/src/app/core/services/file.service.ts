import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AttachmentDto } from '../models/attachment.model';

@Injectable({
  providedIn: 'root',
})
export class FileService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}`;

  upload(file: File, entityType: string, entityId: string): Observable<AttachmentDto> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('entityType', entityType);
    formData.append('entityId', entityId);
    return this.http.post<AttachmentDto>(`${this.base}/files/upload`, formData);
  }

  getFileInfo(id: string): Observable<AttachmentDto> {
    return this.http.get<AttachmentDto>(`${this.base}/files/${id}`);
  }

  getDownloadUrl(id: string): string {
    return `/api/files/${id}/download`;
  }

  deleteFile(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/files/${id}`);
  }

  getEntityFiles(entityType: string, entityId: string): Observable<AttachmentDto[]> {
    const params = new HttpParams()
      .set('entityType', entityType)
      .set('entityId', entityId);
    return this.http.get<AttachmentDto[]>(`${this.base}/files`, { params });
  }
}
