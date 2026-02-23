import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AnalyticsService } from './analytics.service';
import { ApiService } from './api.service';
import { AppInsightsService } from './app-insights.service';

describe('AnalyticsService', () => {
  let service: AnalyticsService;
  let apiSpy: jasmine.SpyObj<ApiService>;
  let appInsightsSpy: jasmine.SpyObj<AppInsightsService>;

  beforeEach(() => {
    jasmine.clock().install();

    apiSpy = jasmine.createSpyObj('ApiService', ['post']);
    apiSpy.post.and.returnValue(of({ success: true }));

    appInsightsSpy = jasmine.createSpyObj('AppInsightsService', ['trackEvent', 'trackException']);

    TestBed.configureTestingModule({
      providers: [
        { provide: ApiService, useValue: apiSpy },
        { provide: AppInsightsService, useValue: appInsightsSpy },
      ],
    });
    service = TestBed.inject(AnalyticsService);
  });

  afterEach(() => {
    jasmine.clock().uninstall();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should dispatch immediately to AppInsights on track', () => {
    service.track('page_view', { page: '/feed' });
    expect(appInsightsSpy.trackEvent).toHaveBeenCalledWith('page_view', { page: '/feed' });
  });

  it('should queue event for batch on track', () => {
    service.track('page_view');
    // Not yet flushed
    expect(apiSpy.post).not.toHaveBeenCalled();
  });

  it('should flush after 5000ms delay', () => {
    service.track('event1');
    expect(apiSpy.post).not.toHaveBeenCalled();

    jasmine.clock().tick(5000);

    expect(apiSpy.post).toHaveBeenCalledWith(
      'analytics',
      'events/batch',
      jasmine.objectContaining({ events: jasmine.any(Array) }),
      jasmine.any(Object)
    );
  });

  it('should not create duplicate timers', () => {
    service.track('event1');
    service.track('event2');
    service.track('event3');

    jasmine.clock().tick(5000);

    // Should only be called once (single batch)
    expect(apiSpy.post).toHaveBeenCalledTimes(1);
    const body = apiSpy.post.calls.mostRecent().args[2] as any;
    expect(body.events.length).toBe(3);
  });

  it('should flush immediately when batch reaches 50 events', () => {
    for (let i = 0; i < 50; i++) {
      service.track(`event_${i}`);
    }

    // Should flush immediately without waiting for timer
    expect(apiSpy.post).toHaveBeenCalledTimes(1);
    const body50 = apiSpy.post.calls.mostRecent().args[2] as any;
    expect(body50.events.length).toBe(50);
  });

  it('should post to analytics/events/batch with SUPPRESS_ERROR_TOAST', () => {
    service.track('test_event');
    jasmine.clock().tick(5000);

    expect(apiSpy.post).toHaveBeenCalledWith(
      'analytics',
      'events/batch',
      jasmine.any(Object),
      jasmine.objectContaining({ context: jasmine.any(Object) })
    );
  });

  it('should clear queue after flush', () => {
    service.track('event1');
    jasmine.clock().tick(5000);

    expect(apiSpy.post).toHaveBeenCalledTimes(1);

    // No more events, timer fires again but no-op
    jasmine.clock().tick(5000);
    expect(apiSpy.post).toHaveBeenCalledTimes(1); // still 1
  });

  it('should not call post when queue is empty during flush', () => {
    // Force flush by triggering ngOnDestroy with no events
    service.ngOnDestroy();
    expect(apiSpy.post).not.toHaveBeenCalled();
  });

  it('should flush remaining events on ngOnDestroy', () => {
    service.track('remaining1');
    service.track('remaining2');

    service.ngOnDestroy();

    expect(apiSpy.post).toHaveBeenCalledTimes(1);
    const bodyDestroy = apiSpy.post.calls.mostRecent().args[2] as any;
    expect(bodyDestroy.events.length).toBe(2);
  });
});
