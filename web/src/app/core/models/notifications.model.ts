export interface VapidKeyResponse {
  publicKey: string;
}

export interface PushSubscriptionRequest {
  endpoint: string;
  p256dhKey: string;
  authKey: string;
}
