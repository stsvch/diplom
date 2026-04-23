import { Component, Input } from '@angular/core';
import { ImageBlockData } from '../../models';

@Component({
  selector: 'app-image-block-viewer',
  standalone: true,
  template: `
    @if (data.url) {
      <figure>
        <img [src]="data.url" [alt]="data.alt || ''" />
        @if (data.caption) {
          <figcaption>{{ data.caption }}</figcaption>
        }
      </figure>
    } @else {
      <div class="placeholder">Изображение не загружено</div>
    }
  `,
  styles: [
    `
      :host { display: block; }
      figure { margin: 0; }
      img { max-width: 100%; border-radius: 12px; display: block; }
      figcaption { margin-top: 8px; font-size: 0.875rem; color: #64748B; text-align: center; }
      .placeholder { padding: 48px; background: #F1F5F9; border-radius: 8px; color: #64748B; text-align: center; }
    `,
  ],
})
export class ImageBlockViewerComponent {
  @Input({ required: true }) data!: ImageBlockData;
}
