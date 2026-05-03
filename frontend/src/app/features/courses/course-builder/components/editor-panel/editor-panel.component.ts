import { Component, Input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  BookOpen,
  ClipboardCheck,
  FileEdit,
  Layers,
  LucideAngularModule,
  MousePointerClick,
  Paperclip,
  Plus,
  Video,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { CourseInfoEditorComponent } from '../../editors/course-info-editor/course-info-editor.component';
import { LessonEditorComponent } from '../../editors/lesson-editor/lesson-editor.component';
import { TestEditorComponent } from '../../editors/test-editor/test-editor.component';
import { AssignmentEditorComponent } from '../../editors/assignment-editor/assignment-editor.component';
import { LiveSessionEditorComponent } from '../../editors/live-session-editor/live-session-editor.component';
import { MaterialEditorComponent } from '../../editors/material-editor/material-editor.component';
import { CourseItemType } from '../../models/course-builder.model';

@Component({
  selector: 'app-cb-editor-panel',
  standalone: true,
  imports: [
    CommonModule,
    LucideAngularModule,
    CourseInfoEditorComponent,
    LessonEditorComponent,
    TestEditorComponent,
    AssignmentEditorComponent,
    LiveSessionEditorComponent,
    MaterialEditorComponent,
  ],
  templateUrl: './editor-panel.component.html',
  styleUrl: './editor-panel.component.scss',
})
export class EditorPanelComponent {
  @Input({ required: true }) store!: CourseBuilderStore;

  readonly icons = {
    layers: Layers,
    bookOpen: BookOpen,
    plus: Plus,
    pointer: MousePointerClick,
    test: ClipboardCheck,
    assignment: FileEdit,
    live: Video,
    material: Paperclip,
  };

  readonly quickAdd: { type: CourseItemType; label: string; desc: string; icon: any; color: string }[] = [
    { type: 'Lesson',      label: 'Урок',     desc: 'Текст, видео, файлы',     icon: BookOpen,        color: 'indigo' },
    { type: 'Test',        label: 'Тест',     desc: 'Проверка знаний',         icon: ClipboardCheck,  color: 'orange' },
    { type: 'Assignment',  label: 'Задание',  desc: 'Практическая работа',      icon: FileEdit,        color: 'rose' },
    { type: 'LiveSession', label: 'Live',     desc: 'Онлайн-встреча',           icon: Video,           color: 'teal' },
    { type: 'Resource',    label: 'Материал', desc: 'Файл или ссылка',          icon: Paperclip,       color: 'slate' },
  ];

  readonly hasAnyItem = computed(() => this.store.totalItems() > 0);

  addToFirstSection(type: CourseItemType): void {
    const first = this.store.sections()[0];
    if (first) this.store.addItem(first.id, type);
  }
}
