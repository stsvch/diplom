import { Component, Input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  AlertCircle,
  Edit3,
  ExternalLink,
  FileEdit,
  LucideAngularModule,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';

@Component({
  selector: 'app-cb-assignment-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './assignment-editor.component.html',
  styleUrl: './assignment-editor.component.scss',
})
export class AssignmentEditorComponent {
  @Input({ required: true }) store!: CourseBuilderStore;
  private readonly router = inject(Router);

  readonly icons = { assignment: FileEdit, edit: Edit3, link: ExternalLink, alert: AlertCircle };

  readonly item = computed(() => this.store.selectedItem());
  readonly section = computed(() => this.store.selectedSection());

  setTitle(value: string): void {
    const it = this.item();
    if (it) this.store.updateItemTitle(it, value);
  }

  setRequired(value: boolean): void {
    const it = this.item();
    if (it) this.store.setItemRequired(it, value);
  }

  openFullEditor(): void {
    const it = this.item();
    if (!it) return;
    this.router.navigate(['/teacher/assignment', it.sourceId, 'edit']);
  }
}
