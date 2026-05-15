// lesson-preview-host.component.ts
import { ChangeDetectorRef, Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { LessonViewHostComponent } from '../lesson-view-host/lesson-view-host.component';
import { PreviewModeService } from '../services/preview-mode.service';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-lesson-preview-host',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule, LessonViewHostComponent],
  template: `
    <div class="preview-banner">
      <lucide-icon name="book-open" size="16"></lucide-icon>
      Предпросмотр урока
      @if (courseId()) {
        <a class="preview-banner__back" [routerLink]="['/teacher/courses', courseId(), 'preview']">
          ← К странице курса
        </a>
      }
    </div>

    <div class="preview-stage">
      <app-lesson-view-host (ready)="onReady()"></app-lesson-view-host>

      @if (!ready()) {
        <div class="preview-loader" role="status" aria-live="polite">
          <div class="preview-loader__spinner"></div>
          <p class="preview-loader__text">Загружаем урок…</p>
          <p class="preview-loader__hint">Подгружаем содержимое и блоки</p>
        </div>
      }
    </div>
  `,
  styles: [
    `
      :host { display: block; }

      .preview-banner {
        position: sticky; top: 0; z-index: 10;
        display: flex; align-items: center; gap: 8px;
        padding: 10px 24px;
        background: #FEF3C7; color: #92400E;
        font-size: 0.875rem;
        border-bottom: 1px solid #FBBF24;
      }
      .preview-banner__back {
        margin-left: auto;
        color: #92400E; text-decoration: none; font-weight: 600;
      }
      .preview-banner__back:hover { text-decoration: underline; }

      .preview-stage {
        position: relative;
        min-height: 60vh;
      }

      .preview-loader {
        position: absolute;
        inset: 0;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 12px;
        background: rgba(248, 250, 252, 0.96);
        backdrop-filter: blur(2px);
        z-index: 5;
        animation: preview-fade-in 200ms ease-out;
      }

      .preview-loader__spinner {
        width: 48px;
        height: 48px;
        border: 3px solid #e0e7ff;
        border-top-color: #6366f1;
        border-radius: 50%;
        animation: preview-spin 0.9s linear infinite;
      }

      .preview-loader__text {
        margin: 0;
        font-size: 1rem;
        font-weight: 600;
        color: #334155;
      }

      .preview-loader__hint {
        margin: 0;
        font-size: 0.8125rem;
        color: #64748b;
      }

      @keyframes preview-spin {
        to { transform: rotate(360deg); }
      }

      @keyframes preview-fade-in {
        from { opacity: 0; }
        to   { opacity: 1; }
      }
    `,
  ],
})
export class LessonPreviewHostComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly previewMode = inject(PreviewModeService);
  private readonly changeDetector = inject(ChangeDetectorRef);

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  courseId = signal<string | null>(null);
  ready = signal<boolean>(false);

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnInit(): void {
    const cid = this.route.snapshot.queryParamMap.get('courseId');
    this.courseId.set(cid);
    this.ready.set(false);
    this.previewMode.enable(cid);
  }

  ngOnDestroy(): void {
    this.previewMode.disable();
  }

  onReady(): void {
    this.ready.set(true);
    this.changeDetector.detectChanges();
  }
}
