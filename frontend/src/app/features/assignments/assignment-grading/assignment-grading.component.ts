import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  User,
  Clock,
  CheckCircle2,
  XCircle,
  AlertCircle,
  Loader2,
  ClipboardList,
  Star,
  MessageSquare,
} from 'lucide-angular';
import { AssignmentsService } from '../services/assignments.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { SubmissionDto } from '../models/assignment.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { RichTextViewerComponent } from '../../../shared/components/rich-text-viewer/rich-text-viewer.component';

type FilterTab = 'all' | 'pending' | 'overdue';

interface EnrichedSubmission extends SubmissionDto {
  assignmentTitle?: string;
}

@Component({
  selector: 'app-assignment-grading',
  standalone: true,
  imports: [
    FormsModule,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
    RichTextViewerComponent,
  ],
  templateUrl: './assignment-grading.component.html',
  styleUrl: './assignment-grading.component.scss',
})
export class AssignmentGradingComponent implements OnInit {
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly toastService = inject(ToastService);

  readonly UserIcon = User;
  readonly ClockIcon = Clock;
  readonly CheckCircleIcon = CheckCircle2;
  readonly XCircleIcon = XCircle;
  readonly AlertCircleIcon = AlertCircle;
  readonly Loader2Icon = Loader2;
  readonly ClipboardListIcon = ClipboardList;
  readonly StarIcon = Star;
  readonly MessageIcon = MessageSquare;

  readonly loading = signal(true);
  readonly grading = signal(false);
  readonly submissions = signal<EnrichedSubmission[]>([]);
  readonly selectedSubmission = signal<EnrichedSubmission | null>(null);
  readonly activeTab = signal<FilterTab>('all');

  // Grading form
  score = 0;
  comment = '';

  readonly quickScores = [50, 70, 85, 100];

  readonly filteredSubmissions = computed(() => {
    const tab = this.activeTab();
    const subs = this.submissions();
    if (tab === 'pending') return subs.filter((s) => s.status === 'Submitted' || s.status === 'Pending');
    if (tab === 'overdue') return subs.filter((s) => s.status === 'Overdue');
    return subs;
  });

  ngOnInit(): void {
    this.loadPending();
  }

  loadPending(): void {
    this.loading.set(true);
    this.assignmentsService.getPendingSubmissions().subscribe({
      next: (data) => {
        this.submissions.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  selectSubmission(sub: EnrichedSubmission): void {
    this.selectedSubmission.set(sub);
    this.score = sub.score ?? 0;
    this.comment = sub.teacherComment ?? '';
  }

  setQuickScore(pct: number): void {
    const sub = this.selectedSubmission();
    if (!sub) return;
    this.score = Math.round((sub.maxScore * pct) / 100);
  }

  accept(): void {
    const sub = this.selectedSubmission();
    if (!sub) return;
    if (this.score < 0 || this.score > sub.maxScore) {
      this.toastService.error(`Балл должен быть от 0 до ${sub.maxScore}`);
      return;
    }
    this.grading.set(true);
    this.assignmentsService
      .gradeSubmission(sub.id, {
        score: this.score,
        comment: this.comment || undefined,
        returnForRevision: false,
      })
      .subscribe({
        next: () => {
          this.grading.set(false);
          this.toastService.success('Оценка выставлена');
          this.removeFromList(sub.id);
        },
        error: (err) => {
          this.grading.set(false);
          this.toastService.error(parseApiError(err).message);
        },
      });
  }

  returnForRevision(): void {
    const sub = this.selectedSubmission();
    if (!sub) return;
    this.grading.set(true);
    this.assignmentsService
      .gradeSubmission(sub.id, {
        score: 0,
        comment: this.comment || undefined,
        returnForRevision: true,
      })
      .subscribe({
        next: () => {
          this.grading.set(false);
          this.toastService.success('Задание возвращено на доработку');
          this.removeFromList(sub.id);
        },
        error: (err) => {
          this.grading.set(false);
          this.toastService.error(parseApiError(err).message);
        },
      });
  }

  private removeFromList(id: string): void {
    this.submissions.update((subs) => subs.filter((s) => s.id !== id));
    this.selectedSubmission.set(null);
    this.score = 0;
    this.comment = '';
  }

  setTab(tab: FilterTab): void {
    this.activeTab.set(tab);
  }

  getStatusVariant(status: string): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    switch (status) {
      case 'Graded': return 'success';
      case 'Submitted': return 'primary';
      case 'Pending': return 'warning';
      case 'Overdue': return 'danger';
      case 'ReturnedForRevision': return 'warning';
      default: return 'neutral';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Graded': return 'Оценено';
      case 'Submitted': return 'Ожидает';
      case 'Pending': return 'Ожидает';
      case 'Overdue': return 'Просрочено';
      case 'ReturnedForRevision': return 'На доработке';
      default: return status;
    }
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
