import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap, map, catchError, of } from 'rxjs';
import { Capacitor } from '@capacitor/core';
import { Browser } from '@capacitor/browser';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { ToastService } from './toast.service';
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
export class CommerceService {
  private readonly api = inject(ApiService);
  private readonly storage = inject(StorageService);
  private readonly toast = inject(ToastService);

  private sessionId: string | null = null;
  readonly isTracking = signal(false);

  constructor() {
    this.initSession();
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
            this.openProductUrl(response.data.affiliateUrl);
          } else {
            this.toast.error('Unable to open product link');
          }
        },
        error: () => {
          this.toast.error('Unable to open product link');
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

    if (platform === 'web') {
      window.open(url, '_blank');
    } else {
      // Use Capacitor Browser for native in-app browser
      try {
        await Browser.open({
          url,
          presentationStyle: 'popover',
          toolbarColor: '#ffffff',
        });
      } catch {
        // Fallback to window.open
        window.open(url, '_blank');
      }
    }
  }

  /**
   * Get product details
   */
  getProduct(productId: string): Observable<ProductResponse | null> {
    return this.api.get<ProductResponse>('commerce', `products/${productId}`).pipe(
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
      })
      .pipe(
        map((response) => (response.success ? response.data ?? [] : [])),
        catchError(() => of([]))
      );
  }

  /**
   * Get all active retailers
   */
  getRetailers(): Observable<RetailerResponse[]> {
    return this.api.get<RetailerResponse[]>('commerce', 'retailers').pipe(
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
      })
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
      })
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
      })
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
