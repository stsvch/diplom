import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { PublishIssue } from '../services/courses.service';

@Component({
  selector: 'app-publish-checklist',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  template: `
    <div class="backdrop" (click)="close.emit()">
      <div class="dialog" (click)="$event.stopPropagation()">
        <header class="dialog__head">
          <h2>Публикация курса</h2>
          <button type="button" class="dialog__close" (click)="close.emit()">
            <lucide-icon name="x" size="20"></lucide-icon>
          </button>
        </header>

        <div class="dialog__body">
          @if (issues.length === 0) {
            <div class="ok">
              <lucide-icon name="check-circle" size="48"></lucide-icon>
              <h3>Всё в порядке</h3>
              <p>Курс готов к публикации.</p>
            </div>
          } @else {
            @if (errors().length > 0) {
              <div class="section">
                <h3 class="section__title section__title--error">
                  <lucide-icon name="circle-x" size="18"></lucide-icon>
                  Ошибки ({{ errors().length }}) — блокируют публикацию
                </h3>
                <ul class="issues">
                  @for (issue of errors(); track issue.code + issue.path) {
                    <li class="issue issue--error">
                      <strong>{{ issue.message }}</strong>
                      <span class="issue__path">{{ issue.path }}</span>
                    </li>
                  }
                </ul>
              </div>
            }

            @if (warnings().length > 0) {
              <div class="section">
                <h3 class="section__title section__title--warning">
                  <lucide-icon name="triangle-alert" size="18"></lucide-icon>
                  Предупреждения ({{ warnings().length }})
                </h3>
                <ul class="issues">
                  @for (issue of warnings(); track issue.code + issue.path) {
                    <li class="issue issue--warning">
                      <strong>{{ issue.message }}</strong>
                      <span class="issue__path">{{ issue.path }}</span>
                    </li>
                  }
                </ul>
              </div>
            }
          }
        </div>

        <footer class="dialog__foot">
          <button type="button" class="btn btn--ghost" (click)="close.emit()">Отмена</button>
          @if (errors().length === 0) {
            <button type="button" class="btn btn--primary" [disabled]="publishing()" (click)="doPublish(false)">
              @if (publishing()) { Публикуем… } @else { Опубликовать }
            </button>
          } @else {
            <button type="button" class="btn btn--danger" [disabled]="publishing()" (click)="doPublish(true)">
              @if (publishing()) { Публикуем… } @else { Опубликовать несмотря на ошибки }
            </button>
          }
        </footer>
      </div>
    </div>
  `,
  styles: [
    `
      :host { display: block; }
      .backdrop {
        position: fixed; inset: 0; background: rgba(15, 23, 42, 0.5);
        display: flex; align-items: center; justify-content: center; z-index: 1000;
      }
      .dialog {
        width: 640px; max-width: 95vw; max-height: 85vh;
        background: #fff; border-radius: 12px; overflow: hidden;
        display: flex; flex-direction: column;
        box-shadow: 0 20px 50px rgba(0, 0, 0, 0.3);
      }
      .dialog__head {
        display: flex; justify-content: space-between; align-items: center;
        padding: 16px 24px; border-bottom: 1px solid #E2E8F0;
      }
      .dialog__head h2 { margin: 0; font-size: 1.125rem; }
      .dialog__close {
        width: 32px; height: 32px; border: none; background: transparent;
        border-radius: 8px; cursor: pointer; color: #64748B;
        display: flex; align-items: center; justify-content: center;
      }
      .dialog__close:hover { background: #F1F5F9; }
      .dialog__body { padding: 20px 24px; overflow-y: auto; flex: 1; }
      .dialog__foot {
        display: flex; justify-content: flex-end; gap: 8px;
        padding: 12px 24px; border-top: 1px solid #E2E8F0;
      }
      .ok {
        display: flex; flex-direction: column; align-items: center; gap: 8px;
        padding: 32px; color: #166534;
      }
      .ok h3 { margin: 0; }
      .ok p { margin: 0; color: #64748B; }
      .section { margin-bottom: 16px; }
      .section__title {
        display: flex; align-items: center; gap: 6px;
        font-size: 0.9375rem; margin: 0 0 8px;
      }
      .section__title--error { color: #991B1B; }
      .section__title--warning { color: #92400E; }
      .issues { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 6px; }
      .issue {
        padding: 10px 12px; border-radius: 8px; display: flex;
        flex-direction: column; gap: 2px;
      }
      .issue--error { background: #FEE2E2; }
      .issue--warning { background: #FEF3C7; }
      .issue strong { font-weight: 600; color: #0F172A; }
      .issue__path { font-size: 0.75rem; color: #64748B; font-family: ui-monospace, monospace; }
      .btn {
        padding: 8px 16px; border: none; border-radius: 8px;
        font-size: 0.875rem; font-weight: 600; cursor: pointer;
      }
      .btn--ghost { background: transparent; color: #64748B; }
      .btn--ghost:hover { background: #F1F5F9; }
      .btn--primary { background: #4F46E5; color: #fff; }
      .btn--primary:hover:not(:disabled) { background: #4338CA; }
      .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
      .btn--danger { background: #EF4444; color: #fff; }
      .btn--danger:hover:not(:disabled) { background: #DC2626; }
      .btn--danger:disabled { opacity: 0.6; cursor: not-allowed; }
    `,
  ],
})
export class PublishChecklistComponent {
  @Input() issues: PublishIssue[] = [];
  @Input() publishing = signal(false);
  @Output() close = new EventEmitter<void>();
  @Output() publish = new EventEmitter<boolean>();

  errors(): PublishIssue[] {
    return this.issues.filter((i) => i.type === 'error');
  }

  warnings(): PublishIssue[] {
    return this.issues.filter((i) => i.type === 'warning');
  }

  doPublish(force: boolean): void {
    this.publish.emit(force);
  }
}
