import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import {
  LucideAngularModule,
  ChevronLeft,
  FilePlus,
  GraduationCap,
  Zap,
  ClipboardCheck,
} from 'lucide-angular';
import { COURSE_TEMPLATES, CourseTemplate } from '../services/course-templates';

@Component({
  selector: 'app-course-new',
  standalone: true,
  imports: [CommonModule, RouterLink, LucideAngularModule],
  template: `
    <div class="page">
      <header class="page__head">
        <a class="back" [routerLink]="['/teacher/courses']">
          <lucide-icon [img]="ChevronLeftIcon" size="18"></lucide-icon>
          К моим курсам
        </a>
        <h1>Создать новый курс</h1>
        <p>Выберите шаблон для быстрого старта или начните с пустого курса</p>
      </header>

      <div class="grid">
        @for (tpl of templates; track tpl.id) {
          <button class="card" type="button" (click)="select(tpl)">
            <div class="card__icon">
              <lucide-icon [img]="iconForTemplate(tpl.icon)" size="24"></lucide-icon>
            </div>
            <h3 class="card__name">{{ tpl.name }}</h3>
            <p class="card__desc">{{ tpl.description }}</p>
            <div class="card__meta">
              @if (tpl.modules.length === 0) {
                <span>— пусто</span>
              } @else {
                <span>{{ tpl.modules.length }} модул(ь/я/ей)</span>
                <span>·</span>
                <span>{{ totalLessons(tpl) }} урок(ов)</span>
              }
            </div>
          </button>
        }
      </div>
    </div>
  `,
  styles: [
    `
      :host { display: block; }
      .page { max-width: 960px; margin: 0 auto; padding: 32px 24px; }
      .page__head { margin-bottom: 32px; }
      .page__head h1 { margin: 16px 0 8px; font-size: 1.875rem; color: #0F172A; }
      .page__head p { margin: 0; color: #64748B; }
      .back {
        display: inline-flex; align-items: center; gap: 4px;
        color: #64748B; text-decoration: none; font-size: 0.875rem;
      }
      .back:hover { color: #4F46E5; }

      .grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
        gap: 16px;
      }

      .card {
        display: flex; flex-direction: column; align-items: flex-start;
        padding: 24px;
        background: #fff;
        border: 2px solid #E2E8F0;
        border-radius: 12px;
        cursor: pointer;
        text-align: left;
        transition: all 0.15s ease;
        font: inherit;
        color: inherit;
      }
      .card:hover {
        border-color: #4F46E5;
        transform: translateY(-2px);
        box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
      }

      .card__icon {
        width: 48px; height: 48px;
        border-radius: 12px;
        background: #EEF2FF;
        color: #4F46E5;
        display: flex; align-items: center; justify-content: center;
        margin-bottom: 16px;
      }
      .card__name { margin: 0 0 8px; font-size: 1.125rem; color: #0F172A; }
      .card__desc { margin: 0 0 16px; font-size: 0.875rem; color: #64748B; line-height: 1.4; }
      .card__meta { display: flex; gap: 6px; font-size: 0.75rem; color: #475569; }
    `,
  ],
})
export class CourseNewComponent {
  private readonly router = inject(Router);
  readonly ChevronLeftIcon = ChevronLeft;
  private readonly templateIcons: Record<string, typeof FilePlus> = {
    'file-plus': FilePlus,
    'graduation-cap': GraduationCap,
    zap: Zap,
    'clipboard-check': ClipboardCheck,
  };

  templates = COURSE_TEMPLATES;

  totalLessons(tpl: CourseTemplate): number {
    return tpl.modules.reduce((sum, m) => sum + m.lessons.length, 0);
  }

  select(tpl: CourseTemplate): void {
    this.router.navigate(['/teacher/courses/create'], { state: { templateId: tpl.id } });
  }

  iconForTemplate(icon: string): typeof FilePlus {
    return this.templateIcons[icon] ?? FilePlus;
  }
}
