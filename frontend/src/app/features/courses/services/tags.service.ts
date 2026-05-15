// tags.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { TagDto } from '../models/tag.model';

// Сервис инкапсулирует бизнес-операции, состояние и обращения к API своего модуля.
@Injectable({ providedIn: 'root' })
export class TagsService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tags`;

  search(query?: string, limit = 10): Observable<TagDto[]> {
    let params = new HttpParams().set('limit', String(limit));
    if (query) params = params.set('q', query);
    // HTTP-вызов делегирует обмен с backend API и возвращает Observable вызывающему коду.
    return this.http.get<TagDto[]>(this.base, { params });
  }
}
