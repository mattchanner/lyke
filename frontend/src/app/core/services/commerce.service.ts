import { Injectable, inject, signal, OnDestroy } from '@angular/core';
import { HttpContext } from '@angular/common/http';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { Capacitor, type PluginListenerHandle } from '@capacitor/core';
import { Browser } from '@capacitor/browser';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { ToastService } from './toast.service';
import { SUPPRESS_ERROR_TOAST } from '../interceptors/error.interceptor';
import {
  ApiResponse,
  TrackClickRequest,
  TrackClickResponse,
  ProductResponse,
  RetailerResponse,
  RetailerProductsResponse,
} from '../../models';

const SESSION_ID_KEY = 'lyke_session_id';

export interface ClickContext {
  source: TrackClickRequest['source'];
  feedPosition?: number;
  searchQuery?: string;
}

@Injectable({
  providedIn: 'root',
})
export class CommerceService implements OnDestroy {
  private readonly api = inject(ApiService);
  private readonly storage = inject(StorageService);
  private readonly toast = inject(ToastService);

  private sessionId: string | null = null;
  private browserListeners: PluginListenerHandle[] = [];

  private readonly noToast = { context: new HttpContext().set(SUPPRESS_ERROR_TOAST, true) };

  readonly isTracking = signal(false);
  readonly isBrowsing = signal(false);

  constructor() {
    this.initSession();
    this.setupBrowserListeners();
  }

  ngOnDestroy(): void {
    this.browserListeners.forEach((l) => l.remove());
  }

  private async setupBrowserListeners(): Promise<void> {
    if (Capacitor.getPlatform() === 'web') return;
    const handle = await Browser.addListener('browserFinished', () => {
      this.isBrowsing.set(false);
    });
    this.browserListeners.push(handle);
  }

  private async initSession(): Promise<void> {
    const stored = await this.storage.get(SESSION_ID_KEY);
    if (stored) {
      this.sessionId = stored;
    } else {
      this.sessionId = this.generateSessionId();
      await this.storage.set(SESSION_ID_KEY, this.sessionId);
    }
  }

  private generateSessionId(): string {
    return `${Date.now()}-${Math.random().toString(36).substring(2, 15)}`;
  }

  private getPlatform(): TrackClickRequest['platform'] {
    const platform = Capacitor.getPlatform();
    if (platform === 'ios') return 'ios';
    if (platform === 'android') return 'android';
    return 'web';
  }

  private getAppVersion(): string {
    // In production, this would come from Capacitor App plugin
    return '1.0.0';
  }

