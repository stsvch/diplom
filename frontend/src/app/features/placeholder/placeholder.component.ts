import { Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-placeholder',
  standalone: true,
  imports: [],
  template: `
    <div style="padding: 24px;">
      <h1 style="font-size: 1.5rem; font-weight: 700; color: #0F172A; margin-bottom: 8px;">
        {{ routeName() }}
      </h1>
      <p style="color: #64748B; font-size: 0.9375rem;">
        Эта страница находится в разработке.
      </p>
    </div>
  `,
})
export class PlaceholderComponent {
  private router = inject(Router);

  readonly routeName = computed(() => {
    const segments = this.router.url.split('/').filter(Boolean);
    const last = segments[segments.length - 1];
    const map: Record<string, string> = {
      dashboard: 'Дашборд',
      courses: 'Мои курсы',
      catalog: 'Каталог курсов',
      calendar: 'Календарь',
      messages: 'Сообщения',
      notifications: 'Уведомления',
      payments: 'Подписки и платежи',
      profile: 'Профиль',
      assignments: 'Проверка работ',
      gradebook: 'Журнал оценок',
      schedule: 'Расписание',
      reports: 'Отчёты',
      glossary: 'Словарь',
      users: 'Пользователи',
      disciplines: 'Дисциплины',
      analytics: 'Аналитика',
      settings: 'Настройки',
      create: 'Создать курс',
      login: 'Вход',
      register: 'Регистрация',
    };
    return map[last] ?? last ?? 'Страница';
  });
}
