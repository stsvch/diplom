import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import {
  LessonBlockAnswer,
  LessonBlockAttemptDto,
  LessonBlockDto,
} from '../../models';
import { TextBlockViewerComponent } from '../../blocks/text/text-block-viewer.component';
import { VideoBlockViewerComponent } from '../../blocks/video/video-block-viewer.component';
import { AudioBlockViewerComponent } from '../../blocks/audio/audio-block-viewer.component';
import { ImageBlockViewerComponent } from '../../blocks/image/image-block-viewer.component';
import { BannerBlockViewerComponent } from '../../blocks/banner/banner-block-viewer.component';
import { FileBlockViewerComponent } from '../../blocks/file/file-block-viewer.component';
import { SingleChoiceViewerComponent } from '../../blocks/single-choice/single-choice-viewer.component';
import { MultipleChoiceViewerComponent } from '../../blocks/multiple-choice/multiple-choice-viewer.component';
import { TrueFalseViewerComponent } from '../../blocks/true-false/true-false-viewer.component';
import { FillGapViewerComponent } from '../../blocks/fill-gap/fill-gap-viewer.component';
import { DropdownViewerComponent } from '../../blocks/dropdown/dropdown-viewer.component';
import { WordBankViewerComponent } from '../../blocks/word-bank/word-bank-viewer.component';
import { ReorderViewerComponent } from '../../blocks/reorder/reorder-viewer.component';
import { MatchingViewerComponent } from '../../blocks/matching/matching-viewer.component';
import { OpenTextViewerComponent } from '../../blocks/open-text/open-text-viewer.component';
import { CodeExerciseViewerComponent } from '../../blocks/code-exercise/code-exercise-viewer.component';
import { QuizViewerComponent } from '../../blocks/quiz/quiz-viewer.component';
import { AssignmentViewerComponent } from '../../blocks/assignment/assignment-viewer.component';

@Component({
  selector: 'app-block-viewer-host',
  standalone: true,
  imports: [
    CommonModule,
    TextBlockViewerComponent,
    VideoBlockViewerComponent,
    AudioBlockViewerComponent,
    ImageBlockViewerComponent,
    BannerBlockViewerComponent,
    FileBlockViewerComponent,
    SingleChoiceViewerComponent,
    MultipleChoiceViewerComponent,
    TrueFalseViewerComponent,
    FillGapViewerComponent,
    DropdownViewerComponent,
    WordBankViewerComponent,
    ReorderViewerComponent,
    MatchingViewerComponent,
    OpenTextViewerComponent,
    CodeExerciseViewerComponent,
    QuizViewerComponent,
    AssignmentViewerComponent,
  ],
  template: `
    @switch (block.type) {
      @case ('Text') { <app-text-block-viewer [data]="$any(block.data)"></app-text-block-viewer> }
      @case ('Video') { <app-video-block-viewer [data]="$any(block.data)"></app-video-block-viewer> }
      @case ('Audio') { <app-audio-block-viewer [data]="$any(block.data)"></app-audio-block-viewer> }
      @case ('Image') { <app-image-block-viewer [data]="$any(block.data)"></app-image-block-viewer> }
      @case ('Banner') { <app-banner-block-viewer [data]="$any(block.data)"></app-banner-block-viewer> }
      @case ('File') { <app-file-block-viewer [data]="$any(block.data)"></app-file-block-viewer> }

      @case ('SingleChoice') {
        <app-single-choice-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-single-choice-viewer>
      }
      @case ('MultipleChoice') {
        <app-multiple-choice-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-multiple-choice-viewer>
      }
      @case ('TrueFalse') {
        <app-true-false-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-true-false-viewer>
      }
      @case ('FillGap') {
        <app-fill-gap-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-fill-gap-viewer>
      }
      @case ('Dropdown') {
        <app-dropdown-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-dropdown-viewer>
      }
      @case ('WordBank') {
        <app-word-bank-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-word-bank-viewer>
      }
      @case ('Reorder') {
        <app-reorder-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-reorder-viewer>
      }
      @case ('Matching') {
        <app-matching-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          [showFeedback]="block.settings.showFeedback ?? true"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-matching-viewer>
      }

      @case ('OpenText') {
        <app-open-text-viewer
          [data]="$any(block.data)"
          [attempt]="attempt"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-open-text-viewer>
      }
      @case ('CodeExercise') {
        <app-code-exercise-viewer
          [data]="$any(block.data)"
          [blockId]="block.id"
          [attempt]="attempt"
          (submitAnswer)="submitAnswer.emit($event)"
        ></app-code-exercise-viewer>
      }

      @case ('Quiz') { <app-quiz-viewer [data]="$any(block.data)"></app-quiz-viewer> }
      @case ('Assignment') { <app-assignment-viewer [data]="$any(block.data)"></app-assignment-viewer> }
    }
  `,
})
export class BlockViewerHostComponent {
  @Input({ required: true }) block!: LessonBlockDto;
  @Input() attempt: LessonBlockAttemptDto | null = null;

  @Output() submitAnswer = new EventEmitter<LessonBlockAnswer>();
}
