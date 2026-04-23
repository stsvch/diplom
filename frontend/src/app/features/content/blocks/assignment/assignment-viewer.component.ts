import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { AssignmentBlockData } from '../../models';

@Component({
  selector: 'app-assignment-viewer',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  template: `
    @if (data.assignmentId) {
      <a class="assign-card" [routerLink]="['/student/assignment', data.assignmentId]">
        <lucide-icon name="briefcase" size="24"></lucide-icon>
        <div class="info">
          <div class="title">Задание для сдачи</div>
          <div class="meta">Нажмите, чтобы открыть</div>
        </div>
        <lucide-icon name="chevron-right" size="20"></lucide-icon>
      </a>
    } @else {
      <div class="empty">Задание не привязано</div>
    }
  `,
  styles: [
    `
      :host { display: block; }
      .assign-card {
        display: flex; align-items: center; gap: 16px; padding: 16px;
        border: 1px solid #BFDBFE; background: #EFF6FF; color: #1E40AF;
        border-radius: 12px; text-decoration: none; transition: all 0.15s ease;
      }
      .assign-card:hover { background: #DBEAFE; border-color: #3B82F6; }
      .info { flex: 1; }
      .title { font-weight: 600; font-size: 0.9375rem; }
      .meta { font-size: 0.75rem; color: #3B82F6; margin-top: 2px; }
      .empty { padding: 16px; background: #F1F5F9; border-radius: 8px; color: #64748B; font-size: 0.875rem; }
    `,
  ],
})
export class AssignmentViewerComponent {
  @Input({ required: true }) data!: AssignmentBlockData;
}
