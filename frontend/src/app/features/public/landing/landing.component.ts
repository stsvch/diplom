// landing.component.ts
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ArrowRight,
  BarChart2,
  Bell,
  BookOpen,
  Calendar,
  CheckCircle,
  Clock,
  CreditCard,
  GraduationCap,
  LucideAngularModule,
  MessageSquare,
  Users,
  Zap,
} from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';
import { RevealDirective } from '../../../shared/directives/reveal.directive';

interface RoleEntry {
  key: 'student' | 'teacher';
  label: string;
  gradient: string;
  badge: string;
  role: string;
  subtitle: string;
  features: string[];
}

interface FeatureEntry {
  icon: typeof BookOpen;
  title: string;
  description: string;
  gradient: string;
}

interface StatEntry {
  value: string;
  label: string;
  icon: typeof BookOpen;
  gradient: string;
}

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, LucideAngularModule, RevealDirective],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);

  // Иконки
  readonly graduationCapIcon = GraduationCap;
  readonly arrowRightIcon = ArrowRight;
  readonly bookOpenIcon = BookOpen;
  readonly checkCircleIcon = CheckCircle;
  readonly clockIcon = Clock;
  readonly calendarIcon = Calendar;

  // Текущая роль для авторизованных
  readonly currentRole = this.auth.userRole;
  readonly isAuth = this.auth.isAuthenticated;

  // Hero floating "role" widget — анимированное переключение
  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  readonly activeRoleIndex = signal(0);
  readonly heroProgress = signal(0);
  private roleTimer: ReturnType<typeof setInterval> | null = null;

  readonly heroRoles: { key: 'student' | 'teacher'; label: string }[] = [
    { key: 'student', label: 'STUDENT' },
    { key: 'teacher', label: 'TEACHER' },
  ];

  readonly stats: StatEntry[] = [
    { value: '18', label: 'типов учебных блоков', icon: BookOpen, gradient: 'indigo-purple' },
    { value: '2', label: 'роли — студент и преподаватель', icon: Users, gradient: 'teal-cyan' },
    { value: '1-click', label: 'локальный запуск со Stripe', icon: Zap, gradient: 'amber-orange' },
    { value: 'Real-time', label: 'уведомления и чаты', icon: Bell, gradient: 'rose-pink' },
  ];

  readonly roles: RoleEntry[] = [
    {
      key: 'student',
      label: 'Student',
      role: 'Student',
      gradient: 'indigo-purple',
      badge: 'STUDENT',
      subtitle: 'Персональный учебный центр с прогрессом, расписанием и аналитикой',
      features: [
        'Dashboard с прогрессом и KPI',
        'Словарь и флеш-карточки',
        'Кодовые упражнения',
        'Оценки и история активности',
        'Расписание и дедлайны',
        'Личные сообщения',
      ],
    },
    {
      key: 'teacher',
      label: 'Teacher',
      role: 'Teacher',
      gradient: 'amber-orange',
      badge: 'TEACHER',
      subtitle: 'Операционный кабинет для управления курсами, студентами и выплатами',
      features: [
        'Редактор курсов (18 блоков)',
        'Code review и проверка работ',
        'Аналитика по курсу',
        'Расписание занятий',
        'Teacher payouts через Stripe',
        'Общение со студентами',
      ],
    },
  ];

  readonly features: FeatureEntry[] = [
    {
      icon: BookOpen,
      title: 'Курсы и контент',
      description: 'Курсы, модули, уроки, тесты, задания и блочный редактор с 18 типами контента',
      gradient: 'indigo-purple',
    },
    {
      icon: BarChart2,
      title: 'Прогресс и аналитика',
      description: 'Дашборды для студента и преподавателя с прогрессом и KPI',
      gradient: 'teal-cyan',
    },
    {
      icon: Calendar,
      title: 'Расписание и события',
      description: 'Календарь, занятия с преподавателем, дедлайны и реальные уведомления',
      gradient: 'amber-orange',
    },
    {
      icon: MessageSquare,
      title: 'Коммуникация',
      description: 'Course/direct chats, unread sync, уведомления и переходы в нужный контекст',
      gradient: 'rose-pink',
    },
    {
      icon: CreditCard,
      title: 'Оплата и подписки',
      description: 'Платные курсы, teacher payouts, подписки и платформенная выручка',
      gradient: 'violet-purple',
    },
  ];

  // Lifecycle hook запускает первичную загрузку или очистку ресурсов компонента.
  ngOnInit(): void {
    // Цикл анимации роли (каждые 2.5 сек)
    this.roleTimer = setInterval(() => {
      this.activeRoleIndex.update((i) => (i + 1) % this.heroRoles.length);
    }, 2500);

    // Анимация прогресс-кольца — стартует через 600мс после маунта
    setTimeout(() => this.heroProgress.set(65), 600);
  }

  ngOnDestroy(): void {
    if (this.roleTimer) {
      clearInterval(this.roleTimer);
      this.roleTimer = null;
    }
  }

  /** Куда вести "В кабинет" — на dashboard если авторизован */
  primaryCtaLink(): string {
    if (!this.isAuth()) {
      return '/login';
    }
    switch (this.currentRole()) {
      case UserRole.Student:
        return '/student/dashboard';
      case UserRole.Teacher:
        return '/teacher/dashboard';
      case UserRole.Admin:
        return '/admin/dashboard';
      default:
        return '/login';
    }
  }
}
