import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { PushSubscriptionRequest, VapidKeyResponse } from '../core/models/notifications.model';

@Injectable({ providedIn: 'root' })
export class NotificationsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.notificationsApiUrl;

  getVapidPublicKey() {
    return this.http.get<VapidKeyResponse>(`${this.baseUrl}/vapid`);
  }

  subscribe(request: PushSubscriptionRequest) {
    return this.http.post<void>(`${this.baseUrl}/push/subscribe`, request);
  }
}
