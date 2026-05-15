// search-input.component.ts
import { Component, Input, model } from '@angular/core';
import { LucideAngularModule, Search } from 'lucide-angular';

// Компонент связывает шаблон, стили и состояние этого участка интерфейса.
@Component({
  selector: 'app-search-input',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './search-input.component.html',
  styleUrl: './search-input.component.scss',
})
export class SearchInputComponent {
  @Input() placeholder = 'Поиск...';

  // Signals и computed-значения хранят реактивное состояние без ручной синхронизации с шаблоном.
  readonly value = model('');

  readonly SearchIcon = Search;

  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.value.set(target.value);
  }
}
