import { Component, inject } from '@angular/core';
import { LucideAngularModule, CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-angular';
import { ToastService } from './toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [LucideAngularModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss',
})
export class ToastComponent {
  private toastService = inject(ToastService);

  readonly toasts = this.toastService.toasts;

  readonly icons = {
    success: CheckCircle,
    error: XCircle,
    warning: AlertTriangle,
    info: Info,
    close: X,
  };

  dismiss(id: number): void {
    this.toastService.remove(id);
  }
}
