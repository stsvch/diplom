import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { LessonViewHostComponent } from '../lesson-view-host/lesson-view-host.component';
import { PreviewModeService } from '../services/preview-mode.service';

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
    <app-lesson-view-host></app-lesson-view-host>
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
    `,
  ],
})
export class LessonPreviewHostComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly previewMode = inject(PreviewModeService);

  courseId = signal<string | null>(null);

  ngOnInit(): void {
    const cid = this.route.snapshot.queryParamMap.get('courseId');
    this.courseId.set(cid);
    this.previewMode.enable(cid);
  }

  ngOnDestroy(): void {
    this.previewMode.disable();
  }
}
