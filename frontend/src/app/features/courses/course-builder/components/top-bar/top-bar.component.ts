import { Component, Input, Output, EventEmitter, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  AlertCircle,
  ArrowLeft,
  BookOpen,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  Eye,
  LucideAngularModule,
  Menu,
  PanelRight,
  Save,
  Upload,
  X,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';

@Component({
  selector: 'app-cb-top-bar',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './top-bar.component.html',
  styleUrl: './top-bar.component.scss',
})
export class TopBarComponent {
  @Input({ required: true }) store!: CourseBuilderStore;
  @Output() toggleLeft = new EventEmitter<void>();
  @Output() toggleRight = new EventEmitter<void>();

  readonly icons = {
    arrowLeft: ArrowLeft,
    bookOpen: BookOpen,
    eye: Eye,
    upload: Upload,
    check: CheckCircle2,
    alert: AlertCircle,
    save: Save,
    x: X,
    menu: Menu,
    panelRight: PanelRight,
    chevronLeft: ChevronLeft,
    chevronRight: ChevronRight,
  };

  readonly showPublishModal = signal(false);
  readonly showBackModal = signal(false);

  readonly statusLabel = computed(() => {
    const c = this.store.course();
    if (!c) return '';
    if (c.isArchived) return 'В архиве';
    if (c.isPublished) return 'Опубликован';
    return 'Черновик';
  });

  readonly statusClass = computed(() => {
    const c = this.store.course();
    if (!c) return '';
    if (c.isArchived) return 'tb-status--archived';
    if (c.isPublished) return 'tb-status--published';
    return 'tb-status--draft';
  });

  constructor(private router: Router) {}

  onPublishClick(): void {
    const r = this.store.readiness();
    if (r && r.errorCount > 0 && !this.store.course()?.isPublished) {
      this.showPublishModal.set(true);
    } else {
      this.store.publish();
    }
  }

  forcePublish(): void {
    this.showPublishModal.set(false);
    this.store.publish(true);
  }

  goBack(): void {
    if (this.store.isDirty()) {
      this.showBackModal.set(true);
    } else {
      this.router.navigate(['/teacher/courses']);
    }
  }

  exitWithoutSaving(): void {
    this.showBackModal.set(false);
    this.router.navigate(['/teacher/courses']);
  }

  goPreview(): void {
    const id = this.store.courseId();
    this.router.navigate(['/teacher/courses', id, 'preview']);
  }
}
