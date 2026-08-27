import { Injectable, inject, signal } from '@angular/core';
import { Match } from '../models/matching.model';
import { MatchingApiService } from '../../data-access/matching-api.service';

@Injectable({ providedIn: 'root' })
export class MatchesStoreService {
  private readonly matchingApi = inject(MatchingApiService);

  private readonly matchesSignal = signal<Match[]>([]);
  readonly matches = this.matchesSignal.asReadonly();

  refresh(householdId: string): void {
    this.matchingApi.getMatches(householdId).subscribe((matches) => this.matchesSignal.set(matches));
  }
}
