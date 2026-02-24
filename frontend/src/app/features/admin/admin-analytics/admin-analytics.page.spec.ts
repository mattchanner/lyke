import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { AdminAnalyticsPage } from './admin-analytics.page';
import { AdminService } from '../../../core/services/admin.service';
import { AdminAnalyticsResponse } from '../../../models';

describe('AdminAnalyticsPage', () => {
  let component: AdminAnalyticsPage;
  let fixture: ComponentFixture<AdminAnalyticsPage>;
  let adminService: jasmine.SpyObj<AdminService>;

  const mockAnalytics: AdminAnalyticsResponse = {
    summary: { newUsers: 50, postsPublished: 30, views: 10000, likes: 2000, saves: 500, clicks: 300 },
    dailyMetrics: [
      { date: '2026-01-01', newUsers: 10, postsPublished: 5, views: 2000, likes: 400, saves: 100, clicks: 60 },
      { date: '2026-01-02', newUsers: 15, postsPublished: 8, views: 3000, likes: 600, saves: 150, clicks: 90 },
      { date: '2026-01-03', newUsers: 25, postsPublished: 17, views: 5000, likes: 1000, saves: 250, clicks: 150 },
    ],
    topCreators: [],
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getPlatformAnalytics']);
    adminService.getPlatformAnalytics.and.returnValue(of(mockAnalytics));

    await TestBed.configureTestingModule({
      imports: [AdminAnalyticsPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminAnalyticsPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load analytics on init', () => {
    fixture.detectChanges();
    expect(adminService.getPlatformAnalytics).toHaveBeenCalled();
    expect(component.analytics()).toEqual(mockAnalytics);
  });

  it('should set isLoading false after load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBeFalse();
  });

  it('should compute chartTotal for views', () => {
    fixture.detectChanges();
    component.setChartMetric('views');
    expect(component.chartTotal()).toBe(10000);
  });

  it('should compute chartTotal for newUsers', () => {
    fixture.detectChanges();
    component.setChartMetric('newUsers');
    expect(component.chartTotal()).toBe(50);
  });

  it('should change date range and reload', () => {
    fixture.detectChanges();
    adminService.getPlatformAnalytics.calls.reset();
    component.setDateRange('7d');
    expect(component.dateRange()).toBe('7d');
    expect(adminService.getPlatformAnalytics).toHaveBeenCalled();
  });

  it('should not reload if same date range', () => {
    fixture.detectChanges();
    adminService.getPlatformAnalytics.calls.reset();
    component.setDateRange('30d');
    expect(adminService.getPlatformAnalytics).not.toHaveBeenCalled();
  });

  it('should return 0 for chartTotal when no analytics', () => {
    expect(component.chartTotal()).toBe(0);
  });
});
