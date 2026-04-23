import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CourseDetailComponent } from '../course-detail/course-detail.component';
import { PreviewModeService } from '../services/preview-mode.service';

@Component({
  selector: 'app-course-preview-host',
  standalone: true,
  imports: [CourseDetailComponent],
  template: `<app-course-detail></app-course-detail>`,
})
export class CoursePreviewHostComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly previewMode = inject(PreviewModeService);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.previewMode.enable(id);
  }

  ngOnDestroy(): void {
    this.previewMode.disable();
  }
}
