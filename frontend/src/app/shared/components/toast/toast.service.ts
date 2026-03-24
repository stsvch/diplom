import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
}

let nextId = 0;

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  readonly toasts = signal<Toast[]>([]);

  private add(message: string, type: ToastType): void {
    const id = ++nextId;
    this.toasts.update((list) => [...list, { id, message, type }]);
    setTimeout(() => this.remove(id), 4000);
  }

  success(message: string): void {
    this.add(message, 'success');
  }

  error(message: string): void {
    this.add(message, 'error');
  }

  warning(message: string): void {
    this.add(message, 'warning');
  }

  info(message: string): void {
    this.add(message, 'info');
  }

  remove(id: number): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }
}
