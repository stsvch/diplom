import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { QuizBlockData } from '../../models';

@Component({
  selector: 'app-quiz-viewer',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  template: `
    @if (data.testId) {
      <a class="quiz-card" [routerLink]="['/student/test', data.testId, 'play']">
        <lucide-icon name="clipboard-list" size="24"></lucide-icon>
        <div class="info">
          <div class="title">Встроенный тест</div>
          <div class="meta">Нажмите, чтобы пройти</div>
        </div>
        <lucide-icon name="chevron-right" size="20"></lucide-icon>
      </a>
    } @else {
      <div class="empty">Тест не привязан</div>
    }
  `,
  styles: [
    `
      :host { display: block; }
      .quiz-card {
        display: flex; align-items: center; gap: 16px; padding: 16px;
        border: 1px solid #BFDBFE; background: #EFF6FF; color: #1E40AF;
        border-radius: 12px; text-decoration: none; transition: all 0.15s ease;
      }
      .quiz-card:hover { background: #DBEAFE; border-color: #3B82F6; }
      .info { flex: 1; }
      .title { font-weight: 600; font-size: 0.9375rem; }
      .meta { font-size: 0.75rem; color: #3B82F6; margin-top: 2px; }
      .empty { padding: 16px; background: #F1F5F9; border-radius: 8px; color: #64748B; font-size: 0.875rem; }
    `,
  ],
})
export class QuizViewerComponent {
  @Input({ required: true }) data!: QuizBlockData;
}
