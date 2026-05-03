import { Component, OnInit, OnDestroy, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { CourseBuilderStore } from './state/course-builder.store';
import { TopBarComponent } from './components/top-bar/top-bar.component';
import { StructurePanelComponent } from './components/structure-panel/structure-panel.component';
import { EditorPanelComponent } from './components/editor-panel/editor-panel.component';
import { HintsPanelComponent } from './components/hints-panel/hints-panel.component';
import { OnboardingModalComponent } from './components/onboarding-modal/onboarding-modal.component';

@Component({
  selector: 'app-course-builder',
  standalone: true,
  imports: [
    TopBarComponent,
    StructurePanelComponent,
    EditorPanelComponent,
    HintsPanelComponent,
    OnboardingModalComponent,
  ],
  providers: [CourseBuilderStore],
  templateUrl: './course-builder.component.html',
  styleUrl: './course-builder.component.scss',
})
export class CourseBuilderComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  readonly store = inject(CourseBuilderStore);

  private readonly destroy$ = new Subject<void>();

  readonly mobileLeftOpen = signal(false);
  readonly mobileRightOpen = signal(false);

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.destroy$)).subscribe((p) => {
      const id = p.get('id');
      if (!id) {
        this.router.navigate(['/teacher/courses']);
        return;
      }
      this.store.load(id);

      // Показываем onboarding при первом входе
      const seen = localStorage.getItem('cb-onboarding-shown');
      if (!seen) {
        setTimeout(() => this.store.showOnboarding.set(true), 600);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleLeft(): void {
    this.mobileLeftOpen.update((v) => !v);
    if (this.mobileLeftOpen()) this.mobileRightOpen.set(false);
  }

  toggleRight(): void {
    this.mobileRightOpen.update((v) => !v);
    if (this.mobileRightOpen()) this.mobileLeftOpen.set(false);
  }

  onOnboardingClose(): void {
    localStorage.setItem('cb-onboarding-shown', '1');
    this.store.showOnboarding.set(false);
  }
}
