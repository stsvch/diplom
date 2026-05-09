import { Component, Input, OnDestroy, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import {
  CdkDragDrop,
  DragDropModule,
  moveItemInArray,
} from '@angular/cdk/drag-drop';
import {
  AlertCircle,
  BookOpen,
  CheckCircle2,
  ClipboardList,
  Code,
  Edit3,
  ExternalLink,
  File as FileIcon,
  FileText,
  GripVertical,
  Headphones,
  Image as ImageIcon,
  Link as LinkIcon,
  LucideAngularModule,
  Music,
  Play,
  Plus,
  Settings,
  Trash2,
  Type,
  Upload as UploadIcon,
  Video as VideoIcon,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { ContentService } from '../../../../content/services/content.service';
import { FileService } from '../../../../../core/services/file.service';
import { ToastService } from '../../../../../shared/components/toast/toast.service';
import { RichTextEditorComponent } from '../../../../../shared/components/rich-text-editor/rich-text-editor.component';
import {
  AssignmentBlockData,
  AudioBlockData,
  FileBlockData,
  ImageBlockData,
  LessonBlockDto,
  LessonBlockType,
  QuizBlockData,
  TextBlockData,
  VideoBlockData,
} from '../../../../content/models';
import { TestsService } from '../../../../tests/services/tests.service';
import { AssignmentsService } from '../../../../assignments/services/assignments.service';
import { TestDto } from '../../../../tests/models/test.model';
import { AssignmentDto } from '../../../../assignments/models/assignment.model';

type SimpleBlockType =
  | 'Text'
  | 'Video'
  | 'Audio'
  | 'Image'
  | 'File'
  | 'CodeExercise'
  | 'Quiz'
  | 'Assignment';

interface BlockTypeOption {
  type: SimpleBlockType;
  label: string;
  desc: string;
  icon: any;
  colorClass: string;
}

@Component({
  selector: 'app-cb-lesson-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, DragDropModule, RichTextEditorComponent],
  templateUrl: './lesson-editor.component.html',
  styleUrl: './lesson-editor.component.scss',
})
export class LessonEditorComponent implements OnDestroy {
  @Input({ required: true }) store!: CourseBuilderStore;

  private readonly router = inject(Router);
  private readonly content = inject(ContentService);
  private readonly fileService = inject(FileService);
  private readonly toast = inject(ToastService);
  private readonly testsService = inject(TestsService);
  private readonly assignmentsService = inject(AssignmentsService);
  private readonly sanitizer = inject(DomSanitizer);
  private readonly destroy$ = new Subject<void>();
  private readonly blockSave$ = new Subject<LessonBlockDto>();

  readonly icons = {
    book: BookOpen,
    text: Type,
    video: VideoIcon,
    audio: Headphones,
    image: ImageIcon,
    file: FileIcon,
    code: Code,
    quiz: ClipboardList,
    assignment: FileText,
    plus: Plus,
    trash: Trash2,
    grip: GripVertical,
    music: Music,
    play: Play,
    link: LinkIcon,
    external: ExternalLink,
    edit: Edit3,
    settings: Settings,
    alert: AlertCircle,
    check: CheckCircle2,
    upload: UploadIcon,
  };

  /** ID блоков, для которых сейчас идёт загрузка файла. */
  readonly uploadingBlocks = signal<ReadonlySet<string>>(new Set());

  readonly blockTypes: BlockTypeOption[] = [
    { type: 'Text',         label: 'Текст',       desc: 'Параграф, заголовок, список',  icon: Type,           colorClass: 'le-bt--text' },
    { type: 'Video',        label: 'Видео',       desc: 'YouTube, Vimeo, ссылка',       icon: VideoIcon,      colorClass: 'le-bt--video' },
    { type: 'Image',        label: 'Изображение', desc: 'Картинка, схема, скриншот',    icon: ImageIcon,      colorClass: 'le-bt--image' },
    { type: 'Audio',        label: 'Аудио',       desc: 'Подкаст, лекция, запись',      icon: Headphones,     colorClass: 'le-bt--audio' },
    { type: 'File',         label: 'Файл',        desc: 'PDF, DOCX, ZIP и другие',      icon: FileIcon,       colorClass: 'le-bt--file' },
    { type: 'CodeExercise', label: 'Упражнение',  desc: 'Задание или код',              icon: Code,           colorClass: 'le-bt--code' },
    { type: 'Quiz',         label: 'Тест',        desc: 'Встроенный тест с проверкой',  icon: ClipboardList,  colorClass: 'le-bt--quiz' },
    { type: 'Assignment',   label: 'Задание',     desc: 'Задание со сдачей работы',     icon: FileText,       colorClass: 'le-bt--assignment' },
  ];

  readonly item = computed(() => this.store.selectedItem());
  readonly section = computed(() => this.store.selectedSection());

  readonly blocks = signal<LessonBlockDto[]>([]);
  readonly loading = signal(false);
  readonly inlineMenuFor = signal<number | 'top' | 'bottom' | null>(null);

  /** Список тестов и заданий преподавателя — для блоков Quiz/Assignment. */
  readonly availableTests = signal<TestDto[]>([]);
  readonly availableAssignments = signal<AssignmentDto[]>([]);

  /** Поддержка только указанных типов в новом редакторе. Остальные → полный редактор. */
  readonly supportedTypes: ReadonlyArray<LessonBlockType> = [
    'Text', 'Video', 'Audio', 'Image', 'File', 'CodeExercise', 'Quiz', 'Assignment',
  ];

  constructor() {
    effect(() => {
      const it = this.item();
      if (it && it.type === 'Lesson') {
        this.loadBlocks(it.sourceId);
      } else {
        this.blocks.set([]);
      }
    });

    this.blockSave$
      .pipe(debounceTime(800), takeUntil(this.destroy$))
      .subscribe((block) => this.persistBlock(block));

    this.loadPickerSources();
  }

  private loadPickerSources(): void {
    this.testsService.getMyTests().subscribe({
      next: (tests) => this.availableTests.set(tests),
      error: () => this.availableTests.set([]),
    });
    this.assignmentsService.getMyAssignments().subscribe({
      next: (list) => this.availableAssignments.set(list),
      error: () => this.availableAssignments.set([]),
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadBlocks(lessonId: string): void {
    this.loading.set(true);
    this.content.getByLesson(lessonId).subscribe({
      next: (blocks) => {
        this.blocks.set(blocks.sort((a, b) => a.orderIndex - b.orderIndex));
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  setTitle(value: string): void {
    const it = this.item();
    if (it) this.store.updateItemTitle(it, value);
  }

  setRequired(value: boolean): void {
    const it = this.item();
    if (it) this.store.setItemRequired(it, value);
  }

  openFullEditor(): void {
    const it = this.item();
    if (!it) return;
    this.router.navigate(['/teacher/lesson', it.sourceId, 'edit']);
  }

  // ── Block CRUD ─────────────────────────────────────

  private buildEmptyData(type: SimpleBlockType): any {
    switch (type) {
      case 'Text':         return { type: 'Text', html: '' };
      case 'Video':        return { type: 'Video', url: '' };
      case 'Audio':        return { type: 'Audio', url: '' };
      case 'Image':        return { type: 'Image', url: '' };
      case 'File':         return { type: 'File', attachmentId: null };
      case 'CodeExercise': return {
        type: 'CodeExercise',
        instruction: '',
        starterCode: '',
        language: 'plaintext',
        executable: false,
        testCases: [],
      };
      case 'Quiz':         return { type: 'Quiz', testId: '' };
      case 'Assignment':   return { type: 'Assignment', assignmentId: '' };
    }
  }

  addBlockAt(index: number, type: SimpleBlockType): void {
    const it = this.item();
    if (!it) {
      this.toast.error('Не выбран урок');
      return;
    }
    this.inlineMenuFor.set(null);

    this.content
      .create({
        lessonId: it.sourceId,
        type: type as LessonBlockType,
        data: this.buildEmptyData(type),
      })
      .subscribe({
        next: (created) => {
          const list = [...this.blocks()];
          list.splice(index, 0, created);
          this.blocks.set(list);
          if (index < list.length - 1) {
            this.persistOrder();
          }
        },
        error: (err) => {
          const msg = err?.error?.message ?? err?.message ?? 'Не удалось создать блок';
          this.toast.error(msg);
        },
      });
  }

  addBlockAtEnd(type: SimpleBlockType): void {
    this.addBlockAt(this.blocks().length, type);
  }

  removeBlock(block: LessonBlockDto): void {
    this.content.delete(block.id).subscribe({
      next: () => {
        this.blocks.set(this.blocks().filter((b) => b.id !== block.id));
      },
      error: (err) => {
        const msg = err?.error?.message ?? err?.message ?? 'Не удалось удалить блок';
        this.toast.error(msg);
      },
    });
  }

  /** Локальный update блока + дебаунс автосохранения */
  patchBlock(block: LessonBlockDto, dataUpdate: Partial<any>): void {
    const updated: LessonBlockDto = {
      ...block,
      data: { ...block.data, ...dataUpdate },
    };
    this.blocks.set(this.blocks().map((b) => (b.id === block.id ? updated : b)));
    this.blockSave$.next(updated);
  }

  private persistBlock(block: LessonBlockDto): void {
    this.content
      .update(block.id, { data: block.data, settings: block.settings }, block.type)
      .subscribe({
        next: (saved) => {
          this.blocks.set(this.blocks().map((b) => (b.id === saved.id ? saved : b)));
        },
        error: (err) => {
          const msg = err?.error?.message ?? err?.message ?? 'Не удалось сохранить блок';
          this.toast.error(msg);
        },
      });
  }

  private persistOrder(): void {
    const it = this.item();
    if (!it) return;
    const ids = this.blocks().map((b) => b.id);
    this.content.reorder(it.sourceId, ids).subscribe();
  }

  // ── Drag-and-drop ──────────────────────────────────

  onBlockDrop(event: CdkDragDrop<LessonBlockDto[]>): void {
    if (event.previousIndex === event.currentIndex) return;
    const list = [...this.blocks()];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    this.blocks.set(list);
    this.persistOrder();
  }

  // ── Загрузка файла для блоков Image / Audio / File ──

  isUploading(blockId: string): boolean {
    return this.uploadingBlocks().has(blockId);
  }

  onPickFile(block: LessonBlockDto, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.uploadFileForBlock(block, file);
  }

  onDropFile(block: LessonBlockDto, event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer?.files?.[0];
    if (!file) return;
    this.uploadFileForBlock(block, file);
  }

  onDragOverFile(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  private uploadFileForBlock(block: LessonBlockDto, file: File): void {
    const validation = this.validateFileForBlock(block.type, file);
    if (validation) {
      this.toast.error(validation);
      return;
    }

    const next = new Set(this.uploadingBlocks());
    next.add(block.id);
    this.uploadingBlocks.set(next);

    this.fileService.upload(file, 'LessonBlock', block.id).subscribe({
      next: (att) => {
        this.markUploadDone(block.id);
        if (block.type === 'File') {
          this.patchBlock(block, {
            attachmentId: att.id,
            displayName: att.fileName ?? file.name,
          });
        } else {
          // Image / Audio — храним прямую ссылку
          this.patchBlock(block, { url: att.fileUrl });
        }
      },
      error: (err) => {
        this.markUploadDone(block.id);
        this.toast.error(err?.error?.message ?? 'Не удалось загрузить файл');
      },
    });
  }

  private markUploadDone(blockId: string): void {
    const next = new Set(this.uploadingBlocks());
    next.delete(blockId);
    this.uploadingBlocks.set(next);
  }

  private validateFileForBlock(type: LessonBlockType, file: File): string | null {
    const sizeLimitMb = type === 'Image' ? 5 : type === 'Audio' ? 30 : 50;
    if (file.size > sizeLimitMb * 1024 * 1024) {
      return `Файл больше ${sizeLimitMb} МБ — выберите поменьше.`;
    }
    if (type === 'Image' && !file.type.startsWith('image/')) {
      return 'Нужен файл изображения (PNG, JPG, WebP).';
    }
    if (type === 'Audio' && !file.type.startsWith('audio/')) {
      return 'Нужен аудиофайл (MP3, WAV, OGG).';
    }
    return null;
  }

  toggleInlineMenu(index: number | 'top' | 'bottom'): void {
    this.inlineMenuFor.update((curr) => (curr === index ? null : index));
  }

  isCustomType(type: LessonBlockType): boolean {
    return !this.supportedTypes.includes(type);
  }

  /** YouTube / Vimeo → trusted embed URL для iframe (минуя Angular sanitizer). */
  getEmbedUrl(url: string): SafeResourceUrl | null {
    if (!url) return null;
    let raw: string | null = null;
    const yt = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&?\s]+)/);
    if (yt) raw = `https://www.youtube.com/embed/${yt[1]}`;
    else {
      const vimeo = url.match(/vimeo\.com\/(\d+)/);
      if (vimeo) raw = `https://player.vimeo.com/video/${vimeo[1]}`;
    }
    if (!raw) return null;
    // Без bypassSecurityTrustResourceUrl iframe.src превращается в unsafe:... → чёрный экран.
    return this.sanitizer.bypassSecurityTrustResourceUrl(raw);
  }

  blockTypeLabel(type: LessonBlockType): string {
    const labels: Record<LessonBlockType, string> = {
      Text: 'Текст', Video: 'Видео', Audio: 'Аудио', Image: 'Изображение',
      Banner: 'Баннер', File: 'Файл', SingleChoice: 'Один ответ',
      MultipleChoice: 'Множ. выбор', TrueFalse: 'Верно/Неверно',
      FillGap: 'Пропуски', Dropdown: 'Выпадающий список',
      WordBank: 'Банк слов', Reorder: 'Порядок', Matching: 'Сопоставление',
      OpenText: 'Открытый ответ', CodeExercise: 'Упражнение',
      Quiz: 'Встроенный тест', Assignment: 'Встроенное задание',
    };
    return labels[type] ?? type;
  }

  // ── Type-safe accessors для шаблона ──
  asText(d: any): TextBlockData { return d as TextBlockData; }
  asVideo(d: any): VideoBlockData { return d as VideoBlockData; }
  asAudio(d: any): AudioBlockData { return d as AudioBlockData; }
  asImage(d: any): ImageBlockData { return d as ImageBlockData; }
  asFile(d: any): FileBlockData { return d as FileBlockData; }
  asCode(d: any): { instruction?: string; starterCode?: string } { return d; }
  asQuiz(d: any): QuizBlockData { return d as QuizBlockData; }
  asAssignment(d: any): AssignmentBlockData { return d as AssignmentBlockData; }

  findTest(testId: string | undefined | null): TestDto | undefined {
    if (!testId) return undefined;
    return this.availableTests().find((t) => t.id === testId);
  }

  findAssignment(assignmentId: string | undefined | null): AssignmentDto | undefined {
    if (!assignmentId) return undefined;
    return this.availableAssignments().find((a) => a.id === assignmentId);
  }

  openTestEditor(testId: string): void {
    if (testId) this.router.navigate(['/teacher/test', testId, 'edit']);
  }

  openAssignmentEditor(assignmentId: string): void {
    if (assignmentId) this.router.navigate(['/teacher/assignment', assignmentId, 'edit']);
  }
}
