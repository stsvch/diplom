import { Component, computed, inject } from '@angular/core';
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
export class SidebarComponent {
  private authService = inject(AuthService);
  private sidebarService = inject(SidebarService);

  readonly collapsed = this.sidebarService.collapsed;
  readonly currentUser = this.authService.currentUser;
  readonly userRole = this.authService.userRole;

  readonly icons = {
    GraduationCap,
    ChevronLeft,
    ChevronRight,
  };

  readonly studentNav: NavItem[] = [
    { label: 'Дашборд', route: '/student/dashboard', icon: LayoutDashboard },
    { label: 'Мои курсы', route: '/student/courses', icon: BookOpen },
    { label: 'Каталог курсов', route: '/student/catalog', icon: Library },
    { label: 'Календарь', route: '/student/calendar', icon: Calendar },
    { label: 'Сообщения', route: '/student/messages', icon: MessageSquare, badge: 3 },
    { label: 'Уведомления', route: '/student/notifications', icon: Bell, badge: 5 },
    { label: 'Подписки и платежи', route: '/student/payments', icon: CreditCard },
    { label: 'Профиль', route: '/student/profile', icon: User },
  ];

  readonly teacherNav: NavItem[] = [
    { label: 'Дашборд', route: '/teacher/dashboard', icon: LayoutDashboard },
    { label: 'Мои курсы', route: '/teacher/courses', icon: BookOpen },
    { label: 'Создать курс', route: '/teacher/courses/create', icon: PlusSquare },
    { label: 'Проверка работ', route: '/teacher/assignments', icon: ClipboardCheck, badge: 8 },
    { label: 'Журнал оценок', route: '/teacher/gradebook', icon: BookMarked },
    { label: 'Расписание', route: '/teacher/schedule', icon: CalendarDays },
    { label: 'Отчёты', route: '/teacher/reports', icon: BarChart2 },
    { label: 'Сообщения', route: '/teacher/messages', icon: MessageSquare, badge: 2 },
    { label: 'Уведомления', route: '/teacher/notifications', icon: Bell },
    { label: 'Словарь', route: '/teacher/glossary', icon: Globe },
    { label: 'Профиль', route: '/teacher/profile', icon: User },
  ];

  readonly adminNav: NavItem[] = [
    { label: 'Дашборд', route: '/admin/dashboard', icon: LayoutDashboard },
    { label: 'Пользователи', route: '/admin/users', icon: Users },
    { label: 'Курсы', route: '/admin/courses', icon: BookCopy },
    { label: 'Дисциплины', route: '/admin/disciplines', icon: BookOpen },
    { label: 'Платежи', route: '/admin/payments', icon: DollarSign },
    { label: 'Аналитика', route: '/admin/analytics', icon: BarChart2 },
    { label: 'Настройки', route: '/admin/settings', icon: Settings },
  ];

  readonly navItems = computed<NavItem[]>(() => {
    const role = this.userRole();
    if (role === UserRole.Teacher) return this.teacherNav;
    if (role === UserRole.Admin) return this.adminNav;
    return this.studentNav;
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

  toggle(): void {
    this.sidebarService.toggle();
  }
}
