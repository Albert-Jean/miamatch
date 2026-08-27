import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { NotificationsApiService } from '../../data-access/notifications-api.service';

@Injectable({ providedIn: 'root' })
export class PushService {
  private readonly notificationsApi = inject(NotificationsApiService);

  async registerServiceWorker(): Promise<void> {
    if (!('serviceWorker' in navigator)) return;
    await navigator.serviceWorker.register('push-sw.js');
  }

  async subscribe(): Promise<boolean> {
    if (!('serviceWorker' in navigator) || !('PushManager' in window)) return false;

    const permission = await Notification.requestPermission();
    if (permission !== 'granted') return false;

    const { publicKey } = await firstValueFrom(this.notificationsApi.getVapidPublicKey());
    const registration = await navigator.serviceWorker.ready;
    const subscription = await registration.pushManager.subscribe({
      userVisibleOnly: true,
      applicationServerKey: urlBase64ToUint8Array(publicKey),
    });

    const json = subscription.toJSON();
    await firstValueFrom(
      this.notificationsApi.subscribe({
        endpoint: json.endpoint!,
        p256dhKey: json.keys!['p256dh'],
        authKey: json.keys!['auth'],
      }),
    );
    return true;
  }
}

function urlBase64ToUint8Array(base64: string): BufferSource {
  const padding = '='.repeat((4 - (base64.length % 4)) % 4);
  const normalized = (base64 + padding).replace(/-/g, '+').replace(/_/g, '/');
  const raw = atob(normalized);
  return Uint8Array.from([...raw].map((char) => char.charCodeAt(0))) as BufferSource;
}
