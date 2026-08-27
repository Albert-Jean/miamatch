import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { Match, SwipeRequest, SwipeResponse } from '../core/models/matching.model';

@Injectable({ providedIn: 'root' })
export class MatchingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.matchingApiUrl;

  swipe(request: SwipeRequest) {
    return this.http.post<SwipeResponse>(`${this.baseUrl}/swipes`, request);
  }

  getMatches(householdId: string) {
    return this.http.get<Match[]>(`${this.baseUrl}/matches`, { params: { householdId } });
  }
}
