import { Component, Input } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, Star, Clock, BookOpen, Users, Zap } from 'lucide-angular';
import { CourseListDto } from '../../../features/courses/models/course.model';
import { BadgeComponent } from '../badge/badge.component';
import { ProgressBarComponent } from '../progress-bar/progress-bar.component';
import { DurationPipe } from '../../pipes/duration.pipe';

@Component({
  selector: 'app-course-card',
  standalone: true,
  imports: [RouterLink, LucideAngularModule, BadgeComponent, ProgressBarComponent, DurationPipe, DecimalPipe],
  templateUrl: './course-card.component.html',
  styleUrl: './course-card.component.scss',
})
export class CourseCardComponent {
  @Input() course!: CourseListDto;
  @Input() showProgress = false;
  @Input() linkPrefix = '/student/course';

  readonly StarIcon = Star;
  readonly ClockIcon = Clock;
  readonly BookOpenIcon = BookOpen;
  readonly UsersIcon = Users;
  readonly ZapIcon = Zap;

  get levelLabel(): string {
    const map: Record<string, string> = {
      Beginner: 'Начальный',
      Intermediate: 'Средний',
      Advanced: 'Продвинутый',
    };
    return map[this.course.level] ?? this.course.level;
  }

  get levelVariant(): 'primary' | 'success' | 'warning' | 'danger' | 'neutral' {
    const map: Record<string, 'primary' | 'success' | 'warning' | 'danger' | 'neutral'> = {
      Beginner: 'success',
      Intermediate: 'warning',
      Advanced: 'danger',
    };
    return map[this.course.level] ?? 'neutral';
  }

  get priceLabel(): string {
    if (this.course.isFree) return 'Бесплатно';
    if (this.course.price) return `${this.course.price} ₽`;
    return 'Бесплатно';
  }

  get tags(): string[] {
    if (!this.course.tags) return [];
    return this.course.tags.split(',').map((t) => t.trim()).filter(Boolean).slice(0, 3);
  }

  get courseLink(): string {
    return `${this.linkPrefix}/${this.course.id}`;
  }
}
