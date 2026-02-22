import { Injectable, inject } from '@angular/core';
import { AppInsightsService } from './app-insights.service';

@Injectable({ providedIn: 'root' })
export class WebVitalsService {
  private readonly appInsights = inject(AppInsightsService);

  initialize(): void {
    this.observePaint();
    this.observeLCP();
    this.observeFID();
    this.observeCLS();
  }

  private observePaint(): void {
    try {
      const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
          if (entry.name === 'first-contentful-paint') {
            this.appInsights.trackEvent('web-vital.FCP', {
              value: String(Math.round(entry.startTime)),
            });
            observer.disconnect();
          }
        }
      });
      observer.observe({ type: 'paint', buffered: true });
    } catch {
      // PerformanceObserver not supported
    }
  }

  private observeLCP(): void {
    try {
      const observer = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        const last = entries[entries.length - 1];
        if (last) {
          this.appInsights.trackEvent('web-vital.LCP', {
            value: String(Math.round(last.startTime)),
          });
        }
      });
      observer.observe({ type: 'largest-contentful-paint', buffered: true });
    } catch {
      // Not supported
    }
  }

  private observeFID(): void {
    try {
      const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
          const fidEntry = entry as PerformanceEventTiming;
          this.appInsights.trackEvent('web-vital.FID', {
            value: String(Math.round(fidEntry.processingStart - fidEntry.startTime)),
          });
          observer.disconnect();
        }
      });
      observer.observe({ type: 'first-input', buffered: true });
    } catch {
      // Not supported
    }
  }

  private observeCLS(): void {
    try {
      let clsValue = 0;
      const observer = new PerformanceObserver((list) => {
        for (const entry of list.getEntries()) {
          if (!(entry as any)['hadRecentInput']) {
            clsValue += (entry as any)['value'] ?? 0;
          }
        }
      });
      observer.observe({ type: 'layout-shift', buffered: true });

      // Report CLS when page is hidden
      document.addEventListener(
        'visibilitychange',
        () => {
          if (document.visibilityState === 'hidden') {
            this.appInsights.trackEvent('web-vital.CLS', {
              value: String(clsValue.toFixed(4)),
            });
            observer.disconnect();
          }
        },
        { once: true },
      );
    } catch {
      // Not supported
    }
  }
}
