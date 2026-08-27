import { Component, inject } from '@angular/core';
import { MatchesStoreService } from '../../core/matches/matches-store.service';

@Component({
  selector: 'app-matches',
  templateUrl: './matches.component.html',
})
export class MatchesComponent {
  protected readonly matchesStore = inject(MatchesStoreService);
}
