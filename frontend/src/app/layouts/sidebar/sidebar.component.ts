import { Component, computed, inject, signal, OnInit, effect, HostListener } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import {
  GraduationCap,
  LayoutDashboard,
  BookOpen,
  Library,
  Calendar,
  MessageSquare,
  Bell,
  CreditCard,
  User,
  PlusSquare,
  ClipboardCheck,
  BookMarked,
  CalendarDays,
  BarChart2,
  Users,
  BookCopy,
  DollarSign,
  Settings,
  Globe,
  Home,
  LogOut,
  Menu,
  X,
  LucideAngularModule,
} from 'lucide-angular';
import { AuthService } from '../../core/services/auth.service';
import { SidebarService } from '../../core/services/sidebar.service';
import { UserRole } from '../../core/models/user.model';
import { NotificationsService } from '../../features/notifications/services/notifications.service';
import { MessagingService } from '../../features/messaging/services/messaging.service';
import { AssignmentsService } from '../../features/assignments/services/assignments.service';
import { SignalRService } from '../../core/services/signalr.service';
import { ChatSignalRService } from '../../core/services/chat-signalr.service';

export interface NavItem {
  label: string;
  route: string;
  icon: any;
  badge?: number;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, LucideAngularModule],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent implements OnInit {
  private authService = inject(AuthService);
  private sidebarService = inject(SidebarService);
  private notificationsService = inject(NotificationsService);
  private messagingService = inject(MessagingService);
  private assignmentsService = inject(AssignmentsService);
  private signalRService = inject(SignalRService);
  private chatSignalRService = inject(ChatSignalRService);

  readonly mobileOpen = this.sidebarService.mobileOpen;
  readonly currentUser = this.authService.currentUser;
  readonly userRole = this.authService.userRole;

  readonly icons = {
    GraduationCap,
    Home,
    Settings,
    LogOut,
    X,
    Menu,
  };

  readonly unreadNotifications = this.signalRService.unreadCount;
  readonly unreadMessages = this.chatSignalRService.unreadCount;
  pendingAssignments = signal(0);

  readonly roleKey = computed<'student' | 'teacher' | 'admin'>(() => {
    const role = this.userRole();
    if (role === UserRole.Teacher) return 'teacher';
    if (role === UserRole.Admin) return 'admin';
    return 'student';
  });

  readonly roleLabel = computed(() => {
    switch (this.roleKey()) {
      case 'teacher': return 'TEACHER';
      case 'admin': return 'ADMIN';
      default: return 'STUDENT';
    }
  });

  readonly navItems = computed<NavItem[]>(() => {
    const role = this.userRole();
    const notif = this.unreadNotifications();
    const msg = this.unreadMessages();
    const pending = this.pendingAssignments();

    if (role === UserRole.Teacher) {
      return [
        { label: 'Дашборд', route: '/teacher/dashboard', icon: LayoutDashboard },
        { label: 'Мои курсы', route: '/teacher/courses', icon: BookOpen },
        { label: 'Создать курс', route: '/teacher/courses/new', icon: PlusSquare },
        { label: 'Проверка работ', route: '/teacher/assignments', icon: ClipboardCheck, badge: pending || undefined },
        { label: 'Журнал оценок', route: '/teacher/gradebook', icon: BookMarked },
        { label: 'Календарь', route: '/teacher/calendar', icon: Calendar },
        { label: 'Расписание', route: '/teacher/schedule', icon: CalendarDays },
        { label: 'Выплаты', route: '/teacher/payments', icon: DollarSign },
        { label: 'Отчёты', route: '/teacher/reports', icon: BarChart2 },
        { label: 'Сообщения', route: '/teacher/messages', icon: MessageSquare, badge: msg || undefined },
        { label: 'Уведомления', route: '/teacher/notifications', icon: Bell, badge: notif || undefined },
        { label: 'Словарь', route: '/teacher/glossary', icon: Globe },
      ];
    }
    if (role === UserRole.Admin) {
      return [
        { label: 'Дашборд', route: '/admin/dashboard', icon: LayoutDashboard },
        { label: 'Пользователи', route: '/admin/users', icon: Users },
        { label: 'Курсы', route: '/admin/courses', icon: BookCopy },
        { label: 'Дисциплины', route: '/admin/disciplines', icon: BookOpen },
        { label: 'Платежи', route: '/admin/payments', icon: DollarSign },
        { label: 'Аналитика', route: '/admin/analytics', icon: BarChart2 },
      ];
    }
    return [
      { label: 'Дашборд', route: '/student/dashboard', icon: LayoutDashboard },
      { label: 'Мои курсы', route: '/student/courses', icon: BookOpen },
      { label: 'Каталог курсов', route: '/student/catalog', icon: Library },
      { label: 'Календарь', route: '/student/calendar', icon: Calendar },
      { label: 'Сообщения', route: '/student/messages', icon: MessageSquare, badge: msg || undefined },
      { label: 'Уведомления', route: '/student/notifications', icon: Bell, badge: notif || undefined },
      { label: 'Словарь', route: '/student/glossary', icon: Globe },
      { label: 'Платежи', route: '/student/payments', icon: CreditCard },
    ];
  });

  /** Маршрут к настройкам (admin → settings, остальные → profile) */
  readonly settingsRoute = computed(() => {
    if (this.roleKey() === 'admin') return '/admin/settings';
    return `/${this.roleKey()}/profile`;
  });

  constructor() {
    effect(() => {
      const user = this.currentUser();
      if (!user) return;
      this.loadCounters(user.role);
    });
  }

  ngOnInit(): void {}

  private loadCounters(role: UserRole): void {
    this.notificationsService.getUnreadCount().subscribe({
      error: () => this.signalRService.setUnreadCount(0),
    });

    this.messagingService.getUnreadCount().subscribe({
      error: () => this.chatSignalRService.setUnreadCount(0),
    });

    if (role === UserRole.Teacher) {
      this.assignmentsService.getPendingSubmissions().subscribe({
        next: (list) => this.pendingAssignments.set(list.length),
        error: () => this.pendingAssignments.set(0),
      });
    }
  }

  closeMobile(): void {
    this.sidebarService.closeMobile();
  }

  logout(): void {
    this.authService.logout();
  }

  @HostListener('window:resize')
  onResize(): void {
    if (window.innerWidth >= 1024 && this.mobileOpen()) {
      this.sidebarService.closeMobile();
    }
  }
}
