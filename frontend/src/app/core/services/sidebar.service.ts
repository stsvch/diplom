import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SidebarService {
  /** Свёрнут ли sidebar на десктопе (legacy — оставлено для обратной совместимости) */
  readonly collapsed = signal<boolean>(false);

  /** Открыт ли sidebar на мобиле (overlay) */
  readonly mobileOpen = signal<boolean>(false);

  toggle(): void {
    this.collapsed.update((v) => !v);
  }

  openMobile(): void {
    this.mobileOpen.set(true);
  }

  closeMobile(): void {
    this.mobileOpen.set(false);
  }

  toggleMobile(): void {
    this.mobileOpen.update((v) => !v);
  }
}
