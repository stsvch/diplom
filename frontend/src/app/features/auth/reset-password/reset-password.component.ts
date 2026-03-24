import { Component, OnInit, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { LucideAngularModule, BookOpen, Lock, Eye, EyeOff, ArrowLeft } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

function passwordMatchValidator(ctrl: AbstractControl): ValidationErrors | null {
  const password = ctrl.get('newPassword')?.value;
  const confirmPassword = ctrl.get('confirmPassword')?.value;
  if (password && confirmPassword && password !== confirmPassword) {
    return { passwordMismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss',
})
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);

  readonly BookOpenIcon = BookOpen;
  readonly LockIcon = Lock;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;
  readonly ArrowLeftIcon = ArrowLeft;

  readonly loading = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly invalidLink = signal(false);

  private email = '';
  private token = '';

  readonly form = this.fb.group(
    {
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator },
  );

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';
    if (!this.email || !this.token) {
      this.invalidLink.set(true);
    }
  }

  get newPasswordError(): string {
    const ctrl = this.form.get('newPassword');
    if (ctrl?.touched && ctrl.invalid) {
      if (ctrl.hasError('required')) return 'Пароль обязателен';
      if (ctrl.hasError('minlength')) return 'Минимум 6 символов';
    }
    return '';
  }

  get confirmPasswordError(): string {
    const ctrl = this.form.get('confirmPassword');
    if (ctrl?.touched) {
      if (ctrl.hasError('required')) return 'Повторите пароль';
      if (this.form.hasError('passwordMismatch')) return 'Пароли не совпадают';
    }
    return '';
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword.update((v) => !v);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { newPassword } = this.form.value;
    this.loading.set(true);
    this.authService.resetPassword(this.email, this.token, newPassword!).subscribe({
      next: () => {
        this.loading.set(false);
        this.toastService.success('Пароль успешно изменён. Войдите с новым паролем.');
        this.router.navigate(['/login']);
      },
      error: (err: { message: string }) => {
        this.loading.set(false);
        this.toastService.error(err.message);
      },
    });
  }
}
