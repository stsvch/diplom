// my-courses.component.ts
import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { LucideAngularModule, BookOpen, GraduationCap } from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { CourseListDto } from '../models/course.model';
import { CourseCardComponent } from '../../../shared/components/course-card/course-card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { RouterLink } from '@angular/router';

type CourseFilter = 'all' | 'active' | 'completed';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [LucideAngularModule, CourseCardComponent, ButtonComponent, RouterLink],
  templateUrl: './my-courses.component.html',
  styleUrl: './my-courses.component.scss',
})
export class MyCoursesComponent implements OnInit {
  private readonly coursesService = inject(CoursesService);
  private readonly toastService = inject(ToastService);

  readonly BookOpenIcon = BookOpen;
  readonly GraduationCapIcon = GraduationCap;

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  readonly loading = signal(false);
  readonly courses = signal<CourseListDto[]>([]);
  readonly activeFilter = signal<CourseFilter>('all');

  readonly filters: { value: CourseFilter; label: string }[] = [
    { value: 'all', label: 'Все курсы' },
    { value: 'active', label: 'В процессе' },
    { value: 'completed', label: 'Завершённые' },
  ];

  readonly filteredCourses = computed(() => {
    const f = this.activeFilter();
    return this.courses().filter((c) => {
      if (f === 'active') return (c.progress ?? 0) < 100;
      if (f === 'completed') return (c.progress ?? 0) === 100;
      return true;
    });
  });

  readonly skeletonItems = Array(6).fill(0);

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.loading.set(true);
    // Подписка синхронизирует ответ сервиса с локальным состоянием и уведомлениями.
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

  setFilter(f: CourseFilter): void {
    this.activeFilter.set(f);
  }
}
