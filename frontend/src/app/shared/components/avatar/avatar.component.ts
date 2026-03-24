import { Component, Input, signal, OnChanges, SimpleChanges } from '@angular/core';

export type AvatarSize = 'sm' | 'md' | 'lg' | 'xl';

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

  imageError = signal(false);

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
