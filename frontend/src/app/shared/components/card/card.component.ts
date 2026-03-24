import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [],
  templateUrl: './card.component.html',
  styleUrl: './card.component.scss',
})
export class CardComponent {
  @Input() hoverable = false;
  @Input() padding: 'none' | 'sm' | 'md' | 'lg' = 'md';
}
