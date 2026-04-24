import { Component, OnInit, inject, signal, computed, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LucideAngularModule, Lock, Unlock, UserCog, Trash2, UserPlus, X, Search } from 'lucide-angular';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { AdminService, PagedResult } from '../services/admin.service';
import { AdminUserDto } from '../models/admin.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss',
})
export class AdminUsersComponent implements OnInit {
  private readonly admin = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authService = inject(AuthService);

  readonly LockIcon = Lock;
  readonly UnlockIcon = Unlock;
  readonly UserCogIcon = UserCog;
  readonly Trash2Icon = Trash2;
  readonly UserPlusIcon = UserPlus;
  readonly XIcon = X;
  readonly SearchIcon = Search;

  readonly roles = ['Admin', 'Teacher', 'Student'];

  readonly searchText = signal('');
  readonly roleFilter = signal<string>('');
  readonly onlyBlocked = signal(false);
  readonly page = signal(1);
  readonly pageSize = signal(20);

  readonly loading = signal(false);
  readonly data = signal<PagedResult<AdminUserDto> | null>(null);
  readonly currentUserId = computed(() => this.authService.currentUser()?.id ?? null);

  readonly totalPages = computed(() => this.data()?.totalPages ?? 1);

  readonly createOpen = signal(false);
  readonly createForm = signal({
    email: '',
    firstName: '',
    lastName: '',
    role: 'Student',
    password: '',
  });
  readonly creating = signal(false);

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

  onSearchChange(): void {
    this.page.set(1);
    this.search$.next(this.searchText().trim());
  }

  onFilterChange(): void {
    this.page.set(1);
    this.load();
  }

  prevPage(): void {
    if (this.page() > 1) { this.page.update((v) => v - 1); this.load(); }
  }

  nextPage(): void {
    if (this.page() < this.totalPages()) { this.page.update((v) => v + 1); this.load(); }
  }

  load(): void {
    this.loading.set(true);
    this.admin.getUsers({
      search: this.searchText() || undefined,
      role: this.roleFilter() || undefined,
      onlyBlocked: this.onlyBlocked() || undefined,
      page: this.page(),
      pageSize: this.pageSize(),
    }).subscribe({
      next: (r) => { this.data.set(r); this.loading.set(false); },
      error: (err) => { this.toast.error(parseApiError(err).message); this.loading.set(false); },
    });
  }

  blockUser(u: AdminUserDto): void {
    this.admin.blockUser(u.id).subscribe({
      next: () => { this.toast.success('Пользователь заблокирован'); this.load(); },
      error: (err) => this.toast.error(parseApiError(err).message),
    });
  }

  unblockUser(u: AdminUserDto): void {
    this.admin.unblockUser(u.id).subscribe({
      next: () => { this.toast.success('Пользователь разблокирован'); this.load(); },
      error: (err) => this.toast.error(parseApiError(err).message),
    });
  }

  changeRole(u: AdminUserDto): void {
    const role = prompt('Новая роль (Admin / Teacher / Student):', u.role);
    if (!role || !this.roles.includes(role)) return;
    this.admin.changeUserRole(u.id, { role }).subscribe({
      next: () => { this.toast.success('Роль обновлена'); this.load(); },
      error: (err) => this.toast.error(parseApiError(err).message),
    });
  }

  deleteUser(u: AdminUserDto): void {
    if (!confirm(`Удалить пользователя ${u.fullName || u.email}? Это необратимо.`)) return;
    this.admin.deleteUser(u.id).subscribe({
      next: () => { this.toast.success('Пользователь удалён'); this.load(); },
      error: (err) => this.toast.error(parseApiError(err).message),
    });
  }

  openCreate(): void {
    this.createForm.set({ email: '', firstName: '', lastName: '', role: 'Student', password: '' });
    this.createOpen.set(true);
  }

  cancelCreate(): void {
    this.createOpen.set(false);
  }

  updateForm<K extends 'email' | 'firstName' | 'lastName' | 'role' | 'password'>(key: K, value: string): void {
    this.createForm.update((f) => ({ ...f, [key]: value }));
  }

  submitCreate(): void {
    const f = this.createForm();
    if (!f.email || !f.firstName || !f.password) {
      this.toast.warning('Заполните обязательные поля');
      return;
    }
    this.creating.set(true);
    this.admin.createUser(f).subscribe({
      next: () => {
        this.creating.set(false);
        this.createOpen.set(false);
        this.toast.success('Пользователь создан');
        this.load();
      },
      error: (err) => {
        this.creating.set(false);
        this.toast.error(parseApiError(err).message);
      },
    });
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  roleLabel(role: string): string {
    switch (role) {
      case 'Admin': return 'Админ';
      case 'Teacher': return 'Преподаватель';
      case 'Student': return 'Студент';
      default: return role;
    }
  }

  canManageUser(user: AdminUserDto): boolean {
    return this.currentUserId() !== user.id;
  }
}
