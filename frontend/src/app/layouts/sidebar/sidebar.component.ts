import { Component, computed, inject, signal, OnInit, effect } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule } from 'lucide-angular';
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
  ChevronLeft,
  ChevronRight,
  Globe,
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

  readonly collapsed = this.sidebarService.collapsed;
  readonly currentUser = this.authService.currentUser;
  readonly userRole = this.authService.userRole;

  readonly icons = { GraduationCap, ChevronLeft, ChevronRight };

  readonly unreadNotifications = this.signalRService.unreadCount;
  readonly unreadMessages = this.chatSignalRService.unreadCount;
  pendingAssignments = signal(0);

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
        { label: 'Отчёты', route: '/teacher/reports', icon: BarChart2 },
        { label: 'Сообщения', route: '/teacher/messages', icon: MessageSquare, badge: msg || undefined },
        { label: 'Уведомления', route: '/teacher/notifications', icon: Bell, badge: notif || undefined },
        { label: 'Словарь', route: '/teacher/glossary', icon: Globe },
        { label: 'Профиль', route: '/teacher/profile', icon: User },
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
        { label: 'Настройки', route: '/admin/settings', icon: Settings },
      ];
    }
    return [
      { label: 'Дашборд', route: '/student/dashboard', icon: LayoutDashboard },
      { label: 'Мои курсы', route: '/student/courses', icon: BookOpen },
      { label: 'Каталог курсов', route: '/student/catalog', icon: Library },
      { label: 'Календарь', route: '/student/calendar', icon: Calendar },
      { label: 'Сообщения', route: '/student/messages', icon: MessageSquare, badge: msg || undefined },
      { label: 'Уведомления', route: '/student/notifications', icon: Bell, badge: notif || undefined },
      { label: 'Подписки и платежи', route: '/student/payments', icon: CreditCard },
      { label: 'Профиль', route: '/student/profile', icon: User },
    ];
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

  readonly userRoleLabel = computed(() => {
    const role = this.userRole();
    if (role === UserRole.Admin) return 'Администратор';
    if (role === UserRole.Teacher) return 'Преподаватель';
    return 'Студент';
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

  toggle(): void {
    this.sidebarService.toggle();
  }
}
