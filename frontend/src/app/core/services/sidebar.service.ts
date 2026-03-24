import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SidebarService {
  readonly collapsed = signal<boolean>(false);

  toggle(): void {
    this.collapsed.update((v) => !v);
  }
}
