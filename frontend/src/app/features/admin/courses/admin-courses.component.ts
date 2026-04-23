import { Component, OnInit, inject, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LucideAngularModule, Archive, Search, X } from 'lucide-angular';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { AdminService, PagedResult } from '../services/admin.service';
import { AdminCourseDto } from '../models/admin.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-admin-courses',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './admin-courses.component.html',
  styleUrl: './admin-courses.component.scss',
})
export class AdminCoursesComponent implements OnInit {
  private readonly admin = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly ArchiveIcon = Archive;
  readonly SearchIcon = Search;
  readonly XIcon = X;

  readonly searchText = signal('');
  readonly statusFilter = signal('');
  readonly page = signal(1);
  readonly pageSize = signal(20);

  readonly loading = signal(false);
  readonly data = signal<PagedResult<AdminCourseDto> | null>(null);
  readonly totalPages = computed(() => this.data()?.totalPages ?? 1);

  readonly archiveOpenFor = signal<AdminCourseDto | null>(null);
  readonly archiveReason = signal('');
  readonly archiving = signal(false);

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.search$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.load());

    this.load();
  }

  onSearchChange(): void { this.page.set(1); this.search$.next(this.searchText().trim()); }
  onFilterChange(): void { this.page.set(1); this.load(); }

  prevPage(): void { if (this.page() > 1) { this.page.update(v => v - 1); this.load(); } }
  nextPage(): void { if (this.page() < this.totalPages()) { this.page.update(v => v + 1); this.load(); } }

  load(): void {
    this.loading.set(true);
    this.admin.getCourses({
      search: this.searchText() || undefined,
      status: this.statusFilter() || undefined,
      page: this.page(),
      pageSize: this.pageSize(),
    }).subscribe({
      next: (r) => { this.data.set(r); this.loading.set(false); },
      error: (err) => { this.toast.error(parseApiError(err).message); this.loading.set(false); },
    });
  }

  openArchive(course: AdminCourseDto): void {
    this.archiveReason.set('');
    this.archiveOpenFor.set(course);
  }

  cancelArchive(): void {
    this.archiveOpenFor.set(null);
    this.archiveReason.set('');
  }

  submitArchive(): void {
    const course = this.archiveOpenFor();
    if (!course) return;
    const reason = this.archiveReason().trim();
    if (!reason) {
      this.toast.warning('Укажите причину');
      return;
    }
    this.archiving.set(true);
    this.admin.forceArchiveCourse(course.id, { reason }).subscribe({
      next: () => {
        this.archiving.set(false);
        this.cancelArchive();
        this.toast.success('Курс архивирован');
        this.load();
      },
      error: (err) => {
        this.archiving.set(false);
        this.toast.error(parseApiError(err).message);
      },
    });
  }

  statusLabel(c: AdminCourseDto): string {
    if (c.isArchived) return 'В архиве';
    if (c.isPublished) return 'Опубликован';
    return 'Черновик';
  }

  statusClass(c: AdminCourseDto): string {
    if (c.isArchived) return 'badge--warn';
    if (c.isPublished) return 'badge--ok';
    return 'badge--neutral';
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }
}
