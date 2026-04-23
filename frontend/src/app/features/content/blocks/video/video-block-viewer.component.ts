import { Component, Input } from '@angular/core';
import { VideoBlockData } from '../../models';

@Component({
  selector: 'app-video-block-viewer',
  standalone: true,
  template: `
    @if (data.url) {
      <video controls [src]="data.url" [poster]="data.posterUrl || null" class="video"></video>
    } @else {
      <div class="placeholder">Видео не загружено</div>
    }
    @if (data.caption) {
      <div class="caption">{{ data.caption }}</div>
    }
  `,
  styles: [
    `
      :host { display: block; }
      .video { width: 100%; max-height: 480px; border-radius: 12px; background: #000; }
      .placeholder { padding: 24px; background: #F1F5F9; border-radius: 8px; color: #64748B; text-align: center; }
      .caption { margin-top: 8px; font-size: 0.875rem; color: #64748B; }
    `,
  ],
})
export class VideoBlockViewerComponent {
  @Input({ required: true }) data!: VideoBlockData;
}
