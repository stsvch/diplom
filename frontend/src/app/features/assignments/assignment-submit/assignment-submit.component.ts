import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  ChevronLeft,
  Clock,
  Star,
  RefreshCw,
  Send,
  Loader2,
  CheckCircle2,
  XCircle,
  AlertCircle,
  ClipboardList,
  MessageSquare,
} from 'lucide-angular';
import { AssignmentsService } from '../services/assignments.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FileService } from '../../../core/services/file.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { AssignmentDetailDto, SubmissionDto } from '../models/assignment.model';
import { AttachmentDto } from '../../../core/models/attachment.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { RichTextViewerComponent } from '../../../shared/components/rich-text-viewer/rich-text-viewer.component';
import { FileUploaderComponent } from '../../../shared/components/file-uploader/file-uploader.component';
import { FileCardComponent } from '../../../shared/components/file-card/file-card.component';

@Component({
  selector: 'app-assignment-submit',
  standalone: true,
  imports: [
    FormsModule,
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
    RichTextViewerComponent,
    FileUploaderComponent,
    FileCardComponent,
  ],
  templateUrl: './assignment-submit.component.html',
  styleUrl: './assignment-submit.component.scss',
})
export class AssignmentSubmitComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly toastService = inject(ToastService);
  private readonly fileService = inject(FileService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly ClockIcon = Clock;
  readonly StarIcon = Star;
  readonly RefreshIcon = RefreshCw;
  readonly SendIcon = Send;
  readonly Loader2Icon = Loader2;
  readonly CheckCircleIcon = CheckCircle2;
  readonly XCircleIcon = XCircle;
  readonly AlertCircleIcon = AlertCircle;
  readonly ClipboardListIcon = ClipboardList;
  readonly MessageIcon = MessageSquare;

  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly assignment = signal<AssignmentDetailDto | null>(null);
  readonly mySubmissions = signal<SubmissionDto[]>([]);

  // Current submission that we are building (temp entity to attach files)
  readonly pendingSubmissionId = signal<string | null>(null);
  readonly uploadedFiles = signal<AttachmentDto[]>([]);

  // Form
  content = '';

  assignmentId = '';

  readonly attemptsUsed = computed(() => this.mySubmissions().length);
  readonly attemptsRemaining = computed(() => {
    const a = this.assignment();
    if (!a) return null;
    if (!a.maxAttempts) return null;
    return Math.max(0, a.maxAttempts - this.attemptsUsed());
  });

  readonly isDeadlinePassed = computed(() => {
    const a = this.assignment();
    if (!a?.deadline) return false;
    return new Date(a.deadline) < new Date();
  });

  readonly canSubmit = computed(() => {
    const a = this.assignment();
    if (!a) return false;
    if (this.isDeadlinePassed()) return false;
    const remaining = this.attemptsRemaining();
    if (remaining !== null && remaining <= 0) return false;
    return true;
  });

  ngOnInit(): void {
    this.assignmentId = this.route.snapshot.paramMap.get('id') ?? '';
    if (this.assignmentId) {
      this.loadAssignment();
      this.loadMySubmissions();
    }
  }

  loadAssignment(): void {
    this.loading.set(true);
    this.assignmentsService.getAssignment(this.assignmentId).subscribe({
      next: (data) => {
        this.assignment.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  loadMySubmissions(): void {
    this.assignmentsService.getMySubmissions(this.assignmentId).subscribe({
      next: (data) => {
        this.mySubmissions.set(data.sort((a, b) =>
          new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()
        ));
      },
      error: () => {},
    });
  }

  onFileUploaded(attachment: AttachmentDto): void {
    this.uploadedFiles.update((files) => [...files, attachment]);
  }

  deleteFile(fileId: string): void {
    this.fileService.deleteFile(fileId).subscribe({
      next: () => {
        this.uploadedFiles.update((files) => files.filter((f) => f.id !== fileId));
        this.toastService.success('Файл удалён');
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
      },
    });
  }

  submit(): void {
    if (!this.canSubmit()) return;

    this.submitting.set(true);
    this.assignmentsService
      .submitAssignment(this.assignmentId, this.content || undefined)
      .subscribe({
        next: (submission) => {
          this.submitting.set(false);
          this.toastService.success('Задание отправлено!');
          this.mySubmissions.update((subs) => [submission, ...subs]);
          this.content = '';
          this.uploadedFiles.set([]);
        },
        error: (err) => {
          this.submitting.set(false);
          this.toastService.error(parseApiError(err).message);
        },
      });
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
      case 'Submitted': return 'На проверке';
      case 'Pending': return 'Ожидает';
      case 'Overdue': return 'Просрочено';
      case 'ReturnedForRevision': return 'На доработке';
      default: return status;
    }
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString('ru-RU', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  get backUrl(): string {
    return '/student/courses';
  }
}
