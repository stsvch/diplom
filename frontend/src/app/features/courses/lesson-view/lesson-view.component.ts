import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  ChevronLeft,
  ChevronRight,
  CheckCircle2,
  Play,
  FileText,
  Download,
  BookOpen,
  Home,
  ClipboardList,
  Clock,
  BarChart2,
  RefreshCw,
  FileEdit,
  Star,
  Send,
} from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { FileService } from '../../../core/services/file.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { LessonBlockDto, LessonDto, CourseDetailDto } from '../models/course.model';
import { AttachmentDto } from '../../../core/models/attachment.model';
import { TestsService } from '../../tests/services/tests.service';
import { TestDetailDto, TestAttemptDto } from '../../tests/models/test.model';
import { AssignmentsService } from '../../assignments/services/assignments.service';
import { AssignmentDto, SubmissionDto } from '../../assignments/models/assignment.model';
import { ProgressService } from '../../progress/services/progress.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { RichTextViewerComponent } from '../../../shared/components/rich-text-viewer/rich-text-viewer.component';
import { VideoPlayerComponent } from '../../../shared/components/video-player/video-player.component';
import { FileCardComponent } from '../../../shared/components/file-card/file-card.component';

interface BlockWithFiles extends LessonBlockDto {
  files?: AttachmentDto[];
  testData?: TestDetailDto | null;
  testAttempts?: TestAttemptDto[];
  assignmentData?: AssignmentDto | null;
  mySubmissions?: SubmissionDto[];
}

