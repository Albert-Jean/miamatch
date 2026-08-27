import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly messageSignal = signal<string | null>(null);
  readonly message = this.messageSignal.asReadonly();
  private timeoutId?: ReturnType<typeof setTimeout>;

  show(message: string): void {
    clearTimeout(this.timeoutId);
    this.messageSignal.set(message);
    this.timeoutId = setTimeout(() => this.messageSignal.set(null), 1800);
  }
}
