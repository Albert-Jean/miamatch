import { JwtClaims } from '../models/auth.model';

const NAME_IDENTIFIER_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const HOUSEHOLD_ID_CLAIM = 'householdId';

// .NET's JwtRegisteredClaimNames map short names to long XML schema URIs unless
// the handler's InboundClaimTypeMap is cleared, so both forms are checked here.
export function decodeJwt(token: string): JwtClaims {
  const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));

  const userId = payload[NAME_IDENTIFIER_CLAIM] ?? payload['nameidentifier'] ?? payload['sub'];
  const name = payload[NAME_CLAIM] ?? payload['name'];
  const rawHouseholdIds = payload[HOUSEHOLD_ID_CLAIM] ?? [];
  const householdIds = Array.isArray(rawHouseholdIds) ? rawHouseholdIds : [rawHouseholdIds];

  return { userId, name, householdIds };
}
