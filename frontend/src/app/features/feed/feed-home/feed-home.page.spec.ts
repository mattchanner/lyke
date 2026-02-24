import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of, Subject } from 'rxjs';
import { FeedHomePage } from './feed-home.page';
import { ApiService, AuthService, PostEngagementService, AnalyticsService } from '../../../core';
import { FeedSortBy, UserType } from '../../../models';

describe('FeedHomePage', () => {
  let component: FeedHomePage;
  let fixture: ComponentFixture<FeedHomePage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let engagementService: jasmine.SpyObj<PostEngagementService>;
  let analyticsService: jasmine.SpyObj<AnalyticsService>;

  const mockFeedResponse = {
    success: true,
    data: [
      {
        id: 'post-1', title: 'Post 1', description: null, mediaType: 'Image',
        mediaUrls: ['url1'], thumbnailUrls: ['thumb1'],
        creator: { id: 'c1', displayName: 'Creator', isVerified: false, bodyProfile: null, profileImageUrl: null },
        products: [], engagements: { views: 10, likes: 5, saves: 2, shares: 1 },
        similarityScore: 0.8, isLiked: false, isSaved: false, publishedAt: '2024-01-01',
      },
    ],
    meta: { page: 1, pageSize: 20, totalCount: 1, hasNextPage: false },
  };

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'delete']);
    engagementService = jasmine.createSpyObj('PostEngagementService', ['notify'], {
      engagementChanged: new Subject(),
    });
    analyticsService = jasmine.createSpyObj('AnalyticsService', ['track']);

    const authService = {
      isAuthenticated: signal(true),
      profileImageUrl: signal(null),
      userType: signal(UserType.Shopper),
      userId: signal('user-1'),
    };

    apiService.get.and.returnValue(of(mockFeedResponse) as any);

    await TestBed.configureTestingModule({
      imports: [FeedHomePage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: AuthService, useValue: authService },
        { provide: PostEngagementService, useValue: engagementService },
        { provide: AnalyticsService, useValue: analyticsService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}) } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FeedHomePage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load feed on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(apiService.get).toHaveBeenCalled();
    expect(component.posts().length).toBe(1);
  }));

  it('should set isLoading during load', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.isLoading()).toBe(false);
  }));

  it('should set hasMore from response meta', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.hasMore()).toBe(false);
  }));

  it('should reset page on setSortBy', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.get.calls.reset();
    component.setSortBy(FeedSortBy.Recent);
    tick();
    expect(component.sortBy()).toBe(FeedSortBy.Recent);
    expect(component.currentPage()).toBe(1);
    expect(apiService.get).toHaveBeenCalled();
  }));

  it('should not reload if same sort', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.get.calls.reset();
    component.setSortBy(FeedSortBy.Relevance);
    tick();
    expect(apiService.get).not.toHaveBeenCalled();
  }));

  it('should increment page on loadMore', fakeAsync(() => {
    fixture.detectChanges();
    tick();

    const mockResponse2 = { ...mockFeedResponse, data: [{ ...mockFeedResponse.data[0], id: 'post-2' }] };
    apiService.get.and.returnValue(of(mockResponse2) as any);

    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    tick(1000);

    expect(component.currentPage()).toBe(2);
  }));

  it('should reset and reload on onRefresh', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.get.calls.reset();

    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.onRefresh(event);
    tick(1000);

    expect(apiService.get).toHaveBeenCalled();
  }));

  it('should compute activeFilterCount', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.activeFilterCount()).toBe(0);

    component['filterCategory'].set('dresses');
    expect(component.activeFilterCount()).toBe(1);

    component['filterRetailerId'].set('r1');
    expect(component.activeFilterCount()).toBe(2);
  }));

  it('should apply filters on onFiltersChanged', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.get.calls.reset();

    component.onFiltersChanged({ category: 'tops', retailerId: null, fitTagIds: [] });
    tick();

    expect(component['filterCategory']()).toBe('tops');
    expect(apiService.get).toHaveBeenCalled();
  }));

  it('should clear category on removeCategory', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component['filterCategory'].set('test');
    apiService.get.calls.reset();

    component.removeCategory();
    tick();

    expect(component['filterCategory']()).toBeNull();
    expect(apiService.get).toHaveBeenCalled();
  }));

  it('should clear retailer on removeRetailer', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component['filterRetailerId'].set('r1');
    apiService.get.calls.reset();

    component.removeRetailer();
    tick();

    expect(component['filterRetailerId']()).toBeNull();
    expect(apiService.get).toHaveBeenCalled();
  }));

  it('should clear creator on removeCreator', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component['filterCreatorId'].set('c1');
    apiService.get.calls.reset();

    component.removeCreator();
    tick();

    expect(component['filterCreatorId']()).toBeNull();
    expect(apiService.get).toHaveBeenCalled();
  }));

  it('should update posts on engagement change', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.posts()[0].isLiked).toBe(false);

    (engagementService.engagementChanged as Subject<any>).next({
      postId: 'post-1', type: 'like', state: true,
    });

    expect(component.posts()[0].isLiked).toBe(true);
    expect(component.posts()[0].engagements.likes).toBe(6);
  }));

  it('should open and close filter', () => {
    fixture.detectChanges();
    component.openFilter();
    expect(component.isFilterOpen()).toBe(true);
    component.closeFilter();
    expect(component.isFilterOpen()).toBe(false);
  });

  it('should handle creatorId from query params', fakeAsync(() => {
    TestBed.resetTestingModule();
    apiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'delete']);
    apiService.get.and.returnValue(of(mockFeedResponse) as any);
    engagementService = jasmine.createSpyObj('PostEngagementService', ['notify'], {
      engagementChanged: new Subject(),
    });

    TestBed.configureTestingModule({
      imports: [FeedHomePage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: AuthService, useValue: { isAuthenticated: signal(true), profileImageUrl: signal(null), userType: signal(UserType.Shopper), userId: signal('user-1') } },
        { provide: PostEngagementService, useValue: engagementService },
        { provide: AnalyticsService, useValue: analyticsService },
        { provide: ActivatedRoute, useValue: { queryParams: of({ creatorId: 'c1', creatorName: 'Creator1' }) } },
      ],
    });

    const f = TestBed.createComponent(FeedHomePage);
    const c = f.componentInstance;
    f.detectChanges();
    tick();

    expect(c['filterCreatorId']()).toBe('c1');
    expect(c['filterCreatorName']()).toBe('Creator1');
  }));
});
