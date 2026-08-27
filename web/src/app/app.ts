import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { PushService } from './core/push/push.service';
import { ToastService } from './core/toast/toast.service';
import { HouseholdSheetComponent } from './shell/household-sheet/household-sheet.component';

@Component({
  imports: [RouterOutlet, HouseholdSheetComponent],
  selector: 'app-root',
  templateUrl: './app.html',
})
export class App implements OnInit {
  protected readonly auth = inject(AuthService);
  protected readonly toast = inject(ToastService);
  private readonly pushService = inject(PushService);

  protected readonly sheetOpen = signal(false);
  protected readonly showHouseholdButton = computed(
    () => this.auth.isAuthenticated() && this.auth.selectedHouseholdId() !== null,
  );
  protected readonly initials = computed(() => (this.auth.displayName() ?? '?').slice(0, 2).toUpperCase());

  ngOnInit(): void {
    void this.pushService.registerServiceWorker();
    if (this.auth.isAuthenticated()) {
      this.auth.refreshMe().subscribe();
    }
  }
}
