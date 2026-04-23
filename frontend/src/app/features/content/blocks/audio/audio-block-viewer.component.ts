import { Component, Input } from '@angular/core';
import { AudioBlockData } from '../../models';

@Component({
  selector: 'app-audio-block-viewer',
  standalone: true,
  template: `
    @if (data.url) {
      <audio controls [src]="data.url" class="audio"></audio>
    } @else {
      <div class="placeholder">Аудио не загружено</div>
    }
    @if (data.transcript) {
      <details class="transcript">
        <summary>Транскрипт</summary>
        <p>{{ data.transcript }}</p>
      </details>
    }
  `,
  styles: [
    `
      :host { display: flex; flex-direction: column; gap: 12px; }
      .audio { width: 100%; }
      .placeholder { padding: 24px; background: #F1F5F9; border-radius: 8px; color: #64748B; text-align: center; }
      .transcript { font-size: 0.875rem; color: #475569; }
      .transcript summary { cursor: pointer; color: #4F46E5; }
      .transcript p { margin: 8px 0 0; white-space: pre-wrap; }
    `,
  ],
})
export class AudioBlockViewerComponent {
  @Input({ required: true }) data!: AudioBlockData;
}
