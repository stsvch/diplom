import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  Plus,
  Edit2,
  Trash2,
  BookOpen,
  Users,
  Eye,
  Archive,
  MoreVertical,
} from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { CourseListDto } from '../models/course.model';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-teacher-courses',
  standalone: true,
  imports: [RouterLink, LucideAngularModule, BadgeComponent, ButtonComponent],
  templateUrl: './teacher-courses.component.html',
  styleUrl: './teacher-courses.component.scss',
})
export class TeacherCoursesComponent implements OnInit {
  private readonly coursesService = inject(CoursesService);
  private readonly toastService = inject(ToastService);

  readonly PlusIcon = Plus;
  readonly EditIcon = Edit2;
  readonly TrashIcon = Trash2;
  readonly BookOpenIcon = BookOpen;
  readonly UsersIcon = Users;
  readonly EyeIcon = Eye;
  readonly ArchiveIcon = Archive;
  readonly MoreVerticalIcon = MoreVertical;

  readonly loading = signal(true);
  readonly publishing = signal<string | null>(null);
  readonly archiving = signal<string | null>(null);
  readonly deleting = signal<string | null>(null);
  readonly courses = signal<CourseListDto[]>([]);
  readonly openMenuId = signal<string | null>(null);

  readonly skeletonItems = Array(5).fill(0);

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.loading.set(true);
    this.coursesService.getMyCourses().subscribe({
      next: (data) => {
        this.courses.set(data);
        this.loading.set(false);
      },
      error: (err: ApiError) => {
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  publish(id: string): void {
    this.publishing.set(id);
    this.coursesService.publishCourse(id).subscribe({
      next: () => {
        this.publishing.set(null);
        this.toastService.success('Курс опубликован!');
        this.loadCourses();
      },
      error: (err: ApiError) => {
        this.publishing.set(null);
        this.toastService.error(err.message);
      },
    });
  }

  archive(id: string): void {
    this.archiving.set(id);
    this.coursesService.archiveCourse(id).subscribe({
      next: () => {
        this.archiving.set(null);
        this.toastService.success('Курс архивирован!');
        this.loadCourses();
      },
      error: (err: ApiError) => {
        this.archiving.set(null);
        this.toastService.error(err.message);
      },
    });
  }

  deleteCourse(id: string): void {
    if (!confirm('Вы уверены, что хотите удалить этот курс?')) return;
    this.deleting.set(id);
    this.coursesService.deleteCourse(id).subscribe({
      next: () => {
        this.deleting.set(null);
        this.toastService.success('Курс удалён!');
        this.courses.update((list) => list.filter((c) => c.id !== id));
      },
      error: (err: ApiError) => {
        this.deleting.set(null);
        this.toastService.error(err.message);
      },
    });
  }

  toggleMenu(id: string): void {
    this.openMenuId.set(this.openMenuId() === id ? null : id);
  }

  closeMenu(): void {
    this.openMenuId.set(null);
  }

  getLevelLabel(level: string): string {
    const map: Record<string, string> = {
      Beginner: 'Начальный',
      Intermediate: 'Средний',
      Advanced: 'Продвинутый',
    };
    return map[level] ?? level;
  }
}
