import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Plus,
  Users,
  User,
  Calendar,
  Clock,
  Link,
  X,
  CheckCircle,
  Check,
  BookOpen,
  Copy,
  ChevronDown,
  Loader2,
} from 'lucide-angular';
import { SchedulingService } from '../services/scheduling.service';
import { ScheduleSlotDto, SlotStatus, CreateSlotRequest } from '../models/scheduling.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { parseApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-teacher-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, ButtonComponent, BadgeComponent],
  templateUrl: './teacher-schedule.component.html',
  styleUrl: './teacher-schedule.component.scss',
})
export class TeacherScheduleComponent implements OnInit {
  private readonly schedulingService = inject(SchedulingService);
  private readonly toastService = inject(ToastService);

  readonly PlusIcon = Plus;
  readonly UsersIcon = Users;
  readonly UserIcon = User;
  readonly CalendarIcon = Calendar;
  readonly ClockIcon = Clock;
  readonly LinkIcon = Link;
  readonly XIcon = X;
  readonly CheckCircleIcon = CheckCircle;
  readonly CheckIcon = Check;
  readonly BookOpenIcon = BookOpen;
  readonly CopyIcon = Copy;
  readonly ChevronDownIcon = ChevronDown;
  readonly Loader2Icon = Loader2;

  readonly SlotStatus = SlotStatus;

  readonly activeTab = signal<'upcoming' | 'completed'>('upcoming');
  readonly slots = signal<ScheduleSlotDto[]>([]);
  readonly loading = signal(false);
  readonly cancelling = signal<string | null>(null);
  readonly completing = signal<string | null>(null);
  readonly showCreateModal = signal(false);
  readonly creating = signal(false);

  // Create form
  readonly form = signal({
    isGroupSession: false,
    date: '',
    startTime: '',
    endTime: '',
    courseId: '',
    courseName: '',
    maxStudents: 1,
    meetingLink: '',
    title: '',
    description: '',
  });

  readonly upcomingSlots = computed(() =>
    this.slots().filter(
      (s) => s.status === SlotStatus.Available || s.status === SlotStatus.Booked,
    ),
  );

  readonly completedSlots = computed(() =>
    this.slots().filter(
      (s) => s.status === SlotStatus.Completed || s.status === SlotStatus.Cancelled,
    ),
  );

  readonly displayedSlots = computed(() =>
    this.activeTab() === 'upcoming' ? this.upcomingSlots() : this.completedSlots(),
  );

  ngOnInit(): void {
    this.loadSlots();
  }

  private loadSlots(): void {
    this.loading.set(true);
    this.schedulingService.getMySlots().subscribe({
      next: (slots) => {
        this.slots.set(slots);
        this.loading.set(false);
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  setTab(tab: 'upcoming' | 'completed'): void {
    this.activeTab.set(tab);
  }

  openCreateModal(): void {
    this.form.set({
      isGroupSession: false,
      date: '',
      startTime: '',
      endTime: '',
      courseId: '',
      courseName: '',
      maxStudents: 1,
      meetingLink: '',
      title: '',
      description: '',
    });
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  updateForm(field: string, value: unknown): void {
    this.form.update((f) => ({ ...f, [field]: value }));
  }

  createSlot(): void {
    const f = this.form();
    if (!f.date || !f.startTime || !f.endTime || !f.title) {
      this.toastService.error('Заполните обязательные поля.');
      return;
    }

    const startTime = new Date(`${f.date}T${f.startTime}`).toISOString();
    const endTime = new Date(`${f.date}T${f.endTime}`).toISOString();

    if (new Date(endTime) <= new Date(startTime)) {
      this.toastService.error('Время окончания должно быть позже времени начала.');
      return;
    }

    const request: CreateSlotRequest = {
      title: f.title,
      description: f.description || undefined,
      startTime,
      endTime,
      isGroupSession: f.isGroupSession,
      maxStudents: f.isGroupSession ? f.maxStudents : 1,
      courseId: f.courseId || undefined,
      courseName: f.courseName || undefined,
      meetingLink: f.meetingLink || undefined,
    };

    this.creating.set(true);
    this.schedulingService.createSlot(request).subscribe({
      next: (slot) => {
        this.slots.update((s) => [slot, ...s]);
        this.creating.set(false);
        this.showCreateModal.set(false);
        this.toastService.success('Занятие создано!');
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.creating.set(false);
      },
    });
  }

  cancelSlot(slot: ScheduleSlotDto, event: Event): void {
    event.stopPropagation();
    if (!confirm('Отменить это занятие?')) return;

    this.cancelling.set(slot.id);
    this.schedulingService.cancelSlot(slot.id).subscribe({
      next: () => {
        this.slots.update((s) =>
          s.map((x) => (x.id === slot.id ? { ...x, status: SlotStatus.Cancelled } : x)),
        );
        this.cancelling.set(null);
        this.toastService.success('Занятие отменено.');
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.cancelling.set(null);
      },
    });
  }

  completeSlot(slot: ScheduleSlotDto, event: Event): void {
    event.stopPropagation();
    this.completing.set(slot.id);
    this.schedulingService.completeSlot(slot.id).subscribe({
      next: () => {
        this.slots.update((s) =>
          s.map((x) => (x.id === slot.id ? { ...x, status: SlotStatus.Completed } : x)),
        );
        this.completing.set(null);
        this.toastService.success('Занятие завершено.');
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.completing.set(null);
      },
    });
  }

  copyLink(link: string, event: Event): void {
    event.stopPropagation();
    navigator.clipboard.writeText(link).then(() => {
      this.toastService.success('Ссылка скопирована!');
    });
  }

  formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: 'long',
      year: 'numeric',
    });
  }

  formatTime(dateStr: string): string {
    const d = new Date(dateStr);
    return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  }

  getStatusVariant(status: SlotStatus): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    switch (status) {
      case SlotStatus.Available:
        return 'primary';
      case SlotStatus.Booked:
        return 'warning';
      case SlotStatus.Completed:
        return 'success';
      case SlotStatus.Cancelled:
        return 'danger';
    }
  }

  getStatusLabel(status: SlotStatus): string {
    switch (status) {
      case SlotStatus.Available:
        return 'Доступно';
      case SlotStatus.Booked:
        return 'Занято';
      case SlotStatus.Completed:
        return 'Завершено';
      case SlotStatus.Cancelled:
        return 'Отменено';
    }
  }
}
