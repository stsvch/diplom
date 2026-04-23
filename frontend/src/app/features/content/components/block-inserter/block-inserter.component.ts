import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, signal, HostListener, ElementRef, inject } from '@angular/core';
import { LucideAngularModule } from 'lucide-angular';
import { LessonBlockType } from '../../models';
import { BlockTypeMenuComponent } from '../block-type-menu/block-type-menu.component';

@Component({
  selector: 'app-block-inserter',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, BlockTypeMenuComponent],
  templateUrl: './block-inserter.component.html',
  styleUrl: './block-inserter.component.scss',
})
export class BlockInserterComponent {
  @Output() insert = new EventEmitter<LessonBlockType>();

  private readonly host = inject(ElementRef<HTMLElement>);

  menuOpen = signal(false);

  toggle() {
    this.menuOpen.update((v) => !v);
  }

  close() {
    this.menuOpen.set(false);
  }

  onSelect(type: LessonBlockType) {
    this.menuOpen.set(false);
    this.insert.emit(type);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.menuOpen()) return;
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.close();
    }
  }
}
