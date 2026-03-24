import { Component, Input, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { inject } from '@angular/core';
import { LucideAngularModule, Play } from 'lucide-angular';

type VideoType = 'youtube' | 'vimeo' | 'direct';

@Component({
  selector: 'app-video-player',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './video-player.component.html',
  styleUrl: './video-player.component.scss',
})
export class VideoPlayerComponent {
  private readonly sanitizer = inject(DomSanitizer);

  @Input() set url(value: string) {
    this._url.set(value ?? '');
  }

  readonly PlayIcon = Play;

  readonly _url = signal('');

  readonly videoType = computed<VideoType>(() => {
    const u = this._url();
    if (!u) return 'direct';
    if (u.includes('youtube.com') || u.includes('youtu.be')) return 'youtube';
    if (u.includes('vimeo.com')) return 'vimeo';
    return 'direct';
  });

  readonly embedUrl = computed<SafeResourceUrl | null>(() => {
    const u = this._url();
    if (!u) return null;
    const type = this.videoType();

    if (type === 'youtube') {
      const id = this.extractYouTubeId(u);
      if (!id) return null;
      return this.sanitizer.bypassSecurityTrustResourceUrl(
        `https://www.youtube.com/embed/${id}?rel=0&modestbranding=1`,
      );
    }

    if (type === 'vimeo') {
      const id = this.extractVimeoId(u);
      if (!id) return null;
      return this.sanitizer.bypassSecurityTrustResourceUrl(
        `https://player.vimeo.com/video/${id}`,
      );
    }

    return null;
  });

  readonly directUrl = computed<string>(() => {
    return this.videoType() === 'direct' ? this._url() : '';
  });

  private extractYouTubeId(url: string): string | null {
    const patterns = [
      /(?:youtube\.com\/watch\?v=)([^&\s]+)/,
      /(?:youtu\.be\/)([^?\s]+)/,
      /(?:youtube\.com\/embed\/)([^?\s]+)/,
    ];
    for (const pattern of patterns) {
      const match = url.match(pattern);
      if (match) return match[1];
    }
    return null;
  }

  private extractVimeoId(url: string): string | null {
    const match = url.match(/vimeo\.com\/(?:video\/)?(\d+)/);
    return match ? match[1] : null;
  }
}
