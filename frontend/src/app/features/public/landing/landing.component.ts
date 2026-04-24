import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  BookOpen,
  CalendarDays,
  ChartColumn,
  CreditCard,
  GraduationCap,
  LucideAngularModule,
  MessageSquare,
  ShieldCheck,
  Sparkles,
} from 'lucide-angular';
import { environment } from '../../../../environments/environment';

interface LandingFeature {
  icon: typeof BookOpen;
  title: string;
  description: string;
}

interface LandingMetric {
  value: string;
  label: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
})
export class LandingComponent {
  readonly bookIcon = BookOpen;
  readonly sparklesIcon = Sparkles;

  readonly metrics: LandingMetric[] = [
    { value: '18', label: 'типов учебных блоков' },
    { value: '3', label: 'основные роли платформы' },
    { value: '1-click', label: 'локальный запуск со Stripe' },
    { value: 'Real-time', label: 'уведомления и чаты' },
  ];

  readonly features: LandingFeature[] = [
    {
      icon: GraduationCap,
      title: 'Курсы и контент',
      description: 'Курсы, модули, уроки, тесты, задания и блочный редактор с 18 типами контента.',
    },
    {
      icon: ChartColumn,
      title: 'Прогресс и аналитика',
      description: 'Дашборды для студента, преподавателя и администратора с прогрессом и KPI.',
    },
    {
      icon: CalendarDays,
      title: 'Расписание и события',
      description: 'Календарь, занятия с преподавателем, дедлайны и реальные уведомления.',
    },
    {
      icon: MessageSquare,
      title: 'Коммуникация',
      description: 'Course/direct chats, unread sync, уведомления и переходы в нужный контекст.',
    },
    {
      icon: CreditCard,
      title: 'Оплата и подписки',
      description: 'Платные курсы, teacher payouts, refunds/disputes и subscription allocation.',
    },
    {
      icon: ShieldCheck,
      title: 'Администрирование',
      description: 'Управление пользователями, модерация курсов, настройки и platform analytics.',
    },
  ];

  readonly swaggerUrl = this.resolveSwaggerUrl();

  private resolveSwaggerUrl(): string {
    const apiUrl = environment.apiUrl ?? '/api';
    return apiUrl.endsWith('/api') ? `${apiUrl.slice(0, -4)}/swagger` : '/swagger';
  }
}
