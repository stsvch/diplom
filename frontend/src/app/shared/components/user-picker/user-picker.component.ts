import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnInit,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Search, X, User as UserIcon } from 'lucide-angular';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { MessagingService } from '../../../features/messaging/services/messaging.service';
import { UserSummaryDto } from '../../../features/messaging/models/messaging.model';

@Component({
  selector: 'app-user-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  template: `
    <div class="picker-backdrop" (click)="close.emit()">
      <div class="picker" (click)="$event.stopPropagation()">
        <header class="picker__head">
          <h3>{{ title }}</h3>
          <button type="button" class="picker__close" (click)="close.emit()">
            <lucide-icon [img]="XIcon" [size]="20"></lucide-icon>
          </button>
        </header>

        <div class="picker__search">
          <lucide-icon [img]="SearchIcon" [size]="18"></lucide-icon>
          <input
            type="text"
            placeholder="Поиск по имени или email…"
            [ngModel]="query()"
            (ngModelChange)="onQueryChange($event)"
          />
        </div>

        <div class="picker__list">
          @if (loading()) {
            <div class="picker__loading">Поиск…</div>
          } @else if (users().length === 0) {
            <div class="picker__empty">
              <lucide-icon [img]="UserIconRef" [size]="24"></lucide-icon>
              <p>{{ query() ? 'Никого не найдено' : 'Начните вводить имя' }}</p>
            </div>
          } @else {
            @for (u of users(); track u.id) {
              <button
                type="button"
                class="picker__item"
                [disabled]="excludedIds.includes(u.id)"
                (click)="pick(u)">
                <div class="picker__avatar">{{ initials(u.fullName) }}</div>
                <div class="picker__info">
                  <span class="picker__name">{{ u.fullName || '(без имени)' }}</span>
                  <span class="picker__meta">{{ u.email }} · {{ roleLabel(u.role) }}</span>
                </div>
                @if (excludedIds.includes(u.id)) {
                  <span class="picker__badge">уже в чате</span>
                }
              </button>
            }
          }
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      :host { display: block; }
      .picker-backdrop {
        position: fixed; inset: 0; background: rgba(15, 23, 42, 0.45);
        display: flex; align-items: center; justify-content: center; z-index: 1000;
      }
      .picker {
        width: 480px; max-width: 95vw; max-height: 80vh;
        background: #fff; border-radius: 12px; overflow: hidden;
        display: flex; flex-direction: column;
        box-shadow: 0 16px 44px rgba(0,0,0,0.25);
      }
      .picker__head {
        display: flex; justify-content: space-between; align-items: center;
        padding: 14px 20px; border-bottom: 1px solid #E2E8F0;
      }
      .picker__head h3 { margin: 0; font-size: 1rem; font-weight: 600; }
      .picker__close {
        width: 32px; height: 32px; border: none; background: transparent;
        border-radius: 8px; cursor: pointer; color: #64748B;
        display: flex; align-items: center; justify-content: center;
      }
      .picker__close:hover { background: #F1F5F9; }
      .picker__search {
        display: flex; align-items: center; gap: 8px;
        padding: 10px 20px; border-bottom: 1px solid #E2E8F0; color: #64748B;
      }
      .picker__search input {
        flex: 1; border: none; outline: none; font-size: 0.9rem; background: transparent;
      }
      .picker__list { overflow-y: auto; flex: 1; padding: 6px; }
      .picker__loading, .picker__empty {
        padding: 28px; text-align: center; color: #64748B;
        display: flex; flex-direction: column; align-items: center; gap: 8px;
      }
      .picker__item {
        width: 100%; display: flex; align-items: center; gap: 10px;
        padding: 10px 12px; border: none; background: transparent; border-radius: 8px;
        cursor: pointer; text-align: left;
      }
      .picker__item:hover:not(:disabled) { background: #F1F5F9; }
      .picker__item:disabled { opacity: 0.5; cursor: not-allowed; }
      .picker__avatar {
        width: 36px; height: 36px; border-radius: 50%;
        background: #E0E7FF; color: #4338CA; display: flex;
        align-items: center; justify-content: center; font-weight: 600;
        font-size: 0.85rem; flex-shrink: 0;
      }
      .picker__info { display: flex; flex-direction: column; min-width: 0; flex: 1; }
      .picker__name { font-weight: 600; color: #0F172A; font-size: 0.9rem;
        white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
      .picker__meta { font-size: 0.75rem; color: #64748B;
        white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
      .picker__badge { font-size: 0.7rem; color: #64748B; background: #E2E8F0;
        padding: 2px 8px; border-radius: 999px; }
    `,
  ],
})
export class UserPickerComponent implements OnInit {
  @Input() title = 'Выберите пользователя';
  @Input() role?: string;
  @Input() excludedIds: string[] = [];
  @Output() close = new EventEmitter<void>();
  @Output() picked = new EventEmitter<UserSummaryDto>();

  private readonly messaging = inject(MessagingService);

  readonly SearchIcon = Search;
  readonly XIcon = X;
  readonly UserIconRef = UserIcon;

  readonly query = signal('');
  readonly users = signal<UserSummaryDto[]>([]);
  readonly loading = signal(false);

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.search$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((q) => this.runSearch(q));
    this.runSearch('');
  }

  onQueryChange(q: string): void {
    this.query.set(q);
    this.search$.next(q);
  }

  private runSearch(q: string): void {
    this.loading.set(true);
    this.messaging.searchUsers(q, this.role).subscribe({
      next: (list) => {
        this.users.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.users.set([]);
        this.loading.set(false);
      },
    });
  }

  pick(u: UserSummaryDto): void {
    if (this.excludedIds.includes(u.id)) return;
    this.picked.emit(u);
  }

  initials(name: string): string {
    return (name || '')
      .split(' ')
      .map((w) => w.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  roleLabel(role: string): string {
    switch (role) {
      case 'Teacher': return 'Преподаватель';
      case 'Student': return 'Студент';
      case 'Admin': return 'Админ';
      default: return role || '—';
    }
  }
}
