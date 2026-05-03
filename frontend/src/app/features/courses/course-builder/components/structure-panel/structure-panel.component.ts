import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import {
  AlertCircle,
  BookOpen,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  ClipboardCheck,
  Edit2,
  FileEdit,
  GripVertical,
  Layers,
  Link2,
  LucideAngularModule,
  MoreHorizontal,
  Paperclip,
  Plus,
  Trash2,
  Video,
  X,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import {
  CourseBuilderItemDto,
  CourseBuilderSectionDto,
  CourseItemType,
  COURSE_ITEM_TYPE_DESCRIPTIONS,
  COURSE_ITEM_TYPE_LABELS,
} from '../../models/course-builder.model';

interface ItemTypeOption {
  type: CourseItemType;
  label: string;
  desc: string;
  icon: any;
  color: string;
}

@Component({
  selector: 'app-cb-structure-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, DragDropModule],
  templateUrl: './structure-panel.component.html',
  styleUrl: './structure-panel.component.scss',
})
export class StructurePanelComponent {
  @Input({ required: true }) store!: CourseBuilderStore;
  @Output() closeMobile = new EventEmitter<void>();

  readonly icons = {
    bookOpen: BookOpen,
    test: ClipboardCheck,
    assignment: FileEdit,
    live: Video,
    material: Paperclip,
    link: Link2,
    layers: Layers,
    plus: Plus,
    chevronDown: ChevronDown,
    chevronRight: ChevronRight,
    trash: Trash2,
    edit: Edit2,
    more: MoreHorizontal,
    alert: AlertCircle,
    check: CheckCircle2,
    grip: GripVertical,
    x: X,
  };

  /** Список ID секций для DnD connect-lists (items могут перемещаться между секциями) */
  readonly sectionDropListIds = computed(() =>
    this.store.sections().map((s) => 'sp-section-' + s.id),
  );

  readonly addMenuFor = signal<string | null | 'unsectioned'>(null);
  readonly editingSectionId = signal<string | null>(null);
  readonly editingItemId = signal<string | null>(null);
  readonly draftTitle = signal<string>('');
  readonly collapsedSections = signal<Set<string>>(new Set());

  readonly itemTypes: ItemTypeOption[] = [
    { type: 'Lesson',       label: 'Урок',          desc: 'Текст, видео, изображения, файлы',   icon: BookOpen,        color: 'indigo' },
    { type: 'Test',         label: 'Тест',          desc: 'Вопросы с баллами и проходным порогом', icon: ClipboardCheck, color: 'orange' },
    { type: 'Assignment',   label: 'Задание',       desc: 'Практическая работа',                  icon: FileEdit,        color: 'rose' },
    { type: 'LiveSession',  label: 'Live-занятие',  desc: 'Онлайн-встреча по расписанию',         icon: Video,           color: 'teal' },
    { type: 'Resource',     label: 'Материал',      desc: 'Файл, PDF, презентация',               icon: Paperclip,       color: 'slate' },
    { type: 'ExternalLink', label: 'Внешняя ссылка',desc: 'Ресурс вне платформы',                 icon: Link2,           color: 'violet' },
  ];

  iconForType(type: CourseItemType) {
    switch (type) {
      case 'Lesson': return this.icons.bookOpen;
      case 'Test': return this.icons.test;
      case 'Assignment': return this.icons.assignment;
      case 'LiveSession': return this.icons.live;
      case 'Resource': return this.icons.material;
      case 'ExternalLink': return this.icons.link;
    }
  }

  colorClassForType(type: CourseItemType): string {
    switch (type) {
      case 'Lesson': return 'sp-item--indigo';
      case 'Test': return 'sp-item--orange';
      case 'Assignment': return 'sp-item--rose';
      case 'LiveSession': return 'sp-item--teal';
      case 'Resource': return 'sp-item--slate';
      case 'ExternalLink': return 'sp-item--violet';
    }
  }

  toggleSectionCollapse(sectionId: string): void {
    this.collapsedSections.update((s) => {
      const next = new Set(s);
      if (next.has(sectionId)) next.delete(sectionId);
      else next.add(sectionId);
      return next;
    });
  }

  isCollapsed(sectionId: string): boolean {
    return this.collapsedSections().has(sectionId);
  }

  startEditSection(section: CourseBuilderSectionDto): void {
    this.editingSectionId.set(section.id);
    this.draftTitle.set(section.title);
  }

  saveSection(section: CourseBuilderSectionDto): void {
    const title = this.draftTitle().trim();
    if (title && title !== section.title) {
      this.store.updateSection(section.id, title);
    }
    this.editingSectionId.set(null);
  }

  startEditItem(item: CourseBuilderItemDto): void {
    this.editingItemId.set(item.sourceId);
    this.draftTitle.set(item.title);
  }

  saveItem(item: CourseBuilderItemDto): void {
    const title = this.draftTitle().trim();
    if (title && title !== item.title) {
      this.store.updateItemTitle(item, title);
    }
    this.editingItemId.set(null);
  }

  toggleAddMenu(sectionId: string | null | 'unsectioned'): void {
    this.addMenuFor.update((curr) => (curr === sectionId ? null : sectionId));
  }

  pickType(sectionId: string | null, type: CourseItemType): void {
    this.store.addItem(sectionId, type);
    this.addMenuFor.set(null);
  }

  selectCourseInfo(): void {
    this.store.setSelection({ kind: 'course_info' });
    this.closeMobile.emit();
  }

  selectItem(item: CourseBuilderItemDto): void {
    this.store.setSelection({
      kind: 'item',
      sectionId: item.sectionId ?? null,
      itemId: item.sourceId,
    });
    this.closeMobile.emit();
  }

  isSelected(item: CourseBuilderItemDto): boolean {
    const sel = this.store.selection();
    return sel.kind === 'item' && sel.itemId === item.sourceId;
  }

  isCourseInfoSelected(): boolean {
    return this.store.selection().kind === 'course_info';
  }

  removeItem(item: CourseBuilderItemDto, ev: Event): void {
    ev.stopPropagation();
    this.store.removeItem(item);
  }

  removeSection(sectionId: string, ev: Event): void {
    ev.stopPropagation();
    if (confirm('Удалить раздел вместе со всеми элементами?')) {
      this.store.deleteSection(sectionId);
    }
  }

  statusDotClass(status: string): string {
    if (status === 'Ready' || status === 'Published') return 'sp-status-dot--ready';
    if (status === 'NeedsContent') return 'sp-status-dot--warn';
    return 'sp-status-dot--draft';
  }

  /** Drag-and-drop для секций */
  onSectionDrop(event: CdkDragDrop<CourseBuilderSectionDto[]>): void {
    if (event.previousIndex === event.currentIndex) return;
    const sections = [...this.store.sections()];
    moveItemInArray(sections, event.previousIndex, event.currentIndex);
    this.store.reorderSections(sections.map((s) => s.id));
  }

  /** Drag-and-drop для элементов в секции (с поддержкой перемещения между секциями) */
  onItemDrop(event: CdkDragDrop<CourseBuilderItemDto[]>, targetSectionId: string): void {
    const item = event.item.data as CourseBuilderItemDto;
    if (!item?.courseItemId) return;

    if (event.previousContainer === event.container) {
      // Перемещение внутри секции
      if (event.previousIndex === event.currentIndex) return;
      const section = this.store.sections().find((s) => s.id === targetSectionId);
      if (!section) return;
      const ids = section.items.map((i) => i.courseItemId).filter((id): id is string => !!id);
      const [moved] = ids.splice(event.previousIndex, 1);
      ids.splice(event.currentIndex, 0, moved);
      this.store.reorderItems(targetSectionId, ids);
    } else {
      // Перемещение между секциями
      this.store.moveItem(item.sourceId, targetSectionId, event.currentIndex);
    }
  }
}
