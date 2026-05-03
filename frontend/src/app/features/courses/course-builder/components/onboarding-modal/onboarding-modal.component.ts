import { Component, Input, Output, EventEmitter, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  BookOpen,
  CheckSquare,
  ChevronLeft,
  ChevronRight,
  Layers,
  LayoutGrid,
  LucideAngularModule,
  Rocket,
  X,
} from 'lucide-angular';

interface Step {
  icon: any;
  iconBg: string;
  iconColor: string;
  title: string;
  description: string;
}

const STEPS: Step[] = [
  {
    icon: BookOpen,
    iconBg: '#e0e7ff',
    iconColor: '#4f46e5',
    title: 'Добро пожаловать в Course Builder',
    description:
      'Это ваш рабочий инструмент для создания онлайн-курсов. Здесь вы соберёте полный учебный маршрут: от первого урока до финального теста.',
  },
  {
    icon: Layers,
    iconBg: '#f3e8ff',
    iconColor: '#9333ea',
    title: 'Структура курса',
    description:
      'Слева — дерево курса. Разделы помогают логически организовать материал. Внутри разделов размещаются элементы: уроки, тесты, задания и live-занятия.',
  },
  {
    icon: LayoutGrid,
    iconBg: '#ffedd5',
    iconColor: '#ea580c',
    title: 'Элементы курса',
    description:
      'В каждый раздел можно добавить разные типы элементов. Каждый тип имеет свой редактор и настройки.',
  },
  {
    icon: CheckSquare,
    iconBg: '#d1fae5',
    iconColor: '#059669',
    title: 'Чеклист готовности',
    description:
      'Справа — чеклист публикации. Он в реальном времени показывает, что готово, что нужно заполнить и какие ошибки блокируют публикацию.',
  },
  {
    icon: Rocket,
    iconBg: '#e0e7ff',
    iconColor: '#4f46e5',
    title: 'Готовы создать курс?',
    description:
      'Заполните информацию о курсе, добавьте разделы и элементы, проверьте чеклист — и опубликуйте курс. Система будет подсказывать на каждом шаге.',
  },
];

@Component({
  selector: 'app-cb-onboarding-modal',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './onboarding-modal.component.html',
  styleUrl: './onboarding-modal.component.scss',
})
export class OnboardingModalComponent {
  @Input() open = false;
  @Output() close = new EventEmitter<void>();

  readonly icons = { x: X, chevLeft: ChevronLeft, chevRight: ChevronRight, rocket: Rocket };
  readonly stepIdx = signal(0);
  readonly steps = STEPS;
  readonly current = computed(() => this.steps[this.stepIdx()]);
  readonly isLast = computed(() => this.stepIdx() === this.steps.length - 1);

  prev(): void {
    this.stepIdx.update((i) => Math.max(0, i - 1));
  }

  next(): void {
    if (this.isLast()) {
      this.finish();
    } else {
      this.stepIdx.update((i) => i + 1);
    }
  }

  goTo(i: number): void {
    this.stepIdx.set(i);
  }

  finish(): void {
    this.stepIdx.set(0);
    this.close.emit();
  }

  skip(): void {
    this.finish();
  }
}
