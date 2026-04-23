import { Injectable, computed, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class PreviewModeService {
  private readonly activeCount = signal(0);
  private readonly _courseId = signal<string | null>(null);

  readonly isPreview = computed(() => this.activeCount() > 0);
  readonly courseId = this._courseId.asReadonly();

  enable(courseId: string | null): void {
    this.activeCount.update((n) => n + 1);
    if (courseId) this._courseId.set(courseId);
  }

  disable(): void {
    this.activeCount.update((n) => Math.max(0, n - 1));
    if (this.activeCount() === 0) this._courseId.set(null);
  }
}
