import { Component, OnInit, inject, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, LucideIconData, Star, Clock, MessageCircle, BookOpen, Trophy, Bell } from 'lucide-angular';
import { NotificationsService } from '../services/notifications.service';
import { NotificationDto, NotificationType } from '../models/notification.model';
import { parseApiError } from '../../../core/models/api-error.model';
import { BadgeComponent } from '../../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { SignalRService } from '../../../core/services/signalr.service';

type FilterTab = 'all' | 'unread' | 'read';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, BadgeComponent, ButtonComponent],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.scss',
})
export class NotificationsPageComponent implements OnInit {
  private readonly notificationsService = inject(NotificationsService);
  private readonly router = inject(Router);
  private readonly signalRService = inject(SignalRService);

  readonly icons = { Star, Clock, MessageCircle, BookOpen, Trophy, Bell };

  readonly notifications = signal<NotificationDto[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly activeTab = signal<FilterTab>('all');
  readonly activeTypeFilter = signal<NotificationType | null>(null);

  readonly typeFilters = [
    { label: 'Все типы', value: null },
    { label: 'Оценка', value: NotificationType.Grade },
    { label: 'Дедлайн', value: NotificationType.Deadline },
    { label: 'Сообщение', value: NotificationType.Message },
    { label: 'Курс', value: NotificationType.Course },
    { label: 'Достижение', value: NotificationType.Achievement },
  ];

  readonly unreadCount = computed(() => this.notifications().filter((n) => !n.isRead).length);

  readonly filteredNotifications = computed(() => {
    let items = this.notifications();
    const tab = this.activeTab();
    const typeFilter = this.activeTypeFilter();

    if (tab === 'unread') items = items.filter((n) => !n.isRead);
    else if (tab === 'read') items = items.filter((n) => n.isRead);

    if (typeFilter !== null) items = items.filter((n) => n.type === typeFilter);

    return items;
  });

  readonly NotificationType = NotificationType;

  private readonly liveUpdatesEffect = effect(() => {
    const notification = this.signalRService.lastNotification();
    if (!notification) {
      return;
    }

    this.notifications.update((list) => {
      const next = [notification, ...list.filter((item) => item.id !== notification.id)];
      return next.slice(0, 100);
    });
  });

  ngOnInit(): void {
    this.loadNotifications();
  }

  private loadNotifications(): void {
    this.loading.set(true);
    this.notificationsService.getNotifications({ page: 1, pageSize: 100 }).subscribe({
      next: (result) => {
        this.notifications.set(result.items);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  setTab(tab: FilterTab): void {
    this.activeTab.set(tab);
  }

  setTypeFilter(type: NotificationType | null): void {
    this.activeTypeFilter.set(type);
  }

  onNotificationClick(notification: NotificationDto): void {
    if (!notification.isRead) {
      this.notificationsService.markAsRead(notification.id).subscribe({
        next: () => this.markNotificationAsReadLocally(notification.id),
      });
    }

    if (notification.linkUrl) {
      this.router.navigateByUrl(notification.linkUrl);
    }
  }

  markAllRead(): void {
    this.notificationsService.markAllAsRead().subscribe({
      next: () => {
        this.notifications.update((list) => list.map((n) => ({ ...n, isRead: true })));
      },
    });
  }

  deleteNotification(id: string, event: Event): void {
    event.stopPropagation();
    const notification = this.notifications().find((item) => item.id === id);

    this.notificationsService.deleteNotification(id).subscribe({
      next: () => {
        this.notifications.update((list) => list.filter((n) => n.id !== id));
        if (notification && !notification.isRead) {
          this.signalRService.decrementUnreadCount();
        }
      },
    });
  }

  private markNotificationAsReadLocally(id: string): void {
    this.notifications.update((list) =>
      list.map((n) => (n.id === id ? { ...n, isRead: true } : n)),
    );
  }

  getTypeIcon(type: NotificationType): LucideIconData {
    switch (type) {
      case NotificationType.Grade: return this.icons.Star;
      case NotificationType.Deadline: return this.icons.Clock;
      case NotificationType.Message: return this.icons.MessageCircle;
      case NotificationType.Course: return this.icons.BookOpen;
      case NotificationType.Achievement: return this.icons.Trophy;
      default: return this.icons.Bell;
    }
  }

  getTypeLabel(type: NotificationType): string {
    switch (type) {
      case NotificationType.Grade: return 'Оценка';
      case NotificationType.Deadline: return 'Дедлайн';
      case NotificationType.Message: return 'Сообщение';
      case NotificationType.Course: return 'Курс';
      case NotificationType.Achievement: return 'Достижение';
      default: return type;
    }
  }

  getTypeBadgeVariant(type: NotificationType): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    switch (type) {
      case NotificationType.Grade: return 'warning';
      case NotificationType.Deadline: return 'danger';
      case NotificationType.Message: return 'primary';
      case NotificationType.Course: return 'primary';
      case NotificationType.Achievement: return 'success';
      default: return 'neutral';
    }
  }

  formatTimeAgo(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    const diffHrs = Math.floor(diffMin / 60);
    const diffDays = Math.floor(diffHrs / 24);

    if (diffMin < 1) return 'только что';
    if (diffMin < 60) return `${diffMin} мин назад`;
    if (diffHrs < 24) return `${diffHrs} ч назад`;
    if (diffDays < 7) return `${diffDays} д назад`;
    return date.toLocaleDateString('ru-RU');
  }
}
