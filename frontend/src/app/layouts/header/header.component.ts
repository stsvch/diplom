import {
  Component,
  computed,
  effect,
  inject,
  signal,
  HostListener,
  ElementRef,
  OnInit,
} from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { LucideAngularModule, LucideIconData } from 'lucide-angular';
import {
  Bell,
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
  Menu,
} from 'lucide-angular';
import { AuthService } from '../../core/services/auth.service';
import { SidebarService } from '../../core/services/sidebar.service';
import { SignalRService } from '../../core/services/signalr.service';
import { NotificationsService } from '../../features/notifications/services/notifications.service';
import { NotificationDto, NotificationType } from '../../features/notifications/models/notification.model';
import { UserRole } from '../../core/models/user.model';

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
    Menu,
  };

  readonly currentUser = this.authService.currentUser;

  notificationsOpen = signal(false);
  profileOpen = signal(false);

  readonly recentNotifications = signal<NotificationDto[]>([]);
  readonly unreadCount = this.signalRService.unreadCount;

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

  readonly userRoleKey = computed<'student' | 'teacher' | 'admin'>(() => {
    switch (this.authService.userRole()) {
      case UserRole.Teacher: return 'teacher';
      case UserRole.Admin: return 'admin';
      default: return 'student';
    }
  });

  readonly userRoleLabel = computed(() => {
    switch (this.userRoleKey()) {
      case 'teacher': return 'Преподаватель';
      case 'admin': return 'Администратор';
      default: return 'Студент';
    }
  });

  readonly profileRoute = computed(() => {
    if (this.userRoleKey() === 'admin') return '/admin/settings';
    return `/${this.userRoleKey()}/profile`;
  });

  private readonly liveUpdatesEffect = effect(() => {
    const notification = this.signalRService.lastNotification();
    if (!notification) return;

    this.recentNotifications.update((list) => {
      const next = [notification, ...list.filter((item) => item.id !== notification.id)];
      return next.slice(0, 5);
    });
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
    if (this.notificationsOpen()) this.loadRecentNotifications();
  }

  toggleProfile(): void {
    this.profileOpen.update((v) => !v);
    this.notificationsOpen.set(false);
  }

  openMobileSidebar(): void {
    this.sidebarService.openMobile();
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
    const notification = this.recentNotifications().find((item) => item.id === id);

    this.notificationsService.deleteNotification(id).subscribe({
      next: () => {
        this.recentNotifications.update((list) => list.filter((n) => n.id !== id));
        if (notification && !notification.isRead) {
          this.signalRService.decrementUnreadCount();
        }
      },
    });
  }

  openNotification(notification: NotificationDto): void {
    const navigate = () => {
      this.notificationsOpen.set(false);

      if (notification.linkUrl) {
        this.router.navigateByUrl(notification.linkUrl);
        return;
      }

      this.navigateToNotifications();
    };

    if (notification.isRead) {
      navigate();
      return;
    }

    this.notificationsService.markAsRead(notification.id).subscribe({
      next: () => {
        this.recentNotifications.update((list) =>
          list.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item)),
        );
        navigate();
      },
      error: () => navigate(),
    });
  }

  navigateToNotifications(): void {
    this.notificationsOpen.set(false);
    const role = this.authService.userRole();
    const prefix = role === 'Teacher' ? 'teacher' : role === 'Admin' ? 'admin' : 'student';
    this.router.navigate([`/${prefix}/notifications`]);
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
