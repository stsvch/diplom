import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal, computed, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import {
  LessonBlockType,
  INFORMATIONAL_TYPES,
  AUTO_GRADED_TYPES,
  MANUAL_GRADED_TYPES,
  COMPOSITE_TYPES,
  BLOCK_TYPE_LABELS,
  BLOCK_TYPE_ICONS,
} from '../../models';

interface MenuGroup {
  title: string;
  types: LessonBlockType[];
}

@Component({
  selector: 'app-block-type-menu',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './block-type-menu.component.html',
  styleUrl: './block-type-menu.component.scss',
})
export class BlockTypeMenuComponent {
  @Output() selected = new EventEmitter<LessonBlockType>();
  @Output() closed = new EventEmitter<void>();

  search = signal('');

  groups: MenuGroup[] = [
    { title: 'Контент', types: INFORMATIONAL_TYPES },
    { title: 'Упражнения — автопроверка', types: AUTO_GRADED_TYPES },
    { title: 'Ручная проверка', types: MANUAL_GRADED_TYPES },
    { title: 'Вставить', types: COMPOSITE_TYPES },
  ];

  filteredGroups = computed(() => {
    const q = this.search().trim().toLowerCase();
    if (!q) return this.groups;
    return this.groups
      .map((g) => ({
        title: g.title,
        types: g.types.filter((t) => BLOCK_TYPE_LABELS[t].toLowerCase().includes(q)),
      }))
      .filter((g) => g.types.length > 0);
  });

  label = (t: LessonBlockType) => BLOCK_TYPE_LABELS[t];
  icon = (t: LessonBlockType) => BLOCK_TYPE_ICONS[t];

  choose(type: LessonBlockType) {
    this.selected.emit(type);
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.closed.emit();
  }
}
