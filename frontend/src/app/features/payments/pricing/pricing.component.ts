import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { LucideAngularModule, Check, Loader2, Users, User, Sparkles } from 'lucide-angular';
import { PaymentsService } from '../services/payments.service';
import { SubscriptionPlanDto, UserSubscriptionDto } from '../models/payments.model';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ButtonComponent } from '../../../shared/components/button/button.component';
import { parseApiError } from '../../../core/models/api-error.model';
import { AuthService } from '../../../core/services/auth.service';

interface TierFeature {
  text: string;
  included: boolean;
}

@Component({
  selector: 'app-pricing',
  standalone: true,
  imports: [CommonModule, LucideAngularModule, ButtonComponent],
  templateUrl: './pricing.component.html',
  styleUrl: './pricing.component.scss',
})
export class PricingComponent implements OnInit {
  private readonly paymentsService = inject(PaymentsService);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly icons = {
    check: Check,
    loader: Loader2,
    users: Users,
    user: User,
    sparkles: Sparkles,
  };

  readonly plans = signal<SubscriptionPlanDto[]>([]);
  readonly mySubscriptions = signal<UserSubscriptionDto[]>([]);
  readonly loading = signal(false);
  readonly checkingOutPlanId = signal<string | null>(null);

  readonly activeSubscription = computed(() =>
    this.mySubscriptions().find((s) => s.status === 'Active'),
  );

  ngOnInit(): void {
    this.loadPlans();
    if (this.authService.isAuthenticated()) {
      this.loadMySubscriptions();
    }
  }

  private loadPlans(): void {
    this.loading.set(true);
    this.paymentsService.getSubscriptionPlans().subscribe({
      next: (plans) => {
        this.plans.set(plans.filter((p) => p.isActive).sort((a, b) => a.sortOrder - b.sortOrder));
        this.loading.set(false);
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.loading.set(false);
      },
    });
  }

  private loadMySubscriptions(): void {
    this.paymentsService.getMySubscriptions().subscribe({
      next: (subs) => this.mySubscriptions.set(subs),
      error: () => { /* не критично */ },
    });
  }

  featuresFor(plan: SubscriptionPlanDto): TierFeature[] {
    const features: TierFeature[] = [
      { text: 'Доступ к покупке курсов', included: true },
      { text: 'Словарь курсов, которые вы купили', included: true },
    ];

    if (plan.groupSlotsPerMonth > 0) {
      features.push({ text: `${plan.groupSlotsPerMonth} групповых занятий в месяц`, included: true });
    } else {
      features.push({ text: 'Групповые живые занятия', included: false });
    }

    if (plan.individualSlotsPerMonth > 0) {
      features.push({ text: `${plan.individualSlotsPerMonth} индивидуальных занятий в месяц`, included: true });
    } else {
      features.push({ text: 'Индивидуальные занятия 1-на-1', included: false });
    }

    return features;
  }

  freeFeatures(): TierFeature[] {
    return [
      { text: 'Доступ к покупке курсов', included: true },
      { text: 'Словарь курсов, которые вы купили', included: true },
      { text: 'Групповые живые занятия', included: false },
      { text: 'Индивидуальные занятия 1-на-1', included: false },
    ];
  }

  isCurrentPlan(planId: string): boolean {
    return this.activeSubscription()?.subscriptionPlanId === planId;
  }

  subscribe(plan: SubscriptionPlanDto): void {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login']);
      return;
    }
    if (this.isCurrentPlan(plan.id)) {
      this.toastService.info('Вы уже подписаны на этот тариф.');
      return;
    }

    this.checkingOutPlanId.set(plan.id);
    this.paymentsService.createSubscriptionCheckout(plan.id).subscribe({
      next: (session) => {
        window.location.href = session.checkoutUrl;
      },
      error: (err) => {
        this.toastService.error(parseApiError(err).message);
        this.checkingOutPlanId.set(null);
      },
    });
  }

  formatPrice(price: number, currency: string): string {
    if (price === 0) return 'Бесплатно';
    const symbol = currency.toLowerCase() === 'usd' ? '$' : currency.toUpperCase();
    return `${symbol}${price}`;
  }
}
