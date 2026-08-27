import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/toast/toast.service';
import { UsersApiService } from '../../data-access/users-api.service';

type Step = 'auth' | 'household';
type AuthMode = 'login' | 'register';
type HouseholdMode = 'choose' | 'join' | 'created';

const DEFAULT_HOUSEHOLD_NAME = 'Notre foyer';

@Component({
  selector: 'app-onboarding',
  imports: [FormsModule],
  templateUrl: './onboarding.component.html',
})
export class OnboardingComponent {
  private readonly auth = inject(AuthService);
  private readonly usersApi = inject(UsersApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly step = signal<Step>('auth');
  protected readonly authMode = signal<AuthMode>('login');
  protected readonly householdMode = signal<HouseholdMode>('choose');
  protected readonly busy = signal(false);
  protected readonly error = signal('');

  protected name = '';
  protected email = '';
  protected password = '';
  protected joinCode = '';
  protected createdInviteCode = '';

  submitAuth(): void {
    if (!this.email.trim() || !this.password.trim()) return;
    this.busy.set(true);
    this.error.set('');

    const request$ =
      this.authMode() === 'login'
        ? this.auth.login({ email: this.email, password: this.password })
        : this.auth.register({ name: this.name, email: this.email, password: this.password });

    request$.subscribe({
      next: () => {
        this.auth.refreshMe().subscribe({
          next: () => {
            this.busy.set(false);
            if (this.auth.households().length > 0) {
              this.router.navigateByUrl('/swipe');
            } else {
              this.step.set('household');
            }
          },
          error: () => this.busy.set(false),
        });
      },
      error: () => {
        this.error.set('Identifiants invalides ou compte déjà existant.');
        this.busy.set(false);
      },
    });
  }

  createHousehold(): void {
    this.busy.set(true);
    this.error.set('');
    this.usersApi.createHousehold({ name: DEFAULT_HOUSEHOLD_NAME }).subscribe({
      next: (household) => {
        this.auth.applyToken(household.token);
        this.auth.cacheHousehold(household);
        this.createdInviteCode = household.inviteCode;
        this.householdMode.set('created');
        this.busy.set(false);
      },
      error: () => {
        this.error.set('Impossible de créer le foyer.');
        this.busy.set(false);
      },
    });
  }

  joinHousehold(): void {
    if (!this.joinCode.trim()) return;
    this.busy.set(true);
    this.error.set('');
    this.usersApi.joinHousehold({ inviteCode: this.joinCode.trim().toUpperCase() }).subscribe({
      next: (household) => {
        this.auth.applyToken(household.token);
        this.auth.cacheHousehold(household);
        this.finishOnboarding();
      },
      error: () => {
        this.error.set('Code invalide.');
        this.busy.set(false);
      },
    });
  }

  finishOnboarding(): void {
    this.auth.refreshMe().subscribe(() => this.router.navigateByUrl('/swipe'));
  }

  copyInviteCode(): void {
    void navigator.clipboard.writeText(this.createdInviteCode);
    this.toast.show('Code copié ✓');
  }
}
