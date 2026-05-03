import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  Activity,
  Archive,
  BookOpen,
  Globe,
  GraduationCap,
  LucideAngularModule,
  MailWarning,
  ShieldCheck,
  TrendingUp,
  UserCheck,
  UserX,
  Users,
} from 'lucide-angular';
import { AdminService } from '../services/admin.service';
import { DashboardStatsDto } from '../models/admin.model';
import { parseApiError } from '../../../core/models/api-error.model';
import { StatsCardComponent } from '../../../shared/components/stats-card/stats-card.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, StatsCardComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent implements OnInit {
  private readonly admin = inject(AdminService);

  readonly UsersIcon = Users;
  readonly BookOpenIcon = BookOpen;
  readonly ArchiveIcon = Archive;
  readonly GraduationIcon = GraduationCap;
  readonly UserXIcon = UserX;
  readonly UserCheckIcon = UserCheck;
  readonly MailWarningIcon = MailWarning;
  readonly TrendingUpIcon = TrendingUp;
  readonly ShieldIcon = ShieldCheck;
  readonly ActivityIcon = Activity;
  readonly GlobeIcon = Globe;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly stats = signal<DashboardStatsDto | null>(null);

  readonly publishedShare = computed(() => {
    const s = this.stats();
    if (!s || s.courses.total === 0) return 0;
    return Math.round((s.courses.published / s.courses.total) * 100);
  });

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set(null);
    this.admin.getDashboard().subscribe({
      next: (s) => {
        this.stats.set(s);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }
}
