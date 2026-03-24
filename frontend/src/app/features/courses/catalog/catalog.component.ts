import { Component, inject, signal, computed, OnInit, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Search, SlidersHorizontal, BookOpen, ChevronLeft, ChevronRight } from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { DisciplinesService } from '../services/disciplines.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { CourseListDto, DisciplineDto } from '../models/course.model';
import { SearchInputComponent } from '../../../shared/components/search-input/search-input.component';
import { CourseCardComponent } from '../../../shared/components/course-card/course-card.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';

@Component({
  selector: 'app-catalog',
  standalone: true,
  imports: [
    FormsModule,
    LucideAngularModule,
    SearchInputComponent,
    CourseCardComponent,
    ButtonComponent,
  ],
  templateUrl: './catalog.component.html',
  styleUrl: './catalog.component.scss',
})
export class CatalogComponent implements OnInit {
  private readonly coursesService = inject(CoursesService);
  private readonly disciplinesService = inject(DisciplinesService);
  private readonly toastService = inject(ToastService);

  readonly SearchIcon = Search;
  readonly SlidersIcon = SlidersHorizontal;
  readonly BookOpenIcon = BookOpen;
  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;

  readonly loading = signal(false);
  readonly disciplinesLoading = signal(false);
  readonly courses = signal<CourseListDto[]>([]);
  readonly disciplines = signal<DisciplineDto[]>([]);

  readonly searchValue = signal('');
  readonly selectedDisciplineId = signal<string>('');
  readonly selectedLevel = signal('');
  readonly selectedPrice = signal('');
  readonly selectedSort = signal('newest');

  readonly page = signal(1);
  readonly pageSize = 12;
  readonly totalCount = signal(0);
  readonly totalPages = signal(0);

  readonly levels = [
    { value: '', label: 'Все уровни' },
    { value: 'Beginner', label: 'Начальный' },
    { value: 'Intermediate', label: 'Средний' },
    { value: 'Advanced', label: 'Продвинутый' },
  ];

  readonly priceOptions = [
    { value: '', label: 'Любая цена' },
    { value: 'free', label: 'Бесплатные' },
    { value: 'paid', label: 'Платные' },
  ];

  readonly sortOptions = [
    { value: 'newest', label: 'Новые сначала' },
    { value: 'popular', label: 'По популярности' },
    { value: 'price_asc', label: 'Цена: по возрастанию' },
    { value: 'price_desc', label: 'Цена: по убыванию' },
  ];

  readonly pages = computed(() => {
    const total = this.totalPages();
    const cur = this.page();
    const result: (number | null)[] = [];
    if (total <= 7) {
      for (let i = 1; i <= total; i++) result.push(i);
    } else {
      result.push(1);
      if (cur > 3) result.push(null);
      for (let i = Math.max(2, cur - 1); i <= Math.min(total - 1, cur + 1); i++) result.push(i);
      if (cur < total - 2) result.push(null);
      result.push(total);
    }
    return result;
  });

  private searchDebounce: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      const _ = this.searchValue();
      if (this.searchDebounce) clearTimeout(this.searchDebounce);
      this.searchDebounce = setTimeout(() => {
        this.page.set(1);
        this.loadCourses();
      }, 400);
    });
  }

  ngOnInit(): void {
    this.loadDisciplines();
    this.loadCourses();
  }

  loadDisciplines(): void {
    this.disciplinesLoading.set(true);
    this.disciplinesService.getAll().subscribe({
      next: (data) => {
        this.disciplines.set(data);
        this.disciplinesLoading.set(false);
      },
      error: (err: ApiError) => {
        this.disciplinesLoading.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  loadCourses(): void {
    this.loading.set(true);
    const isFree =
      this.selectedPrice() === 'free' ? true : this.selectedPrice() === 'paid' ? false : undefined;

    this.coursesService
      .getCourses({
        search: this.searchValue() || undefined,
        disciplineId: this.selectedDisciplineId() || undefined,
        level: this.selectedLevel() || undefined,
        isFree,
        sortBy: this.selectedSort() || undefined,
        page: this.page(),
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (result) => {
          this.courses.set(result.items);
          this.totalCount.set(result.totalCount);
          this.totalPages.set(result.totalPages);
          this.loading.set(false);
        },
        error: (err: ApiError) => {
          this.loading.set(false);
          this.toastService.error(err.message);
        },
      });
  }

  selectDiscipline(id: string): void {
    this.selectedDisciplineId.set(id);
    this.page.set(1);
    this.loadCourses();
  }

  onFilterChange(): void {
    this.page.set(1);
    this.loadCourses();
  }

  goToPage(p: number | null): void {
    if (p === null) return;
    this.page.set(p);
    this.loadCourses();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  prevPage(): void {
    if (this.page() > 1) this.goToPage(this.page() - 1);
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) this.goToPage(this.page() + 1);
  }

  readonly skeletonItems = Array(12).fill(0);
}
