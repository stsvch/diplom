import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, Users, BookOpen, Archive, GraduationCap, UserX, MailWarning } from 'lucide-angular';
import { AdminService } from '../services/admin.service';
import { DashboardStatsDto } from '../models/admin.model';
import { parseApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  template: `
    <div class="dashboard">
      <h1 class="dashboard__title">Админ-дашборд</h1>

      @if (loading()) {
        <div class="state">Загрузка...</div>
      } @else if (error()) {
        <div class="state state--error">{{ error() }}</div>
      } @else if (stats(); as s) {
        <section class="dashboard__section">
          <h2>Пользователи</h2>
          <div class="kpis">
            <div class="kpi">
              <lucide-icon [img]="UsersIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.users.total }}</span><span class="kpi__label">Всего</span></div>
            </div>
            <div class="kpi">
              <lucide-icon [img]="GraduationIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.users.students }}</span><span class="kpi__label">Студентов</span></div>
            </div>
            <div class="kpi">
              <lucide-icon [img]="UsersIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.users.teachers }}</span><span class="kpi__label">Преподавателей</span></div>
            </div>
            <div class="kpi">
              <lucide-icon [img]="UsersIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.users.admins }}</span><span class="kpi__label">Админов</span></div>
            </div>
            <div class="kpi kpi--warn">
              <lucide-icon [img]="UserXIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.users.blocked }}</span><span class="kpi__label">Заблокировано</span></div>
            </div>
            <div class="kpi kpi--warn">
              <lucide-icon [img]="MailWarningIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.users.unconfirmedEmail }}</span><span class="kpi__label">Не подтверждён email</span></div>
            </div>
            <div class="kpi kpi--accent">
              <lucide-icon [img]="UsersIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">+{{ s.users.newLast7Days }}</span><span class="kpi__label">Новых за 7 дней</span></div>
            </div>
          </div>
        </section>

        <section class="dashboard__section">
          <h2>Курсы</h2>
          <div class="kpis">
            <div class="kpi">
              <lucide-icon [img]="BookOpenIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.courses.total }}</span><span class="kpi__label">Всего</span></div>
            </div>
            <div class="kpi kpi--accent">
              <lucide-icon [img]="BookOpenIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.courses.published }}</span><span class="kpi__label">Опубликовано</span></div>
            </div>
            <div class="kpi">
              <lucide-icon [img]="BookOpenIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.courses.drafts }}</span><span class="kpi__label">Черновиков</span></div>
            </div>
            <div class="kpi kpi--warn">
              <lucide-icon [img]="ArchiveIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.courses.archived }}</span><span class="kpi__label">В архиве</span></div>
            </div>
            <div class="kpi">
              <lucide-icon [img]="UsersIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.courses.totalEnrollments }}</span><span class="kpi__label">Активных записей</span></div>
            </div>
            <div class="kpi">
              <lucide-icon [img]="BookOpenIcon" size="20"></lucide-icon>
              <div><span class="kpi__value">{{ s.courses.disciplines }}</span><span class="kpi__label">Дисциплин</span></div>
            </div>
          </div>
        </section>
      }
    </div>
  `,
  styles: [`
    :host { display: block; }
    .dashboard { padding: 24px; }
    .dashboard__title { margin: 0 0 24px; font-size: 1.5rem; font-weight: 700; color: #0F172A; }
    .dashboard__section { margin-bottom: 28px; }
    .dashboard__section h2 { margin: 0 0 12px; font-size: 1.05rem; color: #475569; }
    .kpis {
      display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 12px;
    }
    .kpi {
      background: #fff; border: 1px solid #E2E8F0; border-radius: 10px;
      padding: 14px 16px; display: flex; align-items: center; gap: 12px;
    }
    .kpi div { display: flex; flex-direction: column; }
    .kpi__value { font-size: 1.5rem; font-weight: 700; color: #0F172A; }
    .kpi__label { font-size: 0.75rem; color: #64748B; }
    .kpi--accent { border-color: #C7D2FE; background: #EEF2FF; }
    .kpi--accent .kpi__value { color: #4338CA; }
    .kpi--warn { border-color: #FCD34D; background: #FEF3C7; }
    .kpi--warn .kpi__value { color: #92400E; }
    .state { padding: 20px; color: #64748B; }
    .state--error { color: #DC2626; }
  `],
})
export class AdminDashboardComponent implements OnInit {
  private readonly admin = inject(AdminService);

  readonly UsersIcon = Users;
  readonly BookOpenIcon = BookOpen;
  readonly ArchiveIcon = Archive;
  readonly GraduationIcon = GraduationCap;
  readonly UserXIcon = UserX;
  readonly MailWarningIcon = MailWarning;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly stats = signal<DashboardStatsDto | null>(null);

  ngOnInit(): void {
    this.loading.set(true);
    this.admin.getDashboard().subscribe({
      next: (s) => { this.stats.set(s); this.loading.set(false); },
      error: (err) => { this.error.set(parseApiError(err).message); this.loading.set(false); },
    });
  }
}
