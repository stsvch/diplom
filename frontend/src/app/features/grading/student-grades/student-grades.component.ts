import { Component, inject, signal, computed, OnInit } from '@angular/core';
import {
  LucideAngularModule,
  Star,
  ClipboardList,
  CheckCircle2,
  AlertCircle,
  BarChart2,
  BookOpen,
} from 'lucide-angular';
import { GradingService } from '../services/grading.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { GradeDto } from '../models/grading.model';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { StatsCardComponent } from '../../../shared/components/stats-card/stats-card.component';

interface GradeGroup {
  courseId: string;
  courseName: string;
  grades: GradeDto[];
  averageScore: number;
}

@Component({
  selector: 'app-student-grades',
  standalone: true,
  imports: [
    LucideAngularModule,
    BadgeComponent,
    StatsCardComponent,
  ],
  templateUrl: './student-grades.component.html',
  styleUrl: './student-grades.component.scss',
})
export class StudentGradesComponent implements OnInit {
  private readonly gradingService = inject(GradingService);
  private readonly toastService = inject(ToastService);

  readonly StarIcon = Star;
  readonly ClipboardIcon = ClipboardList;
  readonly CheckIcon = CheckCircle2;
  readonly AlertIcon = AlertCircle;
  readonly ChartIcon = BarChart2;
  readonly BookIcon = BookOpen;

  readonly loading = signal(true);
  readonly grades = signal<GradeDto[]>([]);

  readonly groupedGrades = computed((): GradeGroup[] => {
    const all = this.grades();
    const map = new Map<string, GradeDto[]>();
    all.forEach(g => {
      const courseId = g.courseId;
      if (!map.has(courseId)) map.set(courseId, []);
      map.get(courseId)!.push(g);
    });
    return Array.from(map.entries()).map(([courseId, grades]) => ({
      courseId,
      courseName: grades[0]?.courseName?.trim() || `Курс ${courseId.slice(0, 8)}`,
      grades,
      averageScore: grades.length
        ? grades.reduce((sum, g) => sum + (g.maxScore > 0 ? (g.score / g.maxScore * 100) : 0), 0) / grades.length
        : 0,
    }));
  });

  readonly totalGrades = computed(() => this.grades().length);

  readonly overallAverage = computed(() => {
    const all = this.grades();
    if (!all.length) return 0;
    const sum = all.reduce((s, g) => s + (g.maxScore > 0 ? (g.score / g.maxScore * 100) : 0), 0);
    return sum / all.length;
  });

  readonly passingCount = computed(() =>
    this.grades().filter(g => g.maxScore > 0 && (g.score / g.maxScore * 100) >= 60).length
  );

  ngOnInit(): void {
    this.loadGrades();
  }

  loadGrades(): void {
    this.loading.set(true);
    this.gradingService.getMyGrades().subscribe({
      next: (data) => {
        this.grades.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
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

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  }

  getPctLabel(score: number, maxScore: number): string {
    if (maxScore <= 0) return '—';
    return ((score / maxScore) * 100).toFixed(0) + '%';
  }
}
