// editor-panel.component.ts
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
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { CourseInfoEditorComponent } from '../../editors/course-info-editor/course-info-editor.component';
import { LessonEditorComponent } from '../../editors/lesson-editor/lesson-editor.component';
import { TestEditorComponent } from '../../editors/test-editor/test-editor.component';
import { AssignmentEditorComponent } from '../../editors/assignment-editor/assignment-editor.component';
import { MaterialEditorComponent } from '../../editors/material-editor/material-editor.component';
import { CourseItemType } from '../../models/course-builder.model';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
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
    material: Paperclip,
  };

  readonly quickAdd: { type: CourseItemType; label: string; desc: string; icon: any; color: string }[] = [
    { type: 'Lesson',      label: 'Урок',     desc: 'Текст, видео, файлы',     icon: BookOpen,        color: 'indigo' },
    { type: 'Test',        label: 'Тест',     desc: 'Проверка знаний',         icon: ClipboardCheck,  color: 'orange' },
    { type: 'Assignment',  label: 'Задание',  desc: 'Практическая работа',      icon: FileEdit,        color: 'rose' },
    { type: 'Resource',    label: 'Материал', desc: 'Файл или ссылка',          icon: Paperclip,       color: 'slate' },
  ];

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  readonly hasAnyItem = computed(() => this.store.totalItems() > 0);

  addToFirstSection(type: CourseItemType): void {
    const first = this.store.sections()[0];
    if (first) this.store.addItem(first.id, type);
  }
}
