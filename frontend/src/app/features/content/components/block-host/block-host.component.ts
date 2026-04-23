import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { LucideAngularModule } from 'lucide-angular';
import {
  LessonBlockType,
  isAutoGraded,
  isComposite,
  isInformational,
  isManualGraded,
  BLOCK_TYPE_LABELS,
  BLOCK_TYPE_ICONS,
} from '../../models';

type BlockCategory = 'informational' | 'auto' | 'manual' | 'composite';

@Component({
  selector: 'app-block-host',
  standalone: true,
  imports: [CommonModule, LucideAngularModule],
  templateUrl: './block-host.component.html',
  styleUrl: './block-host.component.scss',
})
export class BlockHostComponent {
  @Input({ required: true }) type!: LessonBlockType;
  @Input() readonly = false;
  @Input() hasError = false;

  @Output() duplicate = new EventEmitter<void>();
  @Output() remove = new EventEmitter<void>();

  get label(): string {
    return BLOCK_TYPE_LABELS[this.type];
  }

  get icon(): string {
    return BLOCK_TYPE_ICONS[this.type];
  }

  get category(): BlockCategory {
    if (isInformational(this.type)) return 'informational';
    if (isAutoGraded(this.type)) return 'auto';
    if (isManualGraded(this.type)) return 'manual';
    if (isComposite(this.type)) return 'composite';
    return 'informational';
  }
}
