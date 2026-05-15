// rich-text-editor.component.ts
import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Bold,
  Italic,
  List,
  ListOrdered,
  LucideAngularModule,
  Minus,
  Quote,
} from 'lucide-angular';

interface FormatAction {
  key: 'bold' | 'italic' | 'quote' | 'list' | 'olist' | 'rule';
  icon: any;
  title: string;
  prefix: string;
  suffix?: string;
  /** Префикс применяется к началу текущей строки (а не к выделению). */
  linePrefix?: boolean;
  /** Чистая вставка фрагмента — не оборачивает выделение. */
  insert?: boolean;
}

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-rich-text-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './rich-text-editor.component.html',
  styleUrl: './rich-text-editor.component.scss',
})
export class RichTextEditorComponent {
  /** Markdown-текст. */
  @Input() content = '';
  @Input() placeholder =
    'Начните вводить текст… Используйте **жирный**, *курсив*, > цитата, • список';
  @Input() minHeight = '120px';
  @Input() rows = 6;

  @Output() contentChange = new EventEmitter<string>();

  @ViewChild('ta', { static: true }) textareaRef!: ElementRef<HTMLTextAreaElement>;

  readonly actions: FormatAction[] = [
    { key: 'bold',   icon: Bold,        title: 'Жирный (Ctrl+B)',     prefix: '**', suffix: '**' },
    { key: 'italic', icon: Italic,      title: 'Курсив (Ctrl+I)',     prefix: '*',  suffix: '*' },
    { key: 'quote',  icon: Quote,       title: 'Цитата',              prefix: '> ', linePrefix: true },
    { key: 'list',   icon: List,        title: 'Список',              prefix: '• ', linePrefix: true },
    { key: 'olist',  icon: ListOrdered, title: 'Нумерованный список', prefix: '1. ', linePrefix: true },
    { key: 'rule',   icon: Minus,       title: 'Разделитель',         prefix: '\n---\n', insert: true },
  ];

  onInput(value: string): void {
    this.content = value;
    this.contentChange.emit(value);
  }

  onKeydown(event: KeyboardEvent): void {
    if (!(event.ctrlKey || event.metaKey)) return;
    const key = event.key.toLowerCase();
    if (key === 'b') {
      event.preventDefault();
      this.applyAction(this.actions[0]);
    } else if (key === 'i') {
      event.preventDefault();
      this.applyAction(this.actions[1]);
    }
  }

  applyAction(action: FormatAction): void {
    const ta = this.textareaRef?.nativeElement;
    if (!ta) return;

    const start = ta.selectionStart;
    const end = ta.selectionEnd;
    const value = ta.value;

    let next: string;
    let nextStart: number;
    let nextEnd: number;

    if (action.insert) {
      next = value.slice(0, start) + action.prefix + value.slice(end);
      nextStart = nextEnd = start + action.prefix.length;
    } else if (action.linePrefix) {
      const lineStart = value.lastIndexOf('\n', start - 1) + 1;
      next = value.slice(0, lineStart) + action.prefix + value.slice(lineStart);
      nextStart = start + action.prefix.length;
      nextEnd = end + action.prefix.length;
    } else {
      const selected = value.slice(start, end) || 'текст';
      const suffix = action.suffix ?? action.prefix;
      next =
        value.slice(0, start) + action.prefix + selected + suffix + value.slice(end);
      nextStart = start + action.prefix.length;
      nextEnd = nextStart + selected.length;
    }

    this.onInput(next);
    requestAnimationFrame(() => {
      ta.focus();
      ta.setSelectionRange(nextStart, nextEnd);
    });
  }
}
