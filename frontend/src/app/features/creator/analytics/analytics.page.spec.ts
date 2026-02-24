import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { AnalyticsPage } from './analytics.page';
import { CreatorService } from '../../../core';

describe('AnalyticsPage', () => {
  let component: AnalyticsPage;
  let fixture: ComponentFixture<AnalyticsPage>;
  let creatorService: jasmine.SpyObj<CreatorService>;

  const mockAnalytics = {
    summary: {
      totalViews: 1000, totalLikes: 500, totalSaves: 200, totalShares: 0,
      totalClicks: 100, totalEarnings: 500, currency: '$',
    },
    topPosts: [],
    dailyMetrics: [
      { date: '2024-01-01', views: 100, likes: 50, clicks: 20, earnings: 50 },
      { date: '2024-01-02', views: 200, likes: 80, clicks: 30, earnings: 75 },
    ],
  };

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', ['getAnalytics']);
    creatorService.getAnalytics.and.returnValue(of(mockAnalytics as any));

    const router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl'], {
      events: of(),
      url: '/',
    });
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');

    await TestBed.configureTestingModule({
      imports: [AnalyticsPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AnalyticsPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load analytics on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getAnalytics).toHaveBeenCalled();
    expect(component.analytics()).toBeTruthy();
  }));

  it('should compute chartTotal for views', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.chartTotal()).toBe(300); // 100 + 200
  }));

  it('should compute chartTotal for likes', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.setChartMetric('likes' as any);
    expect(component.chartTotal()).toBe(130); // 50 + 80
  }));

  it('should reload on date range change', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.getAnalytics.calls.reset();
    component.setDateRange('7d' as any);
    tick();
    expect(component.dateRange()).toBe('7d');
    expect(creatorService.getAnalytics).toHaveBeenCalled();
  }));

  it('should not reload on same date range', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.getAnalytics.calls.reset();
    component.setDateRange('30d' as any);
    expect(creatorService.getAnalytics).not.toHaveBeenCalled();
  }));

  it('should update chart metric', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.setChartMetric('earnings' as any);
    expect(component.chartMetric()).toBe('earnings');
  }));

  it('should set isLoading false after load', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.isLoading()).toBe(false);
  }));
});
