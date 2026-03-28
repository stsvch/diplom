import {
  Component,
  computed,
  inject,
  signal,
  HostListener,
  ElementRef,
  OnInit,
} from '@angular/core';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { LucideAngularModule, LucideIconData } from 'lucide-angular';
import {
  Bell,
  Search,
  ChevronDown,
  LogOut,
  UserCircle,
  Settings,
  X,
  Star,
  Clock,
  MessageCircle,
  BookOpen,
  Trophy,
} from 'lucide-angular';
import { AuthService } from '../../core/services/auth.service';
import { SidebarService } from '../../core/services/sidebar.service';
import { SignalRService } from '../../core/services/signalr.service';
import { NotificationsService } from '../../features/notifications/services/notifications.service';
import { NotificationDto, NotificationType } from '../../features/notifications/models/notification.model';
import { toSignal } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent implements OnInit {
  private authService = inject(AuthService);
  private sidebarService = inject(SidebarService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);
  private signalRService = inject(SignalRService);
  private notificationsService = inject(NotificationsService);

  readonly icons = {
    Bell,
    Search,
    ChevronDown,
    LogOut,
    UserCircle,
    Settings,
    X,
    Star,
    Clock,
    MessageCircle,
    BookOpen,
    Trophy,
  };

  readonly currentUser = this.authService.currentUser;
  readonly collapsed = this.sidebarService.collapsed;

  notificationsOpen = signal(false);
  profileOpen = signal(false);

  readonly recentNotifications = signal<NotificationDto[]>([]);
  readonly unreadCount = this.signalRService.unreadCount;

  readonly pageTitle = toSignal(
    this.router.events.pipe(filter((e) => e instanceof NavigationEnd)),
    { initialValue: null },
  );

  readonly currentPageTitle = computed(() => {
    this.pageTitle(); // track navigation
    return this.getTitleFromUrl(this.router.url);
  });

  readonly userInitials = computed(() => {
    const user = this.currentUser();
    if (!user) return 'U';
    return `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase();
  });

  readonly userFullName = computed(() => {
    const user = this.currentUser();
    if (!user) return 'Гость';
    return `${user.firstName} ${user.lastName}`;
  });

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.loadUnreadCount();
      this.loadRecentNotifications();
    }
  }

  private loadUnreadCount(): void {
    this.notificationsService.getUnreadCount().subscribe();
  }

  private loadRecentNotifications(): void {
    this.notificationsService.getNotifications({ page: 1, pageSize: 5, isRead: false }).subscribe({
      next: (result) => this.recentNotifications.set(result.items),
    });
  }

  private getTitleFromUrl(url: string): string {
    const segments = url.split('/').filter(Boolean);
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
    };
    return map[last] ?? 'EduPlatform';
  }

  getNotificationIcon(type: NotificationType): LucideIconData {
    switch (type) {
      case NotificationType.Grade: return this.icons.Star;
      case NotificationType.Deadline: return this.icons.Clock;
      case NotificationType.Message: return this.icons.MessageCircle;
      case NotificationType.Course: return this.icons.BookOpen;
      case NotificationType.Achievement: return this.icons.Trophy;
      default: return this.icons.Bell;
    }
  }

  toggleNotifications(): void {
    this.notificationsOpen.update((v) => !v);
    this.profileOpen.set(false);
    if (this.notificationsOpen()) {
      this.loadRecentNotifications();
    }
  }

  toggleProfile(): void {
    this.profileOpen.update((v) => !v);
    this.notificationsOpen.set(false);
  }

  markAllRead(): void {
    this.notificationsService.markAllAsRead().subscribe({
      next: () => {
        this.recentNotifications.update((list) => list.map((n) => ({ ...n, isRead: true })));
      },
    });
  }

  dismissNotification(id: string, event: Event): void {
    event.stopPropagation();
    this.notificationsService.deleteNotification(id).subscribe({
      next: () => {
        this.recentNotifications.update((list) => list.filter((n) => n.id !== id));
      },
    });
  }

  navigateToNotifications(): void {
    this.notificationsOpen.set(false);
    const role = this.authService.userRole();
    const prefix = role === 'Teacher' ? 'teacher' : 'student';
    this.router.navigate([`/${prefix}/notifications`]);
  }

  logout(): void {
    this.authService.logout();
    this.profileOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.notificationsOpen.set(false);
      this.profileOpen.set(false);
    }
  }
}
