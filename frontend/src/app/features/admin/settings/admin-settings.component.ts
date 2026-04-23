import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Save } from 'lucide-angular';
import { AdminService } from '../services/admin.service';
import { PlatformSettingsDto } from '../models/admin.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { parseApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-admin-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, LucideAngularModule],
  template: `
    <div class="settings">
      <h1 class="settings__title">Настройки платформы</h1>

      @if (loading()) {
        <div class="state">Загрузка...</div>
      } @else if (settings(); as s) {
        <div class="settings__form">
          <div class="field">
            <label>Название платформы</label>
            <input type="text" [ngModel]="s.platformName" (ngModelChange)="update('platformName', $event)" />
          </div>
          <div class="field">
            <label>Email поддержки</label>
            <input type="email" [ngModel]="s.supportEmail" (ngModelChange)="update('supportEmail', $event)" />
          </div>
          <div class="field field--switch">
            <label class="switch">
              <input type="checkbox" [ngModel]="s.registrationOpen" (ngModelChange)="update('registrationOpen', $event)" />
              <span>Регистрация открыта</span>
            </label>
            <p class="hint">Если выключить — новые пользователи не смогут зарегистрироваться.</p>
          </div>
          <div class="field field--switch">
            <label class="switch">
              <input type="checkbox" [ngModel]="s.maintenanceMode" (ngModelChange)="update('maintenanceMode', $event)" />
              <span>Режим технического обслуживания</span>
            </label>
            <p class="hint hint--warn">
              В этом режиме все API-запросы (кроме админских) возвращают 503. Будьте осторожны.
            </p>
          </div>

          <div class="settings__actions">
            <button class="btn btn--primary" [disabled]="saving()" (click)="save()">
              <lucide-icon [img]="SaveIcon" size="16"></lucide-icon>
              {{ saving() ? 'Сохраняем…' : 'Сохранить' }}
            </button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host { display: block; }
    .settings { padding: 24px; max-width: 640px; }
    .settings__title { margin: 0 0 20px; font-size: 1.5rem; }
    .settings__form {
      background: #fff; border: 1px solid #E2E8F0; border-radius: 10px; padding: 20px;
    }
    .field { display: flex; flex-direction: column; gap: 6px; margin-bottom: 18px; }
    .field label { font-size: 0.8rem; color: #475569; font-weight: 600; }
    .field input[type=text], .field input[type=email] {
      padding: 9px 12px; border: 1px solid #CBD5E1; border-radius: 6px;
      font-size: 0.9rem; outline: none;
    }
    .field input:focus { border-color: #6366F1; }
    .field--switch { gap: 4px; }
    .switch {
      display: inline-flex; align-items: center; gap: 10px; cursor: pointer;
      font-size: 0.95rem; color: #0F172A; font-weight: 500;
    }
    .switch input { width: 16px; height: 16px; }
    .hint { margin: 0; font-size: 0.8rem; color: #64748B; }
    .hint--warn { color: #92400E; }
    .settings__actions { display: flex; justify-content: flex-end; margin-top: 8px; }
    .btn {
      display: inline-flex; align-items: center; gap: 6px;
      padding: 9px 16px; border-radius: 8px; border: none;
      font-size: 0.9rem; font-weight: 600; cursor: pointer;
    }
    .btn--primary { background: #4F46E5; color: #fff; }
    .btn--primary:hover:not(:disabled) { background: #4338CA; }
    .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .state { padding: 20px; color: #64748B; }
  `],
})
export class AdminSettingsComponent implements OnInit {
  private readonly admin = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly SaveIcon = Save;

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly settings = signal<PlatformSettingsDto | null>(null);

  ngOnInit(): void {
    this.loading.set(true);
    this.admin.getSettings().subscribe({
      next: (s) => { this.settings.set(s); this.loading.set(false); },
      error: (err) => { this.toast.error(parseApiError(err).message); this.loading.set(false); },
    });
  }

  update<K extends keyof PlatformSettingsDto>(key: K, value: PlatformSettingsDto[K]): void {
    const s = this.settings();
    if (!s) return;
    this.settings.set({ ...s, [key]: value });
  }

  save(): void {
    const s = this.settings();
    if (!s) return;
    this.saving.set(true);
    this.admin.updateSettings(s).subscribe({
      next: (updated) => {
        this.settings.set(updated);
        this.saving.set(false);
        this.toast.success('Настройки сохранены');
      },
      error: (err) => {
        this.saving.set(false);
        this.toast.error(parseApiError(err).message);
      },
    });
  }
}
