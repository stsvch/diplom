import { Component, EventEmitter, Output, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { LessonDetailDto } from '../models/course.model';
import { LessonViewComponent } from '../lesson-view/lesson-view.component';
import { LessonViewStepperComponent } from '../lesson-view-stepper/lesson-view-stepper.component';

@Component({
  selector: 'app-lesson-view-host',
  standalone: true,
  imports: [LessonViewComponent, LessonViewStepperComponent],
  template: `
    @if (lesson(); as l) {
      @if (l.layout === 'Stepper') {
        <app-lesson-view-stepper></app-lesson-view-stepper>
      } @else {
        <app-lesson-view></app-lesson-view>
      }
    } @else {
      <div class="lvh-skel">
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
    `,
  ],
})
export class LessonViewHostComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly http = inject(HttpClient);

  /** Сигнал для родителей (preview-host), что урок готов и можно убрать loader. */
  @Output() ready = new EventEmitter<void>();

  lesson = signal<LessonDetailDto | null>(null);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.http.get<LessonDetailDto>(`${environment.apiUrl}/lessons/${id}`).subscribe({
      next: (l) => {
        this.lesson.set(l);
        this.ready.emit();
      },
      error: () => {
        this.lesson.set({ id: '', title: '', orderIndex: 0, isPublished: false, blocksCount: 0, layout: 'Scroll' });
        this.ready.emit();
      },
    });
  }
}
