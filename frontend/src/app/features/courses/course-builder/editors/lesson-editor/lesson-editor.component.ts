import { Component, Input, OnDestroy, computed, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import {
  AlertCircle,
  BookOpen,
  CheckCircle2,
  Code,
  Edit3,
  ExternalLink,
  File as FileIcon,
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
  Video as VideoIcon,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';
import { ContentService } from '../../../../content/services/content.service';
import {
  AudioBlockData,
  FileBlockData,
  ImageBlockData,
  LessonBlockDto,
  LessonBlockType,
  TextBlockData,
  VideoBlockData,
} from '../../../../content/models';

type SimpleBlockType = 'Text' | 'Video' | 'Audio' | 'Image' | 'File' | 'CodeExercise';

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
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './lesson-editor.component.html',
  styleUrl: './lesson-editor.component.scss',
})
export class LessonEditorComponent implements OnDestroy {
  @Input({ required: true }) store!: CourseBuilderStore;

  private readonly router = inject(Router);
  private readonly content = inject(ContentService);
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
  };

  readonly blockTypes: BlockTypeOption[] = [
    { type: 'Text',         label: 'Текст',       desc: 'Параграф, заголовок, список',  icon: Type,       colorClass: 'le-bt--text' },
    { type: 'Video',        label: 'Видео',       desc: 'YouTube, Vimeo, ссылка',       icon: VideoIcon,  colorClass: 'le-bt--video' },
    { type: 'Image',        label: 'Изображение', desc: 'Картинка, схема, скриншот',    icon: ImageIcon,  colorClass: 'le-bt--image' },
    { type: 'Audio',        label: 'Аудио',       desc: 'Подкаст, лекция, запись',      icon: Headphones, colorClass: 'le-bt--audio' },
    { type: 'File',         label: 'Файл',        desc: 'PDF, DOCX, ZIP и другие',      icon: FileIcon,   colorClass: 'le-bt--file' },
    { type: 'CodeExercise', label: 'Упражнение',  desc: 'Задание или код',              icon: Code,       colorClass: 'le-bt--code' },
  ];

  readonly item = computed(() => this.store.selectedItem());
  readonly section = computed(() => this.store.selectedSection());

  readonly blocks = signal<LessonBlockDto[]>([]);
  readonly loading = signal(false);
  readonly inlineMenuFor = signal<number | 'top' | 'bottom' | null>(null);

  /** Поддержка только указанных типов в новом редакторе. Остальные → полный редактор. */
  readonly supportedTypes: ReadonlyArray<LessonBlockType> = [
    'Text', 'Video', 'Audio', 'Image', 'File', 'CodeExercise',
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
      case 'File':         return { type: 'File', attachmentId: '' };
      case 'CodeExercise': return {
        type: 'CodeExercise',
        instruction: '',
        starterCode: '',
        language: 'plaintext',
        executable: false,
        testCases: [],
      };
    }
  }

  addBlockAt(index: number, type: SimpleBlockType): void {
    const it = this.item();
    if (!it) return;
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
    this.content.update(block.id, { data: block.data, settings: block.settings }).subscribe({
      next: (saved) => {
        this.blocks.set(this.blocks().map((b) => (b.id === saved.id ? saved : b)));
      },
    });
  }

  private persistOrder(): void {
    const it = this.item();
    if (!it) return;
    const ids = this.blocks().map((b) => b.id);
    this.content.reorder(it.sourceId, ids).subscribe();
  }

  toggleInlineMenu(index: number | 'top' | 'bottom'): void {
    this.inlineMenuFor.update((curr) => (curr === index ? null : index));
  }

  isCustomType(type: LessonBlockType): boolean {
    return !this.supportedTypes.includes(type);
  }

  /** YouTube / Vimeo → embed URL */
  getEmbedUrl(url: string): string | null {
    if (!url) return null;
    const yt = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&?\s]+)/);
    if (yt) return `https://www.youtube.com/embed/${yt[1]}`;
    const vimeo = url.match(/vimeo\.com\/(\d+)/);
    if (vimeo) return `https://player.vimeo.com/video/${vimeo[1]}`;
    return null;
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
}
