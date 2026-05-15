// avatar.component.ts
import { Component, Input, signal, OnChanges, SimpleChanges } from '@angular/core';

export type AvatarSize = 'sm' | 'md' | 'lg' | 'xl';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-avatar',
  standalone: true,
  imports: [],
  templateUrl: './avatar.component.html',
  styleUrl: './avatar.component.scss',
})
export class AvatarComponent implements OnChanges {
  @Input() src = '';
  @Input() alt = '';
  @Input() size: AvatarSize = 'md';
  @Input() fallback = '';

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  imageError = signal(false);

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['src']) {
      this.imageError.set(false);
    }
  }

  get initials(): string {
    if (this.fallback) {
      return this.fallback
        .split(' ')
        .map((w) => w.charAt(0))
        .slice(0, 2)
        .join('')
        .toUpperCase();
    }
    if (this.alt) {
      return this.alt
        .split(' ')
        .map((w) => w.charAt(0))
        .slice(0, 2)
        .join('')
        .toUpperCase();
    }
    return 'U';
  }

  onImageError(): void {
    this.imageError.set(true);
  }
}
