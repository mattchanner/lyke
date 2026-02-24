import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { SavedPage } from './saved.page';
import { ApiService } from '../../../core';

describe('SavedPage', () => {
  let component: SavedPage;
  let fixture: ComponentFixture<SavedPage>;
  let apiService: jasmine.SpyObj<ApiService>;

  const mockResponse = {
    success: true,
    data: [
      {
        id: 'post-1', title: 'Saved Post', description: null, mediaType: 'Image',
        mediaUrls: ['url'], thumbnailUrls: ['thumb'],
        creator: { id: 'c1', displayName: 'Creator', isVerified: false, bodyProfile: null, profileImageUrl: null },
        products: [], engagements: { views: 5, likes: 2, saves: 1, shares: 0 },
        similarityScore: 0.5, isLiked: false, isSaved: true, publishedAt: '2024-01-01',
      },
    ],
  };

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get']);
    apiService.get.and.returnValue(of(mockResponse));

    await TestBed.configureTestingModule({
      imports: [SavedPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SavedPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load saved posts on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.posts().length).toBe(1);
    expect(component.posts()[0].id).toBe('post-1');
  }));

  it('should call API on loadSaved', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(apiService.get).toHaveBeenCalledWith('posts', 'saved');
  }));

  it('should reload on onRefresh', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.get.calls.reset();

    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.onRefresh(event);
    tick(1000);

    expect(apiService.get).toHaveBeenCalled();
    expect(event.target.complete).toHaveBeenCalled();
  }));

  it('should handle empty saved list', fakeAsync(() => {
    apiService.get.and.returnValue(of({ success: true, data: [] }));
    fixture.detectChanges();
    tick();
    expect(component.posts().length).toBe(0);
  }));
});
