import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, BookOpen, Users, GraduationCap, Star, Eye, EyeOff, Mail, Lock } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  readonly BookOpenIcon = BookOpen;
  readonly UsersIcon = Users;
  readonly GraduationCapIcon = GraduationCap;
  readonly StarIcon = Star;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;
  readonly MailIcon = Mail;
  readonly LockIcon = Lock;

  readonly loading = signal(false);
  readonly showPassword = signal(false);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  get emailError(): string {
    const ctrl = this.form.get('email');
    if (ctrl?.touched && ctrl.invalid) {
      if (ctrl.hasError('required')) return 'Email обязателен';
      if (ctrl.hasError('email')) return 'Введите корректный email';
    }
    return '';
  }

  get passwordError(): string {
    const ctrl = this.form.get('password');
    if (ctrl?.touched && ctrl.invalid) {
      if (ctrl.hasError('required')) return 'Пароль обязателен';
      if (ctrl.hasError('minlength')) return 'Минимум 6 символов';
    }
    return '';
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, password } = this.form.value;
    this.loading.set(true);
    this.authService.login(email!, password!).subscribe({
      next: () => {
        this.loading.set(false);
        this.toastService.success('Добро пожаловать!');
      },
      error: (err: ApiError) => {
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }
}
