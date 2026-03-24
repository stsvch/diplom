import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Plus,
  Edit2,
  Trash2,
  BookOpen,
  X,
  Check,
  AlertTriangle,
} from 'lucide-angular';
import { DisciplinesService } from '../../courses/services/disciplines.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { DisciplineDto } from '../../courses/models/course.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';

type ModalMode = 'create' | 'edit';

@Component({
  selector: 'app-disciplines',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
  ],
  templateUrl: './disciplines.component.html',
  styleUrl: './disciplines.component.scss',
})
export class DisciplinesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly disciplinesService = inject(DisciplinesService);
  private readonly toastService = inject(ToastService);

  readonly PlusIcon = Plus;
  readonly EditIcon = Edit2;
  readonly TrashIcon = Trash2;
  readonly BookOpenIcon = BookOpen;
  readonly XIcon = X;
  readonly CheckIcon = Check;
  readonly AlertIcon = AlertTriangle;

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly deleting = signal<string | null>(null);
  readonly disciplines = signal<DisciplineDto[]>([]);
  readonly showModal = signal(false);
  readonly modalMode = signal<ModalMode>('create');
  readonly editingId = signal<string | null>(null);
  readonly showDeleteConfirm = signal<string | null>(null);

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    description: [''],
  });

  readonly skeletonItems = Array(6).fill(0);

  ngOnInit(): void {
    this.loadDisciplines();
  }

  loadDisciplines(): void {
    this.loading.set(true);
    this.disciplinesService.getAll().subscribe({
      next: (data) => {
        this.disciplines.set(data);
        this.loading.set(false);
      },
      error: (err: ApiError) => {
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  openCreateModal(): void {
    this.form.reset({ name: '', description: '' });
    this.editingId.set(null);
    this.modalMode.set('create');
    this.showModal.set(true);
  }

  openEditModal(discipline: DisciplineDto): void {
    this.form.patchValue({
      name: discipline.name,
      description: discipline.description ?? '',
    });
    this.editingId.set(discipline.id);
    this.modalMode.set('edit');
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.form.reset();
  }

  saveModal(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { name, description } = this.form.value as { name: string; description: string };
    this.saving.set(true);

    if (this.modalMode() === 'create') {
      this.disciplinesService.create({ name, description }).subscribe({
        next: (d) => {
          this.disciplines.update((list) => [...list, d]);
          this.saving.set(false);
          this.closeModal();
          this.toastService.success('Дисциплина создана!');
        },
        error: (err: ApiError) => {
          this.saving.set(false);
          this.toastService.error(err.message);
        },
      });
    } else {
      const id = this.editingId()!;
      this.disciplinesService.update(id, { name, description }).subscribe({
        next: (d) => {
          this.disciplines.update((list) => list.map((item) => (item.id === id ? d : item)));
          this.saving.set(false);
          this.closeModal();
          this.toastService.success('Дисциплина обновлена!');
        },
        error: (err: ApiError) => {
          this.saving.set(false);
          this.toastService.error(err.message);
        },
      });
    }
  }

  confirmDelete(id: string): void {
    this.showDeleteConfirm.set(id);
  }

  cancelDelete(): void {
    this.showDeleteConfirm.set(null);
  }

  deleteDiscipline(id: string): void {
    this.deleting.set(id);
    this.disciplinesService.delete(id).subscribe({
      next: () => {
        this.disciplines.update((list) => list.filter((d) => d.id !== id));
        this.deleting.set(null);
        this.showDeleteConfirm.set(null);
        this.toastService.success('Дисциплина удалена!');
      },
      error: (err: ApiError) => {
        this.deleting.set(null);
        this.toastService.error(err.message);
      },
    });
  }

  getNameError(): string {
    const ctrl = this.form.get('name');
    if (!ctrl?.touched || !ctrl.invalid) return '';
    if (ctrl.hasError('required')) return 'Название обязательно';
    if (ctrl.hasError('minlength')) return 'Минимум 2 символа';
    return '';
  }
}
