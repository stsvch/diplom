import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Download,
  FileSpreadsheet,
  FileText,
  Search,
  Users,
  BarChart2,
  CheckCircle2,
  ClipboardList,
  ChevronDown,
} from 'lucide-angular';
import { GradingService } from '../services/grading.service';
import { CoursesService } from '../../courses/services/courses.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { GradebookDto, GradebookStatsDto, GradeDto, StudentGradesDto } from '../models/grading.model';
import { CourseListDto } from '../../courses/models/course.model';
import { StatsCardComponent } from '../../../shared/components/stats-card/stats-card.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { SearchInputComponent } from '../../../shared/components/search-input/search-input.component';

@Component({
  selector: 'app-gradebook',
  standalone: true,
  imports: [
    FormsModule,
    LucideAngularModule,
    StatsCardComponent,
    BadgeComponent,
    ButtonComponent,
    SearchInputComponent,
  ],
  templateUrl: './gradebook.component.html',
  styleUrl: './gradebook.component.scss',
})
export class GradebookComponent implements OnInit {
  private readonly gradingService = inject(GradingService);
  private readonly coursesService = inject(CoursesService);
  private readonly toastService = inject(ToastService);

  readonly DownloadIcon = Download;
  readonly FileSpreadsheetIcon = FileSpreadsheet;
  readonly FilePdfIcon = FileText;
  readonly SearchIcon = Search;
  readonly UsersIcon = Users;
  readonly ChartIcon = BarChart2;
  readonly CheckIcon = CheckCircle2;
  readonly ClipboardIcon = ClipboardList;
  readonly ChevronDownIcon = ChevronDown;

  readonly loading = signal(false);
  readonly statsLoading = signal(false);
  readonly courses = signal<CourseListDto[]>([]);
  readonly selectedCourseId = signal<string | null>(null);
  readonly gradebook = signal<GradebookDto | null>(null);
  readonly stats = signal<GradebookStatsDto | null>(null);
  readonly searchQuery = signal('');
  readonly exportMenuOpen = signal(false);

  readonly allTitles = computed(() => {
    const gb = this.gradebook();
    if (!gb) return [];
    const titles = gb.students
      .flatMap(s => s.grades.map(g => g.title))
      .filter((v, i, a) => a.indexOf(v) === i)
      .sort();
    return titles;
  });

  readonly filteredStudents = computed(() => {
    const gb = this.gradebook();
    if (!gb) return [];
    const q = this.searchQuery().toLowerCase();
    if (!q) return gb.students;
    return gb.students.filter(s =>
      s.studentName.toLowerCase().includes(q) ||
      s.studentId.toLowerCase().includes(q)
    );
  });

  readonly columnAverages = computed(() => {
    const titles = this.allTitles();
    const students = this.filteredStudents();
    return titles.map(title => {
      const scores = students
        .map(s => s.grades.find(g => g.title === title))
        .filter(g => g != null) as GradeDto[];
      if (!scores.length) return 0;
      const avg = scores.reduce((sum, g) => sum + (g.maxScore > 0 ? (g.score / g.maxScore * 100) : 0), 0) / scores.length;
      return Math.round(avg * 10) / 10;
    });
  });

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.coursesService.getCourses({ pageSize: 100 }).subscribe({
      next: (data) => {
        this.courses.set(data.items);
        if (data.items.length > 0) {
          this.selectCourse(data.items[0].id);
        }
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  selectCourse(courseId: string): void {
    this.selectedCourseId.set(courseId);
    this.loadGradebook(courseId);
    this.loadStats(courseId);
  }

  loadGradebook(courseId: string): void {
    this.loading.set(true);
    this.gradingService.getCourseGradebook(courseId).subscribe({
      next: (data) => {
        this.gradebook.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  loadStats(courseId: string): void {
    this.statsLoading.set(true);
    this.gradingService.getGradebookStats(courseId).subscribe({
      next: (data) => {
        this.stats.set(data);
        this.statsLoading.set(false);
      },
      error: () => {
        this.statsLoading.set(false);
      },
    });
  }

  exportExcel(): void {
    const id = this.selectedCourseId();
    if (!id) return;
    this.exportMenuOpen.set(false);
    if (!this.hasExportableData()) {
      this.toastService.error('В журнале нет данных для экспорта');
      return;
    }
    this.gradingService.exportExcel(id);
  }

  exportPdf(): void {
    const id = this.selectedCourseId();
    if (!id) return;
    this.exportMenuOpen.set(false);
    if (!this.hasExportableData()) {
      this.toastService.error('В журнале нет данных для экспорта');
      return;
    }
    this.gradingService.exportPdf(id);
  }

  toggleExportMenu(): void {
    this.exportMenuOpen.update(v => !v);
  }

  getGrade(student: StudentGradesDto, title: string): GradeDto | null {
    return student.grades.find(g => g.title === title) ?? null;
  }

  editGrade(grade: GradeDto): void {
    const rawScore = prompt('Новый балл', String(grade.score));
    if (rawScore === null) return;

    const rawMaxScore = prompt('Максимальный балл', String(grade.maxScore));
    if (rawMaxScore === null) return;

    const score = Number(rawScore.replace(',', '.'));
    const maxScore = Number(rawMaxScore.replace(',', '.'));

    if (!Number.isFinite(score) || !Number.isFinite(maxScore) || maxScore <= 0) {
      this.toastService.error('Введите корректные числовые значения');
      return;
    }

    if (score < 0 || score > maxScore) {
      this.toastService.error('Балл должен быть в диапазоне от 0 до максимального');
      return;
    }

    const comment = prompt('Комментарий (необязательно)', grade.comment ?? '');
    if (comment === null) return;

    this.gradingService.updateGrade(grade.id, {
      score,
      maxScore,
      comment: comment.trim() || undefined,
    }).subscribe({
      next: () => {
        this.toastService.success('Оценка обновлена');
        const courseId = this.selectedCourseId();
        if (!courseId) return;
        this.loadGradebook(courseId);
        this.loadStats(courseId);
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  getScoreClass(score: number, maxScore: number): string {
    if (maxScore <= 0) return '';
    const pct = (score / maxScore) * 100;
    if (pct >= 90) return 'score--green';
    if (pct >= 75) return 'score--yellow';
    if (pct >= 60) return 'score--orange';
    return 'score--red';
  }

  getSourceTypeLabel(type: string): string {
    return type === 'Test' ? 'Тест' : 'Задание';
  }

  getSourceTypeVariant(type: string): 'primary' | 'warning' {
    return type === 'Test' ? 'primary' : 'warning';
  }

  getUniqueTitlesWithTypes(): { title: string; type: string }[] {
    const gb = this.gradebook();
    if (!gb) return [];
    const map = new Map<string, string>();
    gb.students.forEach(s => {
      s.grades.forEach(g => {
        if (!map.has(g.title)) map.set(g.title, g.sourceType);
      });
    });
    return Array.from(map.entries())
      .map(([title, type]) => ({ title, type }))
      .sort((a, b) => a.title.localeCompare(b.title));
  }

  get selectedCourseName(): string {
    const id = this.selectedCourseId();
    const course = this.courses().find(c => c.id === id);
    return course?.title ?? '';
  }

  private hasExportableData(): boolean {
    const gb = this.gradebook();
    if (!gb) return false;
    return gb.students.some(student => student.grades.length > 0);
  }
}
