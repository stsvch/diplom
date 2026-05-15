// confirm-email.component.ts
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { LucideAngularModule, BookOpen, CheckCircle, XCircle, Loader } from 'lucide-angular';
import { AuthService } from '../../../core/services/auth.service';

// Экран подтверждения email берёт userId/token из ссылки и показывает статус подтверждения аккаунта.
@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [RouterLink, LucideAngularModule],
  templateUrl: './confirm-email.component.html',
  styleUrl: './confirm-email.component.scss',
})
export class ConfirmEmailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  readonly BookOpenIcon = BookOpen;
  readonly CheckCircleIcon = CheckCircle;
  readonly XCircleIcon = XCircle;
  readonly LoaderIcon = Loader;

  // Signals loading/error/success отражают результат подтверждения ссылки из query-параметров.
  readonly status = signal<'loading' | 'success' | 'error'>('loading');
  readonly message = signal('');

  // При открытии страницы берём userId/token из ссылки и сразу запускаем подтверждение email.
  ngOnInit(): void {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!userId || !token) {
      this.status.set('error');
      this.message.set('Неверная ссылка подтверждения. Параметры отсутствуют.');
      return;
    }

    // При инициализации компонент валидирует query-параметры и вызывает confirmEmail; ручного submit здесь нет.
    this.authService.confirmEmail(userId, token).subscribe({
      next: (res) => {
        this.status.set('success');
        this.message.set(res.message || 'Email успешно подтверждён!');
      },
      error: (err) => {
        this.status.set('error');
        this.message.set((err as { message: string }).message || 'Не удалось подтвердить email.');
      },
    });
  }
}
