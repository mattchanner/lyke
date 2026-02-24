import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { DashboardPage } from './dashboard.page';
import { AuthService, CreatorService } from '../../../core';
import { PostStatus, UserType } from '../../../models';

describe('DashboardPage', () => {
  let component: DashboardPage;
  let fixture: ComponentFixture<DashboardPage>;
  let creatorService: jasmine.SpyObj<CreatorService>;

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', [
      'getProfile', 'getAnalytics', 'getEarnings', 'getPosts',
    ]);
    creatorService.getProfile.and.returnValue(of({ id: 'c1', displayName: 'Creator', bio: null, isVerified: false, socialLinks: {}, totalPosts: 5, totalViews: 100, totalLikes: 50 } as any));
    creatorService.getAnalytics.and.returnValue(of({ totalViews: 100, totalLikes: 50, totalSaves: 20, totalClicks: 10, totalEarnings: 99.99, dailyMetrics: [] } as any));
    creatorService.getEarnings.and.returnValue(of({ totalEarnings: 99.99, pendingEarnings: 20, paidEarnings: 79.99 } as any));
    creatorService.getPosts.and.returnValue(of({ success: true, data: [{ id: 'p1', title: 'Post', status: PostStatus.Published }] } as any));

    const authService = {
      isAuthenticated: signal(true),
      userType: signal(UserType.Creator),
      profileImageUrl: signal(null),
    };

    await TestBed.configureTestingModule({
      imports: [DashboardPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: AuthService, useValue: authService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load profile on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getProfile).toHaveBeenCalled();
    expect(component.profile()).toBeTruthy();
  }));

  it('should load analytics on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getAnalytics).toHaveBeenCalled();
    expect(component.analytics()).toBeTruthy();
  }));

  it('should load earnings on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getEarnings).toHaveBeenCalled();
    expect(component.earnings()).toBeTruthy();
  }));

  it('should load recent posts on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getPosts).toHaveBeenCalledWith({ page: 1, pageSize: 3 });
    expect(component.recentPosts().length).toBe(1);
  }));

  it('should reload data on refresh', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.getProfile.calls.reset();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.onRefresh(event);
    tick(1000);
    expect(creatorService.getProfile).toHaveBeenCalled();
  }));

  describe('getStatusColor', () => {
    it('should return medium for Draft', () => {
      expect(component.getStatusColor(PostStatus.Draft)).toBe('medium');
    });
    it('should return warning for PendingReview', () => {
      expect(component.getStatusColor(PostStatus.PendingReview)).toBe('warning');
    });
    it('should return success for Published', () => {
      expect(component.getStatusColor(PostStatus.Published)).toBe('success');
    });
    it('should return danger for Rejected', () => {
      expect(component.getStatusColor(PostStatus.Rejected)).toBe('danger');
    });
  });

  describe('getStatusLabel', () => {
    it('should return Draft for Draft', () => {
      expect(component.getStatusLabel(PostStatus.Draft)).toBe('Draft');
    });
    it('should return Pending for PendingReview', () => {
      expect(component.getStatusLabel(PostStatus.PendingReview)).toBe('Pending');
    });
  });
});
