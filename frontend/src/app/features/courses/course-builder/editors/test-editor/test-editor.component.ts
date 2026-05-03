import { Component, Input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  AlertCircle,
  ClipboardCheck,
  Edit3,
  ExternalLink,
  LucideAngularModule,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';

@Component({
  selector: 'app-cb-test-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './test-editor.component.html',
  styleUrl: './test-editor.component.scss',
})
export class TestEditorComponent {
  @Input({ required: true }) store!: CourseBuilderStore;
  private readonly router = inject(Router);

  readonly icons = { test: ClipboardCheck, edit: Edit3, link: ExternalLink, alert: AlertCircle };

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
    this.router.navigate(['/teacher/test', it.sourceId, 'edit']);
  }
}
