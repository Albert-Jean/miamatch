import { Component, inject, output, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { PushService } from '../../core/push/push.service';
import { ToastService } from '../../core/toast/toast.service';

@Component({
  selector: 'app-household-sheet',
  templateUrl: './household-sheet.component.html',
})
export class HouseholdSheetComponent {
  protected readonly auth = inject(AuthService);
  private readonly pushService = inject(PushService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly closed = output<void>();

  protected readonly pushBusy = signal(false);

  copyInviteCode(inviteCode: string): void {
    void navigator.clipboard.writeText(inviteCode);
    this.toast.show('Code copié ✓');
  }

  switchHousehold(householdId: string): void {
    this.auth.selectHousehold(householdId);
    this.closed.emit();
  }

  async enablePush(): Promise<void> {
    this.pushBusy.set(true);
    try {
      const ok = await this.pushService.subscribe();
      this.toast.show(ok ? 'Notifications activées ✓' : 'Permission refusée');
    } catch {
      this.toast.show('Notifications indisponibles sur cet appareil');
    } finally {
      this.pushBusy.set(false);
    }
  }

  logout(): void {
    this.auth.logout();
    this.closed.emit();
    this.router.navigateByUrl('/onboarding');
  }
}
