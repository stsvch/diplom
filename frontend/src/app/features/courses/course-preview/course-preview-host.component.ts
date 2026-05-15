// course-preview-host.component.ts
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CourseDetailComponent } from '../course-detail/course-detail.component';
import { PreviewModeService } from '../services/preview-mode.service';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-course-preview-host',
  standalone: true,
  imports: [CourseDetailComponent],
  template: `<app-course-detail></app-course-detail>`,
})
export class CoursePreviewHostComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly previewMode = inject(PreviewModeService);

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.previewMode.enable(id);
  }

  ngOnDestroy(): void {
    this.previewMode.disable();
  }
}
