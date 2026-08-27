export interface CreateHouseholdRequest {
  name: string;
}

export interface CreateHouseholdResponse {
  householdId: string;
  name: string;
  inviteCode: string;
  memberCount: number;
  token: string;
}

export interface JoinHouseholdRequest {
  inviteCode: string;
}

export interface JoinHouseholdResponse {
  householdId: string;
  name: string;
  inviteCode: string;
  memberCount: number;
  token: string;
}
