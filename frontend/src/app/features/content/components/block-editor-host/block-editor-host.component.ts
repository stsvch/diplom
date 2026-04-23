import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  LessonBlockData,
  LessonBlockDto,
  LessonBlockSettings,
} from '../../models';
import { TextBlockEditorComponent } from '../../blocks/text/text-block-editor.component';
import { VideoBlockEditorComponent } from '../../blocks/video/video-block-editor.component';
import { AudioBlockEditorComponent } from '../../blocks/audio/audio-block-editor.component';
import { ImageBlockEditorComponent } from '../../blocks/image/image-block-editor.component';
import { BannerBlockEditorComponent } from '../../blocks/banner/banner-block-editor.component';
import { FileBlockEditorComponent } from '../../blocks/file/file-block-editor.component';
import { SingleChoiceEditorComponent } from '../../blocks/single-choice/single-choice-editor.component';
import { MultipleChoiceEditorComponent } from '../../blocks/multiple-choice/multiple-choice-editor.component';
import { TrueFalseEditorComponent } from '../../blocks/true-false/true-false-editor.component';
import { FillGapEditorComponent } from '../../blocks/fill-gap/fill-gap-editor.component';
import { DropdownEditorComponent } from '../../blocks/dropdown/dropdown-editor.component';
import { WordBankEditorComponent } from '../../blocks/word-bank/word-bank-editor.component';
import { ReorderEditorComponent } from '../../blocks/reorder/reorder-editor.component';
import { MatchingEditorComponent } from '../../blocks/matching/matching-editor.component';
import { OpenTextEditorComponent } from '../../blocks/open-text/open-text-editor.component';
import { CodeExerciseEditorComponent } from '../../blocks/code-exercise/code-exercise-editor.component';
import { QuizEditorComponent } from '../../blocks/quiz/quiz-editor.component';
import { AssignmentEditorComponent } from '../../blocks/assignment/assignment-editor.component';

