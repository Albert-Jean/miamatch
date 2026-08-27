import { Component, OnInit, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { MatchesStoreService } from '../core/matches/matches-store.service';

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './shell.component.html',
})
export class ShellComponent implements OnInit {
  private readonly auth = inject(AuthService);
  protected readonly matchesStore = inject(MatchesStoreService);

  protected readonly matchCount = computed(() => this.matchesStore.matches().length);

  ngOnInit(): void {
    const householdId = this.auth.selectedHouseholdId();
    if (householdId) {
      this.matchesStore.refresh(householdId);
    }
  }
}
