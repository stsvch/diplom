import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { LucideAngularModule, BookOpen, CheckCircle, Eye, EyeOff, Mail, Lock, User } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiError } from '../../../core/models/api-error.model';
import { Router } from '@angular/router';

function passwordMatchValidator(ctrl: AbstractControl): ValidationErrors | null {
  const password = ctrl.get('password')?.value;
  const confirmPassword = ctrl.get('confirmPassword')?.value;
  if (password && confirmPassword && password !== confirmPassword) {
    return { passwordMismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, LucideAngularModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss',
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  readonly BookOpenIcon = BookOpen;
  readonly CheckCircleIcon = CheckCircle;
  readonly EyeIcon = Eye;
  readonly EyeOffIcon = EyeOff;
  readonly MailIcon = Mail;
  readonly LockIcon = Lock;
  readonly UserIcon = User;

  readonly loading = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly selectedRole = signal<'Student' | 'Teacher'>('Student');

  readonly benefits = [
    'Доступ к 200+ курсам по различным дисциплинам',
    'Интерактивные задания и тесты',
    'Сертификаты по завершении курсов',
    'Живое общение с преподавателями',
  ];

  readonly form = this.fb.group(
    {
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator },
  );

  get firstNameError(): string {
    const ctrl = this.form.get('firstName');
    if (ctrl?.touched && ctrl.invalid) {
      if (ctrl.hasError('required')) return 'Имя обязательно';
      if (ctrl.hasError('minlength')) return 'Минимум 2 символа';
    }
    return '';
  }

  get lastNameError(): string {
    const ctrl = this.form.get('lastName');
    if (ctrl?.touched && ctrl.invalid) {
      if (ctrl.hasError('required')) return 'Фамилия обязательна';
      if (ctrl.hasError('minlength')) return 'Минимум 2 символа';
    }
    return '';
  }

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

  get confirmPasswordError(): string {
    const ctrl = this.form.get('confirmPassword');
    if (ctrl?.touched) {
      if (ctrl.hasError('required')) return 'Повторите пароль';
      if (this.form.hasError('passwordMismatch')) return 'Пароли не совпадают';
    }
    return '';
  }

  selectRole(role: 'Student' | 'Teacher'): void {
    this.selectedRole.set(role);
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
    const { firstName, lastName, email, password } = this.form.value;
    this.loading.set(true);
    this.authService
      .register({
        firstName: firstName!,
        lastName: lastName!,
        email: email!,
        password: password!,
        role: this.selectedRole(),
      })
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.toastService.success('Проверьте email для подтверждения аккаунта');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.loading.set(false);
          this.toastService.error((err as ApiError).message);
        },
      });
  }
}
