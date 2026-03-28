import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Calendar,
  Clock,
  Users,
  User,
  BookOpen,
  Search,
  CheckCircle,
  X,
  Loader2,
  BookmarkCheck,
} from 'lucide-angular';
import { SchedulingService } from '../services/scheduling.service';
import { ScheduleSlotDto, SlotStatus } from '../models/scheduling.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { parseApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-student-schedule',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule, ButtonComponent, BadgeComponent],
  templateUrl: './student-schedule.component.html',
  styleUrl: './student-schedule.component.scss',
})
export class StudentScheduleComponent implements OnInit {
  private readonly schedulingService = inject(SchedulingService);
  private readonly toastService = inject(ToastService);

  readonly CalendarIcon = Calendar;
  readonly ClockIcon = Clock;
  readonly UsersIcon = Users;
  readonly UserIcon = User;
  readonly BookOpenIcon = BookOpen;
  readonly SearchIcon = Search;
  readonly CheckCircleIcon = CheckCircle;
  readonly XIcon = X;
  readonly Loader2Icon = Loader2;
  readonly BookmarkCheckIcon = BookmarkCheck;

  readonly SlotStatus = SlotStatus;

  readonly activeTab = signal<'available' | 'myBookings'>('available');
  readonly availableSlots = signal<ScheduleSlotDto[]>([]);
  readonly myBookings = signal<ScheduleSlotDto[]>([]);
  readonly loadingAvailable = signal(false);
  readonly loadingBookings = signal(false);
  readonly booking = signal<string | null>(null);
  readonly cancelling = signal<string | null>(null);
  readonly courseFilter = signal('');

  readonly filteredSlots = computed(() => {
    const filter = this.courseFilter().toLowerCase().trim();
    const slots = this.availableSlots();
    if (!filter) return slots;
    return slots.filter(
      (s) =>
        (s.courseName ?? '').toLowerCase().includes(filter) ||
        s.title.toLowerCase().includes(filter) ||
        s.teacherName.toLowerCase().includes(filter),
    );
  });

  readonly upcomingBookings = computed(() => {
    const now = new Date();
    return this.myBookings().filter((s) => new Date(s.startTime) > now);
  });

  readonly pastBookings = computed(() => {
    const now = new Date();
    return this.myBookings().filter((s) => new Date(s.startTime) <= now);
  });

  ngOnInit(): void {
    this.loadAvailableSlots();
    this.loadMyBookings();
  }

  private loadAvailableSlots(): void {
    this.loadingAvailable.set(true);
    this.schedulingService.getAvailableSlots().subscribe({
      next: (slots) => {
        this.availableSlots.set(slots);
        this.loadingAvailable.set(false);
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.loadingAvailable.set(false);
      },
    });
  }

  private loadMyBookings(): void {
    this.loadingBookings.set(true);
    this.schedulingService.getMyBookings().subscribe({
      next: (slots) => {
        this.myBookings.set(slots);
        this.loadingBookings.set(false);
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.loadingBookings.set(false);
      },
    });
  }

  setTab(tab: 'available' | 'myBookings'): void {
    this.activeTab.set(tab);
  }

  bookSlot(slot: ScheduleSlotDto): void {
    this.booking.set(slot.id);
    this.schedulingService.bookSlot(slot.id).subscribe({
      next: () => {
        this.booking.set(null);
        this.toastService.success('Вы успешно записались на занятие!');
        this.loadAvailableSlots();
        this.loadMyBookings();
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.booking.set(null);
      },
    });
  }

  cancelBooking(slot: ScheduleSlotDto): void {
    if (!confirm('Отменить запись на это занятие?')) return;
    this.cancelling.set(slot.id);
    this.schedulingService.cancelBooking(slot.id).subscribe({
      next: () => {
        this.cancelling.set(null);
        this.toastService.success('Запись отменена.');
        this.loadAvailableSlots();
        this.loadMyBookings();
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.cancelling.set(null);
      },
    });
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ru-RU', {
      day: '2-digit',
      month: 'long',
      year: 'numeric',
    });
  }

  formatTime(dateStr: string): string {
    return new Date(dateStr).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  }

  getSpotsLeft(slot: ScheduleSlotDto): number {
    return slot.maxStudents - slot.bookedCount;
  }
}
