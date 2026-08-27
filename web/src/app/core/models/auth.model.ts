export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  userId: string;
  token: string;
}

export interface HouseholdSummary {
  householdId: string;
  name: string;
  inviteCode: string;
  memberCount: number;
}

export interface MeResponse {
  id: string;
  name: string;
  email: string;
  householdIds: string[];
}

export interface JwtClaims {
  userId: string;
  name: string;
  householdIds: string[];
}
