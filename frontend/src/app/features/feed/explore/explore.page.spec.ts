import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of, Subject } from 'rxjs';
import { ExplorePage } from './explore.page';
import { ApiService, AnalyticsService } from '../../../core';
import { SearchType } from '../../../models';

describe('ExplorePage', () => {
  let component: ExplorePage;
  let fixture: ComponentFixture<ExplorePage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let analyticsService: jasmine.SpyObj<AnalyticsService>;
  let router: jasmine.SpyObj<Router>;

  const mockSearchResponse = {
    success: true,
    data: {
      posts: [],
      products: [],
      creators: [{ id: 'c1', displayName: 'Creator 1', isVerified: true, profileImageUrl: null, postCount: 5 }],
      totalResults: 1,
    },
  };

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get']);
    analyticsService = jasmine.createSpyObj('AnalyticsService', ['track']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');
    (router as any).events = of(null);

    await TestBed.configureTestingModule({
      imports: [ExplorePage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: AnalyticsService, useValue: analyticsService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ExplorePage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not call API when query is empty', () => {
    component.searchQuery = '';
    component.onSearch();
    expect(apiService.get).not.toHaveBeenCalled();
  });

  it('should not call API when query is whitespace', () => {
    component.searchQuery = '   ';
    component.onSearch();
    expect(apiService.get).not.toHaveBeenCalled();
    expect(component.results()).toBeNull();
  });

  it('should call API with query and populate results', fakeAsync(() => {
    apiService.get.and.returnValue(of(mockSearchResponse));
    component.searchQuery = 'dress';
    component.onSearch();
    tick();
    expect(apiService.get).toHaveBeenCalled();
    expect(component.results()).toBeTruthy();
    expect(component.results()!.totalResults).toBe(1);
  }));

  it('should track analytics on search', fakeAsync(() => {
    apiService.get.and.returnValue(of(mockSearchResponse));
    component.searchQuery = 'dress';
    component.onSearch();
    tick();
    expect(analyticsService.track).toHaveBeenCalledWith('search.execute', jasmine.objectContaining({ query: 'dress' }));
  }));

  it('should re-execute search on type change', fakeAsync(() => {
    apiService.get.and.returnValue(of(mockSearchResponse));
    component.searchQuery = 'dress';
    component.onTypeChange({ detail: { value: SearchType.Creators } } as CustomEvent);
    tick();
    expect(component.searchType()).toBe(SearchType.Creators);
    expect(apiService.get).toHaveBeenCalled();
  }));

  it('should navigate with creator params on onCreatorTap', () => {
    component.onCreatorTap({ id: 'c1', displayName: 'Creator', isVerified: true, profileImageUrl: null, postCount: 5 });
    expect(router.navigate).toHaveBeenCalledWith(['/feed'], {
      queryParams: { creatorId: 'c1', creatorName: 'Creator' },
    });
  });

  it('should set isLoading during search', fakeAsync(() => {
    const subject = new Subject<any>();
    apiService.get.and.returnValue(subject.asObservable());
    component.searchQuery = 'test';
    component.onSearch();
    expect(component.isLoading()).toBe(true);
    subject.next(mockSearchResponse);
    subject.complete();
    tick();
    expect(component.isLoading()).toBe(false);
  }));
});
