import { Component, inject, signal, OnInit } from '@angular/core';
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
      <div class="state">Загрузка урока…</div>
    }
  `,
  styles: [`.state { padding: 48px; text-align: center; color: #64748B; }`],
})
export class LessonViewHostComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly http = inject(HttpClient);

  lesson = signal<LessonDetailDto | null>(null);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.http.get<LessonDetailDto>(`${environment.apiUrl}/lessons/${id}`).subscribe({
      next: (l) => this.lesson.set(l),
      error: () => this.lesson.set({ id: '', title: '', orderIndex: 0, isPublished: false, blocksCount: 0, layout: 'Scroll' }),
    });
  }
}