  /**
   * Track a product click and open the affiliate URL.
   * This is the main entry point for commerce tracking.
   */
  trackAndShop(
    postId: string,
    postProductId: string,
    context: ClickContext
  ): void {
    this.isTracking.set(true);

    // On web, open a blank window immediately (in the click gesture) so
    // popup blockers don't prevent the redirect once the API responds.
    const platform = Capacitor.getPlatform();
    const preOpenedWindow =
      platform === 'web' ? window.open('about:blank', '_lyke_shop') : null;

    const request: TrackClickRequest = {
      postId,
      postProductId,
      sessionId: this.sessionId ?? undefined,
      source: context.source,
      feedPosition: context.feedPosition,
      searchQuery: context.searchQuery,
      platform: this.getPlatform(),
      appVersion: this.getAppVersion(),
    };

    this.api
      .post<TrackClickResponse>('commerce', 'clicks/track', request)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            if (preOpenedWindow) {
              preOpenedWindow.location.href = response.data.affiliateUrl;
            } else {
              this.openProductUrl(response.data.affiliateUrl);
            }
          } else {
            preOpenedWindow?.close();
            this.toast.error('Unable to open product link');
          }
        },
        error: () => {
          preOpenedWindow?.close();
          this.toast.error('Unable to open product link');
          this.isTracking.set(false);
        },
        complete: () => {
          this.isTracking.set(false);
        },
      });
  }

  /**
   * Track click and return the response (for cases where caller needs the URL)
   */
  trackClick(
    postId: string,
    postProductId: string,
    context: ClickContext
  ): Observable<TrackClickResponse | null> {
    const request: TrackClickRequest = {
      postId,
      postProductId,
      sessionId: this.sessionId ?? undefined,
      source: context.source,
      feedPosition: context.feedPosition,
      searchQuery: context.searchQuery,
      platform: this.getPlatform(),
      appVersion: this.getAppVersion(),
    };

    return this.api
      .post<TrackClickResponse>('commerce', 'clicks/track', request)
      .pipe(
        map((response) => (response.success ? response.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  /**
   * Open product URL in appropriate browser
   */
  async openProductUrl(url: string): Promise<void> {
    const platform = Capacitor.getPlatform();
    this.isBrowsing.set(true);

    if (platform === 'web') {
      window.open(url, '_lyke_shop');
      // No browserFinished event on web, reset after brief delay
      setTimeout(() => this.isBrowsing.set(false), 1000);
    } else {
      // Use Capacitor Browser for native in-app browser
      try {
        await Browser.open({
          url,
          presentationStyle: 'popover',
          toolbarColor: '#c25b3f',
        });
      } catch {
        this.isBrowsing.set(false);
        window.open(url, '_lyke_shop');
      }
    }
  }

  /**
   * Programmatically close the in-app browser
   */
  async closeBrowser(): Promise<void> {
    try {
      await Browser.close();
    } finally {
      this.isBrowsing.set(false);
    }
  }

  /**
   * Get product details
   */
  getProduct(productId: string): Observable<ProductResponse | null> {
    return this.api.get<ProductResponse>('commerce', `products/${productId}`, undefined, this.noToast).pipe(
      map((response) => (response.success ? response.data ?? null : null)),
      catchError(() => of(null))
    );
  }

  /**
   * Search products (for creators tagging posts)
   */
  searchProducts(
    query: string,
    retailerId?: string,
    category?: string,
    page = 1,
    pageSize = 20
  ): Observable<ProductResponse[]> {
    return this.api
      .get<ProductResponse[]>('commerce', 'products/search', {
        query,
        retailerId,
        category,
        page,
        pageSize,
      }, this.noToast)
      .pipe(
        map((response) => (response.success ? response.data ?? [] : [])),
        catchError(() => of([]))
      );
  }

  /**
   * Get all active retailers
   */
  getRetailers(): Observable<RetailerResponse[]> {
    return this.api.get<RetailerResponse[]>('commerce', 'retailers', undefined, this.noToast).pipe(
      map((response) => (response.success ? response.data ?? [] : [])),
      catchError(() => of([]))
    );
  }

  /**
   * Get products for a specific retailer
   */
  getRetailerProducts(
    retailerId: string,
    page = 1,
    pageSize = 20
  ): Observable<ProductResponse[]> {
    return this.api
      .get<RetailerProductsResponse>('commerce', `retailers/${retailerId}/products`, {
        page,
        pageSize,
      }, this.noToast)
      .pipe(
        map((response) => (response.success ? response.data?.products ?? [] : [])),
        catchError(() => of([]))
      );
  }

  /**
   * Search products returning full ApiResponse (for pagination meta)
   */
  searchProductsRaw(
    query: string,
    retailerId?: string,
    category?: string,
    page = 1,
    pageSize = 20
  ): Observable<ApiResponse<ProductResponse[]>> {
    return this.api
      .get<ProductResponse[]>('commerce', 'products/search', {
        query,
        retailerId,
        category,
        page,
        pageSize,
      }, this.noToast)
      .pipe(catchError(() => of({ success: false, data: [] as ProductResponse[] })));
  }

  /**
   * Get retailer products returning full ApiResponse (for pagination meta)
   */
  getRetailerProductsRaw(
    retailerId: string,
    page = 1,
    pageSize = 20
  ): Observable<ApiResponse<ProductResponse[]>> {
    return this.api
      .get<RetailerProductsResponse>('commerce', `retailers/${retailerId}/products`, {
        page,
        pageSize,
      }, this.noToast)
      .pipe(
        map((response) => ({
          ...response,
          data: response.data?.products ?? [],
        })),
        catchError(() => of({ success: false, data: [] as ProductResponse[] }))
      );
  }

  /**
   * Reset session (useful for testing or user logout)
   */
  async resetSession(): Promise<void> {
    this.sessionId = this.generateSessionId();
    await this.storage.set(SESSION_ID_KEY, this.sessionId);
  }
}
