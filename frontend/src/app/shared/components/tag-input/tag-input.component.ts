// tag-input.component.ts
import {
  Component,
  ElementRef,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  inject,
  signal,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, X, Tag } from 'lucide-angular';
import { Subject, debounceTime, switchMap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TagsService } from '../../../features/courses/services/tags.service';
import { TagDto } from '../../../features/courses/models/tag.model';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-tag-input',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './tag-input.component.html',
  styleUrl: './tag-input.component.scss',
})
export class TagInputComponent {
  @Input() tags: string[] = [];
  @Input() placeholder = 'Добавьте тег и нажмите Enter';
  @Input() maxTags = 10;
  @Output() tagsChange = new EventEmitter<string[]>();

  @ViewChild('inputEl') inputEl?: ElementRef<HTMLInputElement>;

  private readonly tagsApi = inject(TagsService);

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  readonly inputValue = signal('');
  readonly suggestions = signal<TagDto[]>([]);
  readonly suggestionsOpen = signal(false);
  readonly highlightedIndex = signal(-1);

  private readonly query$ = new Subject<string>();

  readonly icons = { close: X, tag: Tag };

  constructor() {
    this.query$
      .pipe(
        debounceTime(180),
        switchMap((q) => this.tagsApi.search(q, 8)),
        takeUntilDestroyed(),
      )
      // Подписка синхронизирует ответ сервиса с локальным состоянием и уведомлениями.
      .subscribe({
        next: (list) => {
          // Скрываем уже добавленные теги из подсказок (по slug-сравнению)
          const existing = new Set(this.tags.map((t) => this.makeSlug(t)));
          this.suggestions.set(list.filter((t) => !existing.has(t.slug)));
          this.suggestionsOpen.set(this.suggestions().length > 0);
          this.highlightedIndex.set(-1);
        },
        error: () => {
          this.suggestions.set([]);
          this.suggestionsOpen.set(false);
        },
      });

    // При изменении inputValue или внешних tags — рефрешим подсказки
    effect(() => {
      const v = this.inputValue();
      if (v.trim().length === 0) {
        this.suggestions.set([]);
        this.suggestionsOpen.set(false);
        return;
      }
      this.query$.next(v.trim());
    });
  }

  onInput(value: string): void {
    this.inputValue.set(value);
  }

  onKeyDown(event: KeyboardEvent): void {
    const open = this.suggestionsOpen();
    const list = this.suggestions();

    if (event.key === 'Enter' || event.key === ',') {
      event.preventDefault();
      const idx = this.highlightedIndex();
      if (open && idx >= 0 && idx < list.length) {
        this.addTag(list[idx].name);
      } else {
        this.addTag(this.inputValue());
      }
      return;
    }

    if (event.key === 'Backspace' && this.inputValue() === '' && this.tags.length > 0) {
      event.preventDefault();
      this.removeTag(this.tags.length - 1);
      return;
    }

    if (event.key === 'Tab' && open && list.length > 0) {
      event.preventDefault();
      const idx = this.highlightedIndex() >= 0 ? this.highlightedIndex() : 0;
      this.addTag(list[idx].name);
      return;
    }

    if (event.key === 'Escape') {
      this.suggestionsOpen.set(false);
      return;
    }

    if (event.key === 'ArrowDown' && open) {
      event.preventDefault();
      this.highlightedIndex.update((i) => Math.min(i + 1, list.length - 1));
      return;
    }

    if (event.key === 'ArrowUp' && open) {
      event.preventDefault();
      this.highlightedIndex.update((i) => Math.max(i - 1, 0));
      return;
    }
  }

  selectSuggestion(name: string): void {
    this.addTag(name);
  }

  removeTag(index: number): void {
    const next = this.tags.filter((_, i) => i !== index);
    this.tagsChange.emit(next);
  }

  onBlur(): void {
    // Закрываем dropdown с задержкой, чтобы клик по подсказке успел сработать
    setTimeout(() => this.suggestionsOpen.set(false), 120);
  }

  private addTag(raw: string): void {
    const value = raw.trim();
    if (!value) return;
    if (this.tags.length >= this.maxTags) return;

    const slug = this.makeSlug(value);
    if (!slug) return;
    // Проверка дубля по slug
    if (this.tags.some((t) => this.makeSlug(t) === slug)) {
      this.inputValue.set('');
      return;
    }

    const next = [...this.tags, value];
    this.tagsChange.emit(next);
    this.inputValue.set('');
    this.suggestions.set([]);
    this.suggestionsOpen.set(false);
    this.highlightedIndex.set(-1);
  }

  private makeSlug(input: string): string {
    return input
      .trim()
      .toLowerCase()
      .replace(/[^\p{L}\p{N}]+/gu, '-')
      .replace(/^-+|-+$/g, '');
  }
}
