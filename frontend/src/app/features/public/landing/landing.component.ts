import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  AlertCircle,
  ArrowRight,
  BarChart2,
  Bell,
  BookOpen,
  Calendar,
  CheckCircle,
  ChevronRight,
  Clock,
  Code2,
  CreditCard,
  GraduationCap,
  LucideAngularModule,
  MessageSquare,
  Play,
  ShieldCheck,
  Sparkles,
  Star,
  Users,
  Zap,
} from 'lucide-angular';
import { environment } from '../../../../environments/environment';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';
import { RevealDirective } from '../../../shared/directives/reveal.directive';

interface RoleEntry {
  key: 'student' | 'teacher' | 'admin';
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
  readonly sparklesIcon = Sparkles;
  readonly arrowRightIcon = ArrowRight;
  readonly chevronRightIcon = ChevronRight;
  readonly bookOpenIcon = BookOpen;
  readonly playIcon = Play;
  readonly checkCircleIcon = CheckCircle;
  readonly code2Icon = Code2;
  readonly clockIcon = Clock;
  readonly calendarIcon = Calendar;
  readonly usersIcon = Users;
  readonly shieldCheckIcon = ShieldCheck;
  readonly bellIcon = Bell;
  readonly zapIcon = Zap;
  readonly starIcon = Star;
  readonly alertIcon = AlertCircle;

  readonly showSwaggerLink = !environment.production;
  readonly swaggerUrl = '/swagger';

  // Текущая роль для авторизованных
  readonly currentRole = this.auth.userRole;
  readonly isAuth = this.auth.isAuthenticated;

  // Hero floating "role" widget — анимированное переключение
  readonly activeRoleIndex = signal(0);
  readonly heroProgress = signal(0);
  private roleTimer: ReturnType<typeof setInterval> | null = null;

  readonly heroRoles: { key: string; label: string }[] = [
    { key: 'student', label: 'STUDENT' },
    { key: 'teacher', label: 'TEACHER' },
    { key: 'admin', label: 'ADMIN' },
  ];

  readonly stats: StatEntry[] = [
    { value: '18', label: 'типов учебных блоков', icon: BookOpen, gradient: 'indigo-purple' },
    { value: '3', label: 'основные роли платформы', icon: Users, gradient: 'teal-cyan' },
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
    {
      key: 'admin',
      label: 'Admin',
      role: 'Admin',
      gradient: 'emerald-teal',
      badge: 'ADMIN',
      subtitle: 'Полный контроль над платформой: пользователи, модерация и аналитика',
      features: [
        'Управление пользователями',
        'Модерация курсов',
        'Platform health метрики',
        'Настройки платформы',
        'Архивация и экспорт',
        'Admin analytics',
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
      description: 'Дашборды для студента, преподавателя и администратора с прогрессом и KPI',
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
      description: 'Платные курсы, teacher payouts, refunds/disputes и subscription allocation',
      gradient: 'violet-purple',
    },
    {
      icon: ShieldCheck,
      title: 'Администрирование',
      description: 'Управление пользователями, модерация курсов, настройки и platform analytics',
      gradient: 'emerald-teal',
    },
  ];

  readonly devChecklist: string[] = [
    'One-click запуск start-local-dev.cmd для локального Stripe flow',
    'Swagger поднимается на backend в development-режиме',
    'Роуты разделены по ролям: student, teacher, admin',
  ];

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

  /** Куда вести "Открыть платформу" — на dashboard если авторизован, иначе на регистрацию */
  primaryCtaLink(): string {
    if (!this.isAuth()) {
      return '/register';
    }
    switch (this.currentRole()) {
      case UserRole.Student:
        return '/student/dashboard';
      case UserRole.Teacher:
        return '/teacher/dashboard';
      case UserRole.Admin:
        return '/admin/dashboard';
      default:
        return '/register';
    }
  }

  loginLink(): string {
    if (this.isAuth()) {
      return this.primaryCtaLink();
    }
    return '/login';
  }
}
