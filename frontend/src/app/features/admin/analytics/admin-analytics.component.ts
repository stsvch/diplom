import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  AlertTriangle,
  ArrowRight,
  BadgeDollarSign,
  BookOpen,
  CreditCard,
  GraduationCap,
  LucideAngularModule,
  RefreshCcw,
  TrendingUp,
  UserRoundPlus,
  Users,
} from 'lucide-angular';
import { parseApiError } from '../../../core/models/api-error.model';
import { StatsCardComponent } from '../../../shared/components/stats-card/stats-card.component';
import {
  AdminAnalyticsDto,
  AdminMoneyAmountDto,
  AdminAnalyticsTopCourseDto,
  AdminAnalyticsTopTeacherDto,
  AdminAnalyticsTrendPointDto,
} from '../models/admin.model';
import { AdminService } from '../services/admin.service';

@Component({
  selector: 'app-admin-analytics',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    LucideAngularModule,
    StatsCardComponent,
  ],
  templateUrl: './admin-analytics.component.html',
  styleUrl: './admin-analytics.component.scss',
})
export class AdminAnalyticsComponent implements OnInit {
  private readonly admin = inject(AdminService);

  readonly UsersIcon = Users;
  readonly UserGrowthIcon = UserRoundPlus;
  readonly CoursesIcon = BookOpen;
  readonly RevenueIcon = BadgeDollarSign;
  readonly SubscriptionsIcon = CreditCard;
  readonly CommissionIcon = TrendingUp;
  readonly RiskIcon = AlertTriangle;
  readonly ActivityIcon = RefreshCcw;
  readonly ArrowRightIcon = ArrowRight;
  readonly TeachersIcon = GraduationCap;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly analytics = signal<AdminAnalyticsDto | null>(null);

  readonly summary = computed(() => this.analytics()?.summary ?? null);
  readonly payments = computed(() => this.analytics()?.payments ?? null);
  readonly subscriptions = computed(() => this.analytics()?.subscriptions ?? null);
  readonly trends = computed(() => this.analytics()?.trends ?? []);
  readonly topCourses = computed(() => this.analytics()?.topCourses ?? []);
  readonly topTeachers = computed(() => this.analytics()?.topTeachers ?? []);

  readonly maxTrendUsers = computed(() =>
    Math.max(...this.trends().map((item) => item.newUsers), 1),
  );
  readonly maxTrendEnrollments = computed(() =>
    Math.max(...this.trends().map((item) => item.newEnrollments), 1),
  );
  readonly maxTrendRevenue = computed(() =>
    Math.max(...this.trends().map((item) => this.getSingleCurrencyAmount(item.revenueByCurrency)), 1),
  );
  readonly maxCourseRevenue = computed(() =>
    Math.max(...this.topCourses().map((item) => item.grossRevenue), 1),
  );
  readonly maxTeacherRevenue = computed(() =>
    Math.max(...this.topTeachers().map((item) => item.grossRevenue), 1),
  );
  readonly canCompareCourseRevenue = computed(() =>
    this.countDistinctCurrencies(this.topCourses().map((item) => item.currency)) <= 1,
  );
  readonly canCompareTeacherRevenue = computed(() =>
    this.countDistinctCurrencies(this.topTeachers().map((item) => item.currency)) <= 1,
  );

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.loading.set(true);
    this.error.set(null);

    this.admin.getAnalytics().subscribe({
      next: (analytics) => {
        this.analytics.set(analytics);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  formatMoney(value: number, currency: string): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: currency.toUpperCase(),
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
    }).format(value);
  }

  formatMoneyBreakdown(values: AdminMoneyAmountDto[]): string {
    if (!values.length) {
      return '—';
    }

    return values
      .map((value) => this.formatMoney(value.amount, value.currency))
      .join(' • ');
  }

  hasSingleCurrency(values: AdminMoneyAmountDto[]): boolean {
    return values.length === 1;
  }

  getUsersWidth(item: AdminAnalyticsTrendPointDto): number {
    return (item.newUsers / this.maxTrendUsers()) * 100;
  }

  getEnrollmentsWidth(item: AdminAnalyticsTrendPointDto): number {
    return (item.newEnrollments / this.maxTrendEnrollments()) * 100;
  }

  getRevenueWidth(item: AdminAnalyticsTrendPointDto): number {
    const amount = this.getSingleCurrencyAmount(item.revenueByCurrency);
    return amount > 0 ? (amount / this.maxTrendRevenue()) * 100 : 0;
  }

  getCourseRevenueWidth(item: AdminAnalyticsTopCourseDto): number {
    return (item.grossRevenue / this.maxCourseRevenue()) * 100;
  }

  getTeacherRevenueWidth(item: AdminAnalyticsTopTeacherDto): number {
    return (item.grossRevenue / this.maxTeacherRevenue()) * 100;
  }

  private getSingleCurrencyAmount(values: AdminMoneyAmountDto[]): number {
    return values.length === 1 ? values[0].amount : 0;
  }

  private countDistinctCurrencies(values: string[]): number {
    return new Set(values.filter((value) => !!value)).size;
  }
}
