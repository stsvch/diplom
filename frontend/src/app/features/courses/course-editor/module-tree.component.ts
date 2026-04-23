import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CdkDrag, CdkDropList, CdkDropListGroup, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { LucideAngularModule } from 'lucide-angular';
import { CourseModuleDetailDto, LessonDetailDto } from '../models/course.model';

export type ModuleTreeEvent =
  | { kind: 'rename-module'; moduleId: string; title: string }
  | { kind: 'delete-module'; moduleId: string }
  | { kind: 'add-lesson'; moduleId: string }
  | { kind: 'rename-lesson'; lessonId: string; title: string }
  | { kind: 'delete-lesson'; moduleId: string; lessonId: string }
  | { kind: 'duplicate-lesson'; moduleId: string; lessonId: string }
  | { kind: 'reorder-modules'; orderedIds: string[] }
  | { kind: 'reorder-lessons'; moduleId: string; orderedIds: string[] }
  | { kind: 'move-lesson'; lessonId: string; fromModuleId: string; toModuleId: string; newOrder: number };

@Component({
  selector: 'app-module-tree',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, CdkDropListGroup, CdkDropList, CdkDrag, LucideAngularModule],
  templateUrl: './module-tree.component.html',
  styleUrl: './module-tree.component.scss',
})
export class ModuleTreeComponent {
  @Input() modules: CourseModuleDetailDto[] = [];
  @Output() event = new EventEmitter<ModuleTreeEvent>();

  expandedModules = signal<Set<string>>(new Set());
  editingModuleId = signal<string | null>(null);
  editingLessonId = signal<string | null>(null);

  toggleModule(id: string): void {
    const next = new Set(this.expandedModules());
    if (next.has(id)) next.delete(id); else next.add(id);
    this.expandedModules.set(next);
  }

  isExpanded(id: string): boolean {
    return this.expandedModules().has(id);
  }

  startEditModule(id: string): void {
    this.editingModuleId.set(id);
  }

  finishEditModule(id: string, title: string, original: string): void {
    this.editingModuleId.set(null);
    const trimmed = title.trim();
    if (trimmed && trimmed !== original) {
      this.event.emit({ kind: 'rename-module', moduleId: id, title: trimmed });
    }
  }

  startEditLesson(id: string): void {
    this.editingLessonId.set(id);
  }

  finishEditLesson(id: string, title: string, original: string): void {
    this.editingLessonId.set(null);
    const trimmed = title.trim();
    if (trimmed && trimmed !== original) {
      this.event.emit({ kind: 'rename-lesson', lessonId: id, title: trimmed });
    }
  }

  addLesson(moduleId: string): void {
    this.expandedModules.update((s) => new Set(s).add(moduleId));
    this.event.emit({ kind: 'add-lesson', moduleId });
  }

  deleteModule(id: string): void {
    if (confirm('Удалить модуль вместе со всеми уроками?')) {
      this.event.emit({ kind: 'delete-module', moduleId: id });
    }
  }

  deleteLesson(moduleId: string, lessonId: string): void {
    if (confirm('Удалить урок? Все блоки будут потеряны.')) {
      this.event.emit({ kind: 'delete-lesson', moduleId, lessonId });
    }
  }

  duplicateLesson(moduleId: string, lessonId: string): void {
    this.event.emit({ kind: 'duplicate-lesson', moduleId, lessonId });
  }

  onModuleDrop(event: CdkDragDrop<CourseModuleDetailDto[]>): void {
    if (event.previousIndex === event.currentIndex) return;
    const arr = [...this.modules];
    moveItemInArray(arr, event.previousIndex, event.currentIndex);
    this.event.emit({ kind: 'reorder-modules', orderedIds: arr.map((m) => m.id) });
  }

  onLessonDrop(event: CdkDragDrop<LessonDetailDto[]>, targetModuleId: string): void {
    const fromContainer = event.previousContainer.data;
    const toContainer = event.container.data;

    if (event.previousContainer === event.container) {
      if (event.previousIndex === event.currentIndex) return;
      const orderedIds = [...toContainer];
      moveItemInArray(orderedIds, event.previousIndex, event.currentIndex);
      this.event.emit({
        kind: 'reorder-lessons',
        moduleId: targetModuleId,
        orderedIds: orderedIds.map((l) => l.id),
      });
    } else {
      const lesson = fromContainer[event.previousIndex];
      const fromEl = event.previousContainer.element.nativeElement;
      const fromModuleId = fromEl.getAttribute('data-module-id');
      if (!fromModuleId) return;
      transferArrayItem(fromContainer, toContainer, event.previousIndex, event.currentIndex);
      this.event.emit({
        kind: 'move-lesson',
        lessonId: lesson.id,
        fromModuleId,
        toModuleId: targetModuleId,
        newOrder: event.currentIndex,
      });
    }
  }

  trackById(_: number, item: { id: string }) {
    return item.id;
  }
}