@Component({
  selector: 'app-lesson-view',
  standalone: true,
  imports: [
    RouterLink,
    LucideAngularModule,
    ButtonComponent,
    BadgeComponent,
    RichTextViewerComponent,
    VideoPlayerComponent,
    FileCardComponent,
  ],
  templateUrl: './lesson-view.component.html',
  styleUrl: './lesson-view.component.scss',
})
export class LessonViewComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly coursesService = inject(CoursesService);
  private readonly fileService = inject(FileService);
  private readonly toastService = inject(ToastService);
  private readonly testsService = inject(TestsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly progressService = inject(ProgressService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;
  readonly CheckCircleIcon = CheckCircle2;
  readonly PlayIcon = Play;
  readonly FileTextIcon = FileText;
  readonly DownloadIcon = Download;
  readonly BookOpenIcon = BookOpen;
  readonly HomeIcon = Home;
  readonly ClipboardListIcon = ClipboardList;
  readonly ClockIcon = Clock;
  readonly ChartIcon = BarChart2;
  readonly RefreshIcon = RefreshCw;
  readonly FileEditIcon = FileEdit;
  readonly StarIcon = Star;
  readonly SendIcon = Send;

  readonly loading = signal(true);
  readonly blocksLoading = signal(false);
  readonly blocks = signal<BlockWithFiles[]>([]);
  readonly course = signal<CourseDetailDto | null>(null);
  readonly completed = signal(false);

  lessonId = '';
  courseId = '';

  currentLesson = signal<LessonDto | null>(null);
  allLessons = signal<LessonDto[]>([]);

  ngOnInit(): void {
    this.lessonId = this.route.snapshot.paramMap.get('id') ?? '';
    this.courseId = this.route.snapshot.queryParamMap.get('courseId') ?? '';
    if (this.lessonId) {
      this.loadBlocks();
      if (this.courseId) this.loadCourse();
      this.loadLessonProgress();
    }
  }

  loadLessonProgress(): void {
    this.progressService.getLessonProgress(this.lessonId).subscribe({
      next: (progress) => {
        this.completed.set(progress.isCompleted);
      },
      error: () => {
        // silently ignore — not critical
      },
    });
  }

  loadCourse(): void {
    this.coursesService.getCourseById(this.courseId).subscribe({
      next: (data) => {
        this.course.set(data);
        const lessons: LessonDto[] = [];
        for (const mod of data.modules) {
          for (const lesson of mod.lessons) {
            lessons.push(lesson);
          }
        }
        this.allLessons.set(lessons);
        const current = lessons.find((l) => l.id === this.lessonId) ?? null;
        this.currentLesson.set(current);
      },
      error: (err: ApiError) => {
        this.toastService.error(err.message);
      },
    });
  }

  loadBlocks(): void {
    this.blocksLoading.set(true);
    this.coursesService.getLessonBlocks(this.lessonId).subscribe({
      next: (data) => {
        const sorted = data.sort((a, b) => a.orderIndex - b.orderIndex);
        const withFiles: BlockWithFiles[] = sorted.map((b) => ({ ...b, files: [] }));
        this.blocks.set(withFiles);
        this.blocksLoading.set(false);
        this.loading.set(false);

        // Load files for File blocks, test data for Quiz blocks, assignment data for Assignment blocks
        withFiles.forEach((block) => {
          if (block.type === 'File') {
            this.loadBlockFiles(block.id);
          } else if (block.type === 'Quiz' && block.testId) {
            this.loadTestData(block.id, block.testId);
          } else if (block.type === 'Assignment' && block.assignmentId) {
            this.loadAssignmentData(block.id, block.assignmentId);
          }
        });
      },
      error: (err: ApiError) => {
        this.blocksLoading.set(false);
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }

  loadBlockFiles(blockId: string): void {
    this.fileService.getEntityFiles('LessonBlock', blockId).subscribe({
      next: (files) => {
        this.blocks.update((bs) =>
          bs.map((b) => (b.id === blockId ? { ...b, files } : b)),
        );
      },
      error: () => {
        // silently ignore
      },
    });
  }

  loadTestData(blockId: string, testId: string): void {
    this.testsService.getTest(testId).subscribe({
      next: (test) => {
        this.blocks.update((bs) =>
          bs.map((b) => (b.id === blockId ? { ...b, testData: test } : b)),
        );
      },
      error: () => {},
    });

    this.testsService.getMyAttempts(testId).subscribe({
      next: (attempts) => {
        this.blocks.update((bs) =>
          bs.map((b) => (b.id === blockId ? { ...b, testAttempts: attempts } : b)),
        );
      },
      error: () => {},
    });
  }

  loadAssignmentData(blockId: string, assignmentId: string): void {
    this.assignmentsService.getAssignment(assignmentId).subscribe({
      next: (assignment) => {
        this.blocks.update((bs) =>
          bs.map((b) => (b.id === blockId ? { ...b, assignmentData: assignment } : b)),
        );
      },
      error: () => {},
    });

    this.assignmentsService.getMySubmissions(assignmentId).subscribe({
      next: (submissions) => {
        this.blocks.update((bs) =>
          bs.map((b) => (b.id === blockId ? { ...b, mySubmissions: submissions } : b)),
        );
      },
      error: () => {},
    });
  }

  navigateToAssignment(assignmentId: string): void {
    this.router.navigate(['/student/assignment', assignmentId]);
  }

  getLastSubmission(block: BlockWithFiles): SubmissionDto | null {
    const subs = block.mySubmissions;
    if (!subs || !subs.length) return null;
    return subs.sort((a, b) =>
      new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime()
    )[0];
  }

  isDeadlinePassed(deadline?: string): boolean {
    if (!deadline) return false;
    return new Date(deadline) < new Date();
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

  startTest(testId: string): void {
    this.router.navigate(['/student/test', testId, 'play']);
  }

  viewTestResult(testId: string, attemptId: string): void {
    this.router.navigate(['/student/test', testId, 'result', attemptId]);
  }

  getLastAttempt(block: BlockWithFiles): TestAttemptDto | null {
    const attempts = block.testAttempts;
    if (!attempts || !attempts.length) return null;
    return attempts.sort((a, b) =>
      new Date(b.startedAt).getTime() - new Date(a.startedAt).getTime()
    )[0];
  }

  hasInProgressAttempt(block: BlockWithFiles): boolean {
    return (block.testAttempts ?? []).some((a) => a.status === 'InProgress');
  }

  getInProgressAttemptId(block: BlockWithFiles): string | null {
    const a = (block.testAttempts ?? []).find((a) => a.status === 'InProgress');
    return a?.id ?? null;
  }

  get prevLesson(): LessonDto | null {
    const lessons = this.allLessons();
    const idx = lessons.findIndex((l) => l.id === this.lessonId);
    return idx > 0 ? lessons[idx - 1] : null;
  }

  get nextLesson(): LessonDto | null {
    const lessons = this.allLessons();
    const idx = lessons.findIndex((l) => l.id === this.lessonId);
    return idx >= 0 && idx < lessons.length - 1 ? lessons[idx + 1] : null;
  }

  markCompleted(): void {
    if (this.completed()) {
      // Toggle off
      this.progressService.uncompleteLesson(this.lessonId).subscribe({
        next: () => {
          this.completed.set(false);
          this.toastService.success('Урок отмечен как не пройденный');
        },
        error: (err: ApiError) => {
          this.toastService.error(err.message ?? 'Ошибка');
        },
      });
    } else {
      this.progressService.completeLesson(this.lessonId).subscribe({
        next: () => {
          this.completed.set(true);
          this.toastService.success('Урок отмечен как пройденный!');
        },
        error: (err: ApiError) => {
          this.toastService.error(err.message ?? 'Ошибка');
        },
      });
    }
  }

  getLessonUrl(lessonId: string): string {
    return `/student/lesson/${lessonId}${this.courseId ? '?courseId=' + this.courseId : ''}`;
  }
}
