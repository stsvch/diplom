// lesson-view-host.component.ts
import { ChangeDetectorRef, Component, EventEmitter, OnDestroy, OnInit, Output, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Subject, takeUntil } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { LessonDetailDto } from '../models/course.model';
import { LessonViewComponent } from '../lesson-view/lesson-view.component';
import { LessonViewStepperComponent } from '../lesson-view-stepper/lesson-view-stepper.component';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-lesson-view-host',
  standalone: true,
  imports: [LessonViewComponent, LessonViewStepperComponent],
  template: `
    @if (loading()) {
      <div class="lvh-skel" role="status" aria-live="polite">
        <div class="lvh-skel__status">
          <div class="lvh-skel__spinner" aria-hidden="true"></div>
          <div>
            <p class="lvh-skel__status-title">Открываем урок</p>
            <p class="lvh-skel__status-hint">Проверяем доступ и загружаем структуру</p>
          </div>
        </div>
        <div class="lvh-skel__title"></div>
        <div class="lvh-skel__card">
          <div class="lvh-skel__chip"></div>
          <div class="lvh-skel__line lvh-skel__line--full"></div>
          <div class="lvh-skel__line lvh-skel__line--md"></div>
          <div class="lvh-skel__line lvh-skel__line--sm"></div>
        </div>
        <div class="lvh-skel__card">
          <div class="lvh-skel__chip"></div>
          <div class="lvh-skel__line lvh-skel__line--md"></div>
          <div class="lvh-skel__line lvh-skel__line--full"></div>
        </div>
      </div>
    } @else if (errorMessage()) {
      <div class="lvh-state lvh-state--error">
        <p class="lvh-state__title">Урок не открылся</p>
        <p class="lvh-state__text">{{ errorMessage() }}</p>
        <button type="button" class="lvh-state__action" (click)="retryLoad()">Повторить</button>
      </div>
    } @else if (lesson(); as l) {
      @if (l.layout === 'Stepper') {
        <app-lesson-view-stepper (loaded)="onContentReady()"></app-lesson-view-stepper>
      } @else {
        <app-lesson-view (loaded)="onContentReady()"></app-lesson-view>
      }
    }
  `,
  styles: [
    `
      :host { display: block; }

      .lvh-skel {
        max-width: 840px;
        margin: 0 auto;
        padding: 32px 16px;
        display: flex;
        flex-direction: column;
        gap: 16px;
      }

      .lvh-skel__status {
        display: flex;
        align-items: center;
        gap: 14px;
        padding: 16px 18px;
        background: #fff;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        box-shadow: 0 1px 2px rgba(15, 23, 42, 0.06);
      }

      .lvh-skel__spinner {
        width: 32px;
        height: 32px;
        border: 3px solid #e0e7ff;
        border-top-color: #4f46e5;
        border-radius: 50%;
        flex: 0 0 auto;
        animation: lvh-spin 0.9s linear infinite;
      }

      .lvh-skel__status-title {
        margin: 0;
        color: #0f172a;
        font-weight: 600;
      }

      .lvh-skel__status-hint {
        margin: 4px 0 0;
        color: #64748b;
        font-size: 0.875rem;
      }

      .lvh-skel__title {
        height: 28px;
        width: 60%;
        border-radius: 8px;
        background: #f1f5f9;
        animation: lvh-pulse 1.4s ease-in-out infinite;
      }

      .lvh-skel__card {
        background: #fff;
        border: 1px solid #e2e8f0;
        border-radius: 16px;
        padding: 20px;
        display: flex;
        flex-direction: column;
        gap: 10px;
        border-left: 3px solid #c7d2fe;
      }

      .lvh-skel__chip {
        width: 70px;
        height: 18px;
        border-radius: 999px;
        background: #f1f5f9;
        animation: lvh-pulse 1.4s ease-in-out infinite;
      }

      .lvh-skel__line {
        height: 12px;
        border-radius: 6px;
        background: #f1f5f9;
        animation: lvh-pulse 1.4s ease-in-out infinite;
      }
      .lvh-skel__line--full { width: 100%; }
      .lvh-skel__line--md   { width: 80%; }
      .lvh-skel__line--sm   { width: 50%; }

      @keyframes lvh-pulse {
        0%, 100% { opacity: 1; }
        50% { opacity: 0.5; }
      }

      @keyframes lvh-spin {
        to { transform: rotate(360deg); }
      }

      .lvh-state {
        max-width: 640px;
        margin: 48px auto;
        padding: 32px 20px;
        text-align: center;
        background: #fff;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        box-shadow: 0 1px 2px rgba(15, 23, 42, 0.06);
      }

      .lvh-state--error {
        border-color: #ef4444;
        background: #fee2e2;
      }

      .lvh-state__title {
        margin: 0;
        color: #0f172a;
        font-size: 1.125rem;
        font-weight: 600;
      }

      .lvh-state__text {
        margin: 8px auto 0;
        max-width: 520px;
        color: #475569;
        font-size: 0.875rem;
      }

      .lvh-state__action {
        margin-top: 16px;
        border: 1px solid #4f46e5;
        border-radius: 10px;
        background: #4f46e5;
        color: #fff;
        cursor: pointer;
        font-weight: 600;
        padding: 8px 16px;
        transition: all 150ms ease;
      }

      .lvh-state__action:hover {
        background: #4338ca;
        border-color: #4338ca;
      }
    `,
  ],
})
export class LessonViewHostComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly http = inject(HttpClient);
  private readonly changeDetector = inject(ChangeDetectorRef);
  private readonly destroy$ = new Subject<void>();

  /** Сигнал для родителей (preview-host), что урок готов и можно убрать loader. */
  @Output() ready = new EventEmitter<void>();

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  lesson = signal<LessonDetailDto | null>(null);
  loading = signal(true);
  errorMessage = signal<string | null>(null);

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnInit() {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((params) => {
      const id = params.get('id');
      if (!id) {
        this.errorMessage.set('Не удалось определить урок для предпросмотра.');
        this.loading.set(false);
        this.ready.emit();
        this.changeDetector.detectChanges();
        return;
      }

      this.loadLesson(id);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  retryLoad(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.loadLesson(id);
  }

  onContentReady(): void {
    this.ready.emit();
  }

  private loadLesson(id: string): void {
    this.loading.set(true);
    this.errorMessage.set(null);
    this.lesson.set(null);
    // HTTP-вызов делегирует обмен с backend API и возвращает Observable вызывающему коду.
    // Подписка синхронизирует ответ сервиса с локальным состоянием и уведомлениями.
    this.http.get<LessonDetailDto>(`${environment.apiUrl}/lessons/${id}`).pipe(takeUntil(this.destroy$)).subscribe({
      next: (l) => {
        this.lesson.set(l);
        this.loading.set(false);
        this.changeDetector.detectChanges();
      },
      error: (err) => {
        this.errorMessage.set(err?.message ?? 'Не удалось загрузить данные урока.');
        this.loading.set(false);
        this.ready.emit();
        this.changeDetector.detectChanges();
      },
    });
  }
}
