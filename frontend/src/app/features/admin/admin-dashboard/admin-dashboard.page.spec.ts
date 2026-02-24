import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { AdminDashboardPage } from './admin-dashboard.page';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService, AuthService } from '../../../core';
import { PlatformStatsResponse } from '../../../models';

describe('AdminDashboardPage', () => {
  let component: AdminDashboardPage;
  let fixture: ComponentFixture<AdminDashboardPage>;
  let adminService: jasmine.SpyObj<AdminService>;

  const mockStats: PlatformStatsResponse = {
    users: { totalUsers: 1000, activeUsers: 800, suspendedUsers: 5, shoppers: 700, creators: 200, retailers: 50, admins: 3, newUsersLast7Days: 50, newUsersLast30Days: 200 },
    content: { totalPosts: 500, publishedPosts: 300, pendingReviewPosts: 20, draftPosts: 150, rejectedPosts: 10, flaggedPosts: 5, removedPosts: 3, totalReports: 15, pendingReports: 5, postsLast7Days: 30, postsLast30Days: 100 },
    verifications: { pendingVerifications: 10, approvedCreators: 100, rejectedVerifications: 5, totalCreators: 150 },
    engagement: { totalViews: 50000, totalLikes: 10000, totalSaves: 5000, totalClicks: 3000, viewsLast7Days: 5000, clicksLast7Days: 500 },
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getPlatformStats']);
    adminService.getPlatformStats.and.returnValue(of(mockStats));

    const authService = jasmine.createSpyObj('AuthService', ['logout'], {
      isAuthenticated: jasmine.createSpy().and.returnValue(true),
      userType: jasmine.createSpy().and.returnValue('Admin'),
      profileImageUrl: jasmine.createSpy().and.returnValue(null),
    });

    await TestBed.configureTestingModule({
      imports: [AdminDashboardPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['success', 'error']) },
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: { navigate: jasmine.createSpy(), createUrlTree: jasmine.createSpy().and.returnValue({}), serializeUrl: jasmine.createSpy().and.returnValue('') } },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboardPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load stats on init', () => {
    fixture.detectChanges();
    expect(adminService.getPlatformStats).toHaveBeenCalled();
    expect(component.stats()).toEqual(mockStats);
  });

  it('should set isLoading to false after load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBeFalse();
  });

  it('should format numbers >= 1M', () => {
    expect(component.formatNumber(1500000)).toBe('1.5M');
  });

  it('should format numbers >= 1K', () => {
    expect(component.formatNumber(1000)).toBe('1.0K');
    expect(component.formatNumber(2500)).toBe('2.5K');
  });

  it('should format numbers < 1K', () => {
    expect(component.formatNumber(999)).toBe('999');
  });
});
