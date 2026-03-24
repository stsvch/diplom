import { Component, Input } from '@angular/core';
import { LucideAngularModule, TrendingUp, TrendingDown, Minus } from 'lucide-angular';

export type ChangeType = 'positive' | 'negative' | 'neutral';

@Component({
  selector: 'app-stats-card',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './stats-card.component.html',
  styleUrl: './stats-card.component.scss',
})
export class StatsCardComponent {
  @Input() title = '';
  @Input() value: string | number = '';
  @Input() change = '';
  @Input() changeType: ChangeType = 'neutral';
  @Input() iconName: any = null;
  @Input() iconBg = '#EEF2FF';
  @Input() iconColor = '#4F46E5';
  @Input() subtitle = '';

  readonly TrendingUpIcon = TrendingUp;
  readonly TrendingDownIcon = TrendingDown;
  readonly MinusIcon = Minus;
}