@Component({
  selector: 'app-block-editor-host',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TextBlockEditorComponent,
    VideoBlockEditorComponent,
    AudioBlockEditorComponent,
    ImageBlockEditorComponent,
    BannerBlockEditorComponent,
    FileBlockEditorComponent,
    SingleChoiceEditorComponent,
    MultipleChoiceEditorComponent,
    TrueFalseEditorComponent,
    FillGapEditorComponent,
    DropdownEditorComponent,
    WordBankEditorComponent,
    ReorderEditorComponent,
    MatchingEditorComponent,
    OpenTextEditorComponent,
    CodeExerciseEditorComponent,
    QuizEditorComponent,
    AssignmentEditorComponent,
  ],
  template: `
    <div class="editor">
      @switch (block.type) {
        @case ('Text') {
          <app-text-block-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-text-block-editor>
        }
        @case ('Video') {
          <app-video-block-editor [data]="$any(block.data)" [blockId]="block.id" (dataChange)="dataChange.emit($event)"></app-video-block-editor>
        }
        @case ('Audio') {
          <app-audio-block-editor [data]="$any(block.data)" [blockId]="block.id" (dataChange)="dataChange.emit($event)"></app-audio-block-editor>
        }
        @case ('Image') {
          <app-image-block-editor [data]="$any(block.data)" [blockId]="block.id" (dataChange)="dataChange.emit($event)"></app-image-block-editor>
        }
        @case ('Banner') {
          <app-banner-block-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-banner-block-editor>
        }
        @case ('File') {
          <app-file-block-editor [data]="$any(block.data)" [blockId]="block.id" (dataChange)="dataChange.emit($event)"></app-file-block-editor>
        }

        @case ('SingleChoice') {
          <app-single-choice-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-single-choice-editor>
        }
        @case ('MultipleChoice') {
          <app-multiple-choice-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-multiple-choice-editor>
        }
        @case ('TrueFalse') {
          <app-true-false-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-true-false-editor>
        }
        @case ('FillGap') {
          <app-fill-gap-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-fill-gap-editor>
        }
        @case ('Dropdown') {
          <app-dropdown-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-dropdown-editor>
        }
        @case ('WordBank') {
          <app-word-bank-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-word-bank-editor>
        }
        @case ('Reorder') {
          <app-reorder-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-reorder-editor>
        }
        @case ('Matching') {
          <app-matching-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-matching-editor>
        }

        @case ('OpenText') {
          <app-open-text-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-open-text-editor>
        }
        @case ('CodeExercise') {
          <app-code-exercise-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-code-exercise-editor>
        }

        @case ('Quiz') {
          <app-quiz-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-quiz-editor>
        }
        @case ('Assignment') {
          <app-assignment-editor [data]="$any(block.data)" (dataChange)="dataChange.emit($event)"></app-assignment-editor>
        }
      }

      <button type="button" class="settings-toggle" (click)="showSettings = !showSettings">
        {{ showSettings ? '▾' : '▸' }} Дополнительно
      </button>

      @if (showSettings) {
        <div class="settings">
          <div class="settings__row">
            <label class="field">
              <span>Баллы</span>
              <input class="input" type="number" step="0.5" [ngModel]="block.settings.points" (ngModelChange)="onPointsChange($event)" />
            </label>
            <label class="field">
              <span>Попыток max</span>
              <input class="input" type="number" min="1" placeholder="∞"
                [ngModel]="block.settings.maxAttempts"
                (ngModelChange)="onMaxAttemptsChange($event)"
              />
            </label>
            <label class="checkbox">
              <input type="checkbox"
                [ngModel]="block.settings.requiredForCompletion"
                (ngModelChange)="onRequiredChange($event)"
              />
              <span>Обязательный</span>
            </label>
            <label class="checkbox">
              <input type="checkbox"
                [ngModel]="block.settings.showFeedback"
                (ngModelChange)="onShowFeedbackChange($event)"
              />
              <span>Показывать фидбэк</span>
            </label>
          </div>
          <input class="input" type="text" placeholder="Подсказка"
            [ngModel]="block.settings.hint"
            (ngModelChange)="onHintChange($event)"
          />
        </div>
      }
    </div>
  `,
  styles: [
    `
      .editor { display: flex; flex-direction: column; gap: 12px; }
      .settings-toggle {
        align-self: flex-start; border: none; background: transparent;
        color: #64748B; font-size: 0.875rem; cursor: pointer; padding: 4px 0;
      }
      .settings-toggle:hover { color: #4F46E5; }
      .settings {
        display: flex; flex-direction: column; gap: 12px;
        padding: 12px; background: #F1F5F9; border-radius: 10px;
      }
      .settings__row { display: flex; gap: 16px; flex-wrap: wrap; align-items: end; }
      .field { display: flex; flex-direction: column; gap: 4px; font-size: 0.75rem; color: #64748B; }
      .input {
        padding: 6px 10px; border: 1px solid #E2E8F0; border-radius: 6px;
        background: #fff; font: inherit; font-size: 0.875rem; outline: none;
        min-width: 100px;
      }
      .input:focus { border-color: #4F46E5; }
      .checkbox { display: inline-flex; align-items: center; gap: 6px; font-size: 0.875rem; }
    `,
  ],
})
export class BlockEditorHostComponent {
  @Input({ required: true }) block!: LessonBlockDto;

  @Output() dataChange = new EventEmitter<LessonBlockData>();
  @Output() settingsChange = new EventEmitter<LessonBlockSettings>();

  showSettings = false;

  private emitSettings() {
    this.settingsChange.emit({ ...this.block.settings });
  }

  onPointsChange(v: string | number) {
    const n = typeof v === 'number' ? v : parseFloat(v);
    if (!isNaN(n)) {
      this.block.settings.points = n;
      this.emitSettings();
    }
  }

  onRequiredChange(v: boolean) {
    this.block.settings.requiredForCompletion = v;
    this.emitSettings();
  }

  onShowFeedbackChange(v: boolean) {
    this.block.settings.showFeedback = v;
    this.emitSettings();
  }

  onHintChange(v: string) {
    this.block.settings.hint = v || undefined;
    this.emitSettings();
  }

  onMaxAttemptsChange(v: string) {
    const n = parseInt(v, 10);
    this.block.settings.maxAttempts = isNaN(n) || n <= 0 ? undefined : n;
    this.emitSettings();
  }
}
