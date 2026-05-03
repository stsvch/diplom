import { Component, Input, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  AlertCircle,
  Calendar,
  Clock,
  Edit3,
  ExternalLink,
  Link2,
  LucideAngularModule,
  Users,
  Video,
} from 'lucide-angular';
import { CourseBuilderStore } from '../../state/course-builder.store';

@Component({
  selector: 'app-cb-live-session-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './live-session-editor.component.html',
  styleUrl: './live-session-editor.component.scss',
})
export class LiveSessionEditorComponent {
  @Input({ required: true }) store!: CourseBuilderStore;
  private readonly router = inject(Router);

  readonly icons = {
    video: Video,
    cal: Calendar,
    clock: Clock,
    link: Link2,
    users: Users,
    alert: AlertCircle,
    edit: Edit3,
    external: ExternalLink,
  };

  readonly item = computed(() => this.store.selectedItem());
  readonly section = computed(() => this.store.selectedSection());

  readonly isReady = computed(() => {
    const it = this.item();
    return !!(it?.startTime && it?.meetingLink);
  });

  setTitle(value: string): void {
    const it = this.item();
    if (it) this.store.updateItemTitle(it, value);
  }

  setRequired(value: boolean): void {
    const it = this.item();
    if (it) this.store.setItemRequired(it, value);
  }

  openFullEditor(): void {
    this.router.navigate(['/teacher/schedule']);
  }

  formatDate(s: string | null | undefined): string {
    if (!s) return '';
    return new Date(s).toLocaleString('ru-RU', {
      day: '2-digit',
      month: 'long',
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
