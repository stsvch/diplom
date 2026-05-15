// forgot-password.component.ts
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, BookOpen, Mail, ArrowLeft } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

// Экран восстановления отправляет email на backend и показывает нейтральный успех без раскрытия существования аккаунта.
@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss',
})
export class ForgotPasswordComponent {
  // DI-зависимости создают email-форму, вызывают AuthService и показывают результат через toast.
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  readonly BookOpenIcon = BookOpen;
  readonly MailIcon = Mail;
  readonly ArrowLeftIcon = ArrowLeft;

  // loading блокирует кнопку отправки, sent переключает форму на экран с инструкцией проверить почту.
  readonly loading = signal(false);
  readonly sent = signal(false);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  get emailError(): string {
    const ctrl = this.form.get('email');
    if (ctrl?.touched && ctrl.invalid) {
      if (ctrl.hasError('required')) return 'Email обязателен';
      if (ctrl.hasError('email')) return 'Введите корректный email';
    }
    return '';
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email } = this.form.value;
    this.loading.set(true);
    // При submit отправляется email для восстановления; UI всегда показывает нейтральный успешный сценарий, если backend принял запрос.
    this.authService.forgotPassword(email!).subscribe({
      next: () => {
        this.loading.set(false);
        this.sent.set(true);
        this.toastService.success('Ссылка для сброса пароля отправлена на ваш email');
      },
      error: (err: { message: string }) => {
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }
}
