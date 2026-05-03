import { Component, Input, computed, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  BookOpen,
  ChevronRight,
  Clock,
  LucideAngularModule,
  Star,
  Users,
} from 'lucide-angular';
import { CourseListDto } from '../../../features/courses/models/course.model';
import { DurationPipe } from '../../pipes/duration.pipe';

const GRADIENTS = [
  'indigo-purple',
  'teal-cyan',
  'amber-orange',
  'rose-pink',
  'violet-purple',
  'emerald-teal',
];

@Component({
  selector: 'app-course-card',
  standalone: true,
  imports: [RouterLink, LucideAngularModule, DurationPipe, DecimalPipe],
  templateUrl: './course-card.component.html',
  styleUrl: './course-card.component.scss',
})
export class CourseCardComponent {
  private readonly _course = signal<CourseListDto | null>(null);

  @Input() set course(value: CourseListDto) {
    this._course.set(value);
  }
  get course(): CourseListDto {
    return this._course()!;
  }

  @Input() showProgress = false;
  @Input() showCta = false;
  @Input() linkPrefix = '/student/course';

  readonly StarIcon = Star;
  readonly ClockIcon = Clock;
  readonly BookOpenIcon = BookOpen;
  readonly UsersIcon = Users;
  readonly ChevronRightIcon = ChevronRight;

  /** Детерминированный градиент по id курса */
  readonly gradient = computed(() => {
    const c = this._course();
    if (!c) return GRADIENTS[0];
    let hash = 0;
    for (let i = 0; i < c.id.length; i++) {
      hash = (hash * 31 + c.id.charCodeAt(i)) >>> 0;
    }
    return GRADIENTS[hash % GRADIENTS.length];
  });

  readonly teacherInitial = computed(() => {
    const c = this._course();
    return (c?.teacherName ?? '?').trim().charAt(0).toUpperCase() || '?';
  });

  get levelLabel(): string {
    const map: Record<string, string> = {
      Beginner: 'Начальный',
      Intermediate: 'Средний',
      Advanced: 'Продвинутый',
    };
    return map[this.course.level] ?? this.course.level;
  }

  /** Класс для уровня — управляет цветом */
  get levelClass(): string {
    const map: Record<string, string> = {
      Beginner: 'level-beginner',
      Intermediate: 'level-intermediate',
      Advanced: 'level-advanced',
    };
    return map[this.course.level] ?? 'level-default';
  }

  get priceLabel(): string {
    if (this.course.isFree) return 'Бесплатно';
    if (this.course.price) return `${this.course.price.toLocaleString('ru-RU')} ₽`;
    return 'Бесплатно';
  }

  get tags(): string[] {
    if (!this.course.tags) return [];
    return this.course.tags
      .split(',')
      .map((t) => t.trim())
      .filter(Boolean)
      .slice(0, 3);
  }

  get courseLink(): string {
    return `${this.linkPrefix}/${this.course.id}`;
  }
}
