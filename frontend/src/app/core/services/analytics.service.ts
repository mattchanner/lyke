import { Injectable, inject, OnDestroy } from '@angular/core';
import { AppInsightsService } from './app-insights.service';
import { ApiService } from './api.service';
import { HttpContext } from '@angular/common/http';
import { SUPPRESS_ERROR_TOAST } from '../interceptors/error.interceptor';

interface PendingEvent {
  eventType: string;
  entityId?: string;
  entityType?: string;
  properties?: Record<string, string>;
  sessionId?: string;
}

@Injectable({ providedIn: 'root' })
export class AnalyticsService implements OnDestroy {
  private readonly appInsights = inject(AppInsightsService);
  private readonly api = inject(ApiService);

  private pendingEvents: PendingEvent[] = [];
  private flushTimer: ReturnType<typeof setTimeout> | null = null;
  private readonly FLUSH_DELAY_MS = 5000;
  private readonly MAX_BATCH_SIZE = 50;

  track(
    eventType: string,
    properties?: Record<string, string>,
    entityId?: string,
    entityType?: string,
  ): void {
    // Immediately dispatch to AppInsights
    this.appInsights.trackEvent(eventType, properties);

    // Queue for backend batch dispatch
    this.pendingEvents.push({
      eventType,
      entityId,
      entityType,
      properties,
    });

    if (this.pendingEvents.length >= this.MAX_BATCH_SIZE) {
      this.flush();
    } else {
      this.scheduleFlush();
    }
  }

  private scheduleFlush(): void {
    if (this.flushTimer) return;
    this.flushTimer = setTimeout(() => this.flush(), this.FLUSH_DELAY_MS);
  }

  private flush(): void {
    if (this.flushTimer) {
      clearTimeout(this.flushTimer);
      this.flushTimer = null;
    }

    if (this.pendingEvents.length === 0) return;

    const events = [...this.pendingEvents];
    this.pendingEvents = [];

    const context = new HttpContext().set(SUPPRESS_ERROR_TOAST, true);
    this.api
      .post('analytics', 'events/batch', { events }, { context })
      .subscribe();
  }

  ngOnDestroy(): void {
    this.flush();
  }
}
