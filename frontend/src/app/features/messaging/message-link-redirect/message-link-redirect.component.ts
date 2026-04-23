import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

@Component({
  selector: 'app-message-link-redirect',
  standalone: true,
  template: '',
})
export class MessageLinkRedirectComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  ngOnInit(): void {
    const chatId = this.route.snapshot.paramMap.get('chatId');
    const role = this.authService.userRole();

    if (!chatId) {
      this.router.navigate(['/login'], { replaceUrl: true });
      return;
    }

    if (role === UserRole.Student) {
      this.router.navigate(['/student/messages', chatId], { replaceUrl: true });
      return;
    }

    if (role === UserRole.Teacher) {
      this.router.navigate(['/teacher/messages', chatId], { replaceUrl: true });
      return;
    }

    this.router.navigate(['/admin/dashboard'], { replaceUrl: true });
  }
}
