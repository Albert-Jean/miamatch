import { Injectable, computed, inject, signal } from '@angular/core';
import { forkJoin, map, of, switchMap, tap } from 'rxjs';
import { UsersApiService } from '../../data-access/users-api.service';
import { HouseholdSummary, LoginRequest, RegisterRequest } from '../models/auth.model';
import { decodeJwt } from './jwt.util';

const TOKEN_KEY = 'miammatch_token';
const SELECTED_HOUSEHOLD_KEY = 'miammatch_selected_household';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usersApi = inject(UsersApiService);

  private readonly tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly householdsSignal = signal<HouseholdSummary[]>([]);
  private readonly selectedHouseholdIdSignal = signal<string | null>(
    localStorage.getItem(SELECTED_HOUSEHOLD_KEY),
  );
  private readonly displayNameSignal = signal<string | null>(null);

  readonly isAuthenticated = computed(() => this.tokenSignal() !== null);
  readonly claims = computed(() => {
    const token = this.tokenSignal();
    return token ? decodeJwt(token) : null;
  });
  readonly displayName = this.displayNameSignal.asReadonly();
  readonly households = this.householdsSignal.asReadonly();
  readonly selectedHouseholdId = this.selectedHouseholdIdSignal.asReadonly();
  readonly selectedHousehold = computed(() =>
    this.householdsSignal().find((h) => h.householdId === this.selectedHouseholdIdSignal()) ?? null,
  );

  getToken(): string | null {
    return this.tokenSignal();
  }

  register(request: RegisterRequest) {
    return this.usersApi.register(request).pipe(tap((response) => this.applyToken(response.token)));
  }

  login(request: LoginRequest) {
    return this.usersApi.login(request).pipe(tap((response) => this.applyToken(response.token)));
  }

  // Creating or joining a household changes the caller's "householdId" claims, so the
  // backend reissues a token — swap it in or every other service keeps rejecting requests
  // for that household with a 403 until the next login.
  applyToken(token: string): void {
    this.tokenSignal.set(token);
    localStorage.setItem(TOKEN_KEY, token);
  }

  // GET /me only returns household ids, not full household details, so any id
  // not already cached (e.g. from a create/join response) is hydrated individually.
  refreshMe() {
    return this.usersApi.getMe().pipe(
      switchMap((me) => {
        this.displayNameSignal.set(me.name);

        const cached = this.householdsSignal();
        const stillCached = cached.filter((h) => me.householdIds.includes(h.householdId));
        const missingIds = me.householdIds.filter((id) => !cached.some((h) => h.householdId === id));

        const hydrated$ =
          missingIds.length === 0
            ? of(stillCached)
            : forkJoin(missingIds.map((id) => this.usersApi.getHousehold(id))).pipe(
                map((fetched) => [...stillCached, ...fetched]),
              );

        return hydrated$.pipe(
          tap((households) => {
            this.householdsSignal.set(households);
            const currentSelection = this.selectedHouseholdIdSignal();
            const stillMember = me.householdIds.includes(currentSelection ?? '');
            if (!stillMember) {
              this.selectHousehold(me.householdIds[0] ?? null);
            }
          }),
        );
      }),
    );
  }

  cacheHousehold(household: HouseholdSummary): void {
    this.householdsSignal.update((current) => [
      ...current.filter((h) => h.householdId !== household.householdId),
      household,
    ]);
  }

  selectHousehold(householdId: string | null) {
    this.selectedHouseholdIdSignal.set(householdId);
    if (householdId) {
      localStorage.setItem(SELECTED_HOUSEHOLD_KEY, householdId);
    } else {
      localStorage.removeItem(SELECTED_HOUSEHOLD_KEY);
    }
  }

  logout() {
    this.tokenSignal.set(null);
    this.householdsSignal.set([]);
    this.selectedHouseholdIdSignal.set(null);
    this.displayNameSignal.set(null);
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(SELECTED_HOUSEHOLD_KEY);
  }
}
