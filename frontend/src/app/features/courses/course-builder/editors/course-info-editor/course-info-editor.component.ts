import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Info, LucideAngularModule } from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';

@Component({
  selector: 'app-cb-course-info-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './course-info-editor.component.html',
  styleUrl: './course-info-editor.component.scss',
})
export class CourseInfoEditorComponent {
  @Input({ required: true }) store!: CourseBuilderStore;

  readonly icons = { info: Info };

  setTitle(value: string): void {
    this.store.patchCourseInfo({ title: value });
  }

  setDescription(value: string): void {
    this.store.patchCourseInfo({ description: value });
  }

  setImageUrl(value: string): void {
    this.store.patchCourseInfo({ imageUrl: value });
  }

  setTags(value: string): void {
    this.store.patchCourseInfo({ tags: value });
  }

  setIsFree(value: boolean): void {
    this.store.patchCourseInfo({ isFree: value, price: value ? null : 0 });
  }

  setPrice(value: string): void {
    const n = Number(value);
    if (!Number.isNaN(n)) {
      this.store.patchCourseInfo({ price: n, isFree: false });
    }
  }

  setHasGrading(value: boolean): void {
    this.store.patchCourseInfo({ hasGrading: value });
  }

  setHasCertificate(value: boolean): void {
    this.store.patchCourseInfo({ hasCertificate: value });
  }
}
