import { Component, Input, inject } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { VideoBlockData } from '../../models';

@Component({
  selector: 'app-video-block-viewer',
  standalone: true,
  template: `
    @if (getEmbedUrl(); as embed) {
      <div class="frame">
        <iframe
          [src]="embed"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowfullscreen
        ></iframe>
      </div>
    } @else if (data.url) {
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
      .frame { position: relative; aspect-ratio: 16 / 9; border-radius: 12px; overflow: hidden; background: #000; }
      .frame iframe { position: absolute; inset: 0; width: 100%; height: 100%; border: 0; }
      .video { width: 100%; max-height: 480px; border-radius: 12px; background: #000; }
      .placeholder { padding: 24px; background: #F1F5F9; border-radius: 8px; color: #64748B; text-align: center; }
      .caption { margin-top: 8px; font-size: 0.875rem; color: #64748B; }
    `,
  ],
})
export class VideoBlockViewerComponent {
  private readonly sanitizer = inject(DomSanitizer);

  @Input({ required: true }) data!: VideoBlockData;

  getEmbedUrl(): SafeResourceUrl | null {
    const url = this.data?.url;
    if (!url) return null;
    let raw: string | null = null;
    const yt = url.match(/(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&?\s]+)/);
    if (yt) raw = `https://www.youtube.com/embed/${yt[1]}`;
    else {
      const vimeo = url.match(/vimeo\.com\/(\d+)/);
      if (vimeo) raw = `https://player.vimeo.com/video/${vimeo[1]}`;
    }
    return raw ? this.sanitizer.bypassSecurityTrustResourceUrl(raw) : null;
  }
}
