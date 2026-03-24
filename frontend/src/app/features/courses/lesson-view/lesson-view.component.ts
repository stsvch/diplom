import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
} from 'lucide-angular';
import { CoursesService } from '../services/courses.service';
import { FileService } from '../../../core/services/file.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { LessonBlockDto, LessonDto, CourseDetailDto } from '../models/course.model';
import { AttachmentDto } from '../../../core/models/attachment.model';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { RichTextViewerComponent } from '../../../shared/components/rich-text-viewer/rich-text-viewer.component';
import { VideoPlayerComponent } from '../../../shared/components/video-player/video-player.component';
import { FileCardComponent } from '../../../shared/components/file-card/file-card.component';

interface BlockWithFiles extends LessonBlockDto {
  files?: AttachmentDto[];
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
  private readonly coursesService = inject(CoursesService);
  private readonly fileService = inject(FileService);
  private readonly toastService = inject(ToastService);

  readonly ChevronLeftIcon = ChevronLeft;
  readonly ChevronRightIcon = ChevronRight;
  readonly CheckCircleIcon = CheckCircle2;
  readonly PlayIcon = Play;
  readonly FileTextIcon = FileText;
  readonly DownloadIcon = Download;
  readonly BookOpenIcon = BookOpen;
  readonly HomeIcon = Home;

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
    }
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

        // Load files for File blocks
        withFiles.forEach((block) => {
          if (block.type === 'File') {
            this.loadBlockFiles(block.id);
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
    // TODO: Этап 7 (Progress) — сохранить прогресс на backend: POST /api/progress/lessons/:id/complete
    this.completed.set(true);
    this.toastService.success('Урок отмечен как пройденный!');
  }

  getLessonUrl(lessonId: string): string {
    return `/student/lesson/${lessonId}${this.courseId ? '?courseId=' + this.courseId : ''}`;
  }
}
