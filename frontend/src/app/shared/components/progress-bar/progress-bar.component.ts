import { Component, Input, computed, signal } from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  standalone: true,
  imports: [],
  templateUrl: './progress-bar.component.html',
  styleUrl: './progress-bar.component.scss',
})
export class ProgressBarComponent {
  @Input() set value(v: number) {
    this._value.set(Math.min(100, Math.max(0, v)));
  }
  get value(): number {
    return this._value();
  }
  @Input() showLabel = false;
  @Input() color: 'primary' | 'success' = 'primary';

  private _value = signal(0);
  readonly clampedValue = this._value;

  readonly barColor = computed(() =>
    this._value() === 100 ? 'success' : this.color,
  );
}
