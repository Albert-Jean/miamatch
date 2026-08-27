import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuthResponse, HouseholdSummary, LoginRequest, MeResponse, RegisterRequest } from '../core/models/auth.model';
import {
  CreateHouseholdRequest,
  CreateHouseholdResponse,
  JoinHouseholdRequest,
  JoinHouseholdResponse,
} from '../core/models/household.model';

@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.usersApiUrl;

  register(request: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.baseUrl}/users`, request);
  }

  login(request: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.baseUrl}/auth/login`, request);
  }

  getMe() {
    return this.http.get<MeResponse>(`${this.baseUrl}/me`);
  }

  createHousehold(request: CreateHouseholdRequest) {
    return this.http.post<CreateHouseholdResponse>(`${this.baseUrl}/households`, request);
  }

  joinHousehold(request: JoinHouseholdRequest) {
    return this.http.post<JoinHouseholdResponse>(`${this.baseUrl}/households/join`, request);
  }

  getHousehold(householdId: string) {
    return this.http.get<HouseholdSummary>(`${this.baseUrl}/households/${householdId}`);
  }
}
