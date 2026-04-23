import { Component, Input } from '@angular/core';
import { BannerBlockData } from '../../models';

@Component({
  selector: 'app-banner-block-viewer',
  standalone: true,
  template: `
    <div
      class="banner"
      [style.backgroundColor]="data.bgColor || '#4F46E5'"
      [style.color]="data.textColor || '#ffffff'"
    >
      <h2>{{ data.title }}</h2>
      @if (data.imageUrl) {
        <img [src]="data.imageUrl" alt="" />
      }
    </div>
  `,
  styles: [
    `
      .banner {
        border-radius: 12px; padding: 32px; display: flex; align-items: center;
        justify-content: space-between; gap: 16px; min-height: 120px;
      }
      .banner h2 { margin: 0; font-size: 1.875rem; font-weight: 700; line-height: 1.2; }
      .banner img { max-height: 120px; border-radius: 8px; }
    `,
  ],
})
export class BannerBlockViewerComponent {
  @Input({ required: true }) data!: BannerBlockData;
}
