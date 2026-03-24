import {
  Component,
  computed,
  inject,
  signal,
  HostListener,
  ElementRef,
} from '@angular/core';
import { RouterLink, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { LucideAngularModule } from 'lucide-angular';
import {
  Bell,
  Search,
  ChevronDown,
  LogOut,
  UserCircle,
  Settings,
  X,
  Info,
  CheckCircle,
  AlertTriangle,
} from 'lucide-angular';
import { AuthService } from '../../core/services/auth.service';
import { SidebarService } from '../../core/services/sidebar.service';
import { toSignal } from '@angular/core/rxjs-interop';

interface Notification {
  id: number;
  title: string;
  message: string;
  time: string;
  read: boolean;
  type: 'info' | 'success' | 'warning';
}

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss',
})
export class HeaderComponent {
  private authService = inject(AuthService);
  private sidebarService = inject(SidebarService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  readonly icons = {
    Bell,
    Search,
    ChevronDown,
    LogOut,
    UserCircle,
    Settings,
    X,
    Info,
    CheckCircle,
    AlertTriangle,
  };

  readonly currentUser = this.authService.currentUser;
  readonly collapsed = this.sidebarService.collapsed;

  notificationsOpen = signal(false);
  profileOpen = signal(false);

  readonly notifications = signal<Notification[]>([
    {
      id: 1,
      title: 'Новое задание',
      message: 'Преподаватель добавил новое задание по курсу «Angular»',
      time: '5 мин назад',
      read: false,
      type: 'info',
    },
    {
      id: 2,
      title: 'Оценка выставлена',
      message: 'Ваша работа проверена. Оценка: 95/100',
      time: '1 час назад',
      read: false,
      type: 'success',
    },
    {
      id: 3,
      title: 'Напоминание',
      message: 'Дедлайн по курсу «TypeScript» через 2 дня',
      time: '3 часа назад',
      read: true,
      type: 'warning',
    },
  ]);

  readonly unreadCount = computed(
    () => this.notifications().filter((n) => !n.read).length,
  );

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

  toggleNotifications(): void {
    this.notificationsOpen.update((v) => !v);
    this.profileOpen.set(false);
  }

  toggleProfile(): void {
    this.profileOpen.update((v) => !v);
    this.notificationsOpen.set(false);
  }

  markAllRead(): void {
    this.notifications.update((list) => list.map((n) => ({ ...n, read: true })));
  }

  dismissNotification(id: number): void {
    this.notifications.update((list) => list.filter((n) => n.id !== id));
  }

  logout(): void {
    this.authService.logout();
    this.profileOpen.set(false);
    this.router.navigate(['/login']);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.notificationsOpen.set(false);
      this.profileOpen.set(false);
    }
  }
}
