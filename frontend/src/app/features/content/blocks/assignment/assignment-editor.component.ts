import { Component, EventEmitter, Input, OnInit, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
import { AssignmentsService } from '../../../assignments/services/assignments.service';
import { AssignmentDto } from '../../../assignments/models/assignment.model';
import { AssignmentBlockData } from '../../models';

@Component({
  selector: 'app-assignment-editor',
  standalone: true,
  imports: [FormsModule, RouterLink, LucideAngularModule],
  template: `
    <div class="field">
      <span class="label">Задание</span>
      @if (loading()) {
        <div class="loading">Загрузка списка заданий…</div>
      } @else if (items().length === 0) {
        <div class="empty">У вас пока нет заданий</div>
      } @else {
        <select class="input"
          [ngModel]="data.assignmentId"
          (ngModelChange)="update({ assignmentId: $event })"
        >
          <option value="">— выберите задание —</option>
          @for (a of items(); track a.id) {
            <option [value]="a.id">{{ a.title }}</option>
          }
        </select>
      }
    </div>
    <a class="link" [routerLink]="['/teacher/assignment/new']" target="_blank">
      <lucide-icon name="external-link" size="14"></lucide-icon>
      Создать новое задание
    </a>
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 8px; }
      .field { display: flex; flex-direction: column; gap: 4px; }
      .label { font-size: 0.75rem; color: #64748B; }
      .input {
        padding: 8px 12px; border: 1px solid #E2E8F0; border-radius: 8px;
        background: #F8FAFC; font: inherit; font-size: 0.875rem; outline: none;
      }
      .input:focus { border-color: #4F46E5; background: #fff; }
      .loading, .empty { padding: 10px; background: #F1F5F9; border-radius: 6px; font-size: 0.8125rem; color: #64748B; }
      .link {
        align-self: flex-start; display: inline-flex; align-items: center; gap: 4px;
        font-size: 0.8125rem; color: #4F46E5; text-decoration: none;
      }
      .link:hover { text-decoration: underline; }
    `,
  ],
})
export class AssignmentEditorComponent implements OnInit {
  @Input({ required: true }) data!: AssignmentBlockData;
  @Output() dataChange = new EventEmitter<AssignmentBlockData>();

  private readonly service = inject(AssignmentsService);

  items = signal<AssignmentDto[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.service.getMyAssignments().subscribe({
      next: (list) => {
        this.items.set(list);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  update(patch: Partial<AssignmentBlockData>) {
    this.dataChange.emit({ ...this.data, ...patch });
  }
}
