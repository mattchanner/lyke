import { Injectable, inject, NgZone } from '@angular/core';
import { Router } from '@angular/router';
import { Capacitor } from '@capacitor/core';
import { App } from '@capacitor/app';

@Injectable({
  providedIn: 'root',
})
export class DeepLinkService {
  private readonly router = inject(Router);
  private readonly zone = inject(NgZone);

  initialize(): void {
    if (!Capacitor.isNativePlatform()) {
      return;
    }

    App.addListener('appUrlOpen', (event) => {
      this.zone.run(() => {
        this.handleUrl(event.url);
      });
    });
  }

  handleUrl(url: string): void {
    try {
      const parsed = new URL(url);
      const path = parsed.pathname;

      // Match /post/{uuid}
      const postMatch = path.match(
        /\/post\/([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})/i
      );
      if (postMatch) {
        this.router.navigateByUrl(`/feed/post/${postMatch[1]}`);
        return;
      }

      // Match /product/{uuid}
      const productMatch = path.match(
        /\/product\/([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})/i
      );
      if (productMatch) {
        this.router.navigateByUrl(`/product/${productMatch[1]}`);
        return;
      }

      // Fallback
      this.router.navigateByUrl('/feed');
    } catch {
      this.router.navigateByUrl('/feed');
    }
  }
}
