import { Component, Input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ExternalLink,
  Link2,
  LucideAngularModule,
  Paperclip,
  Upload,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { CourseBuilderService } from '../../services/course-builder.service';
import { ToastService } from '../../../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-cb-material-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './material-editor.component.html',
  styleUrl: './material-editor.component.scss',
})
export class MaterialEditorComponent {
  @Input({ required: true }) store!: CourseBuilderStore;
  private readonly api = inject(CourseBuilderService);
  private readonly toast = inject(ToastService);

  readonly icons = { paperclip: Paperclip, link: Link2, upload: Upload, external: ExternalLink };

  readonly item = computed(() => this.store.selectedItem());
  readonly section = computed(() => this.store.selectedSection());
  readonly isMaterial = computed(() => this.item()?.type === 'Resource');

  setTitle(value: string): void {
    this.persist({ title: value });
  }

  setDescription(value: string): void {
    this.persist({ description: value });
  }

  setUrl(value: string): void {
    this.persist({ url: value });
  }

  setRequired(value: boolean): void {
    const it = this.item();
    if (it) this.store.setItemRequired(it, value);
  }

  private persist(patch: { title?: string; description?: string; url?: string }): void {
    const it = this.item();
    const courseId = this.store.courseId();
    if (!it || !it.courseItemId) return;

    this.store.patchItemLocally(it.sourceId, patch);

    this.api
      .updateStandaloneItem(courseId, it.courseItemId, {
        title: patch.title ?? it.title,
        description: patch.description ?? it.description,
        url: patch.url ?? it.url,
        attachmentId: it.attachmentId,
        resourceKind: it.resourceKind,
      })
      .subscribe({
        error: () => this.toast.error('Не удалось сохранить'),
      });
  }
}
