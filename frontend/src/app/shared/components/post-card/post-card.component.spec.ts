import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { PostCardComponent } from './post-card.component';
import { ApiService, ToastService, AnalyticsService } from '../../../core';
import { ActionSheetController, AlertController } from '@ionic/angular/standalone';
import { FeedPostResponse, MediaType, FitRating, EngagementType } from '../../../models';

describe('PostCardComponent', () => {
  let component: PostCardComponent;
  let fixture: ComponentFixture<PostCardComponent>;
  let apiService: jasmine.SpyObj<ApiService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let analyticsService: jasmine.SpyObj<AnalyticsService>;
  let actionSheetCtrl: jasmine.SpyObj<ActionSheetController>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockPost: FeedPostResponse = {
    id: 'post-1',
    title: 'Test Post',
    description: 'Test Description',
    mediaType: MediaType.Image,
    mediaUrls: ['http://example.com/img1.jpg', 'http://example.com/img2.jpg'],
    thumbnailUrls: ['http://example.com/thumb1.jpg', 'http://example.com/thumb2.jpg'],
    creator: {
      id: 'creator-1',
      displayName: 'Test Creator',
      isVerified: true,
      bodyProfile: null,
      profileImageUrl: null,
    },
    products: [
      {
        id: 'pp-1',
        productId: 'prod-1',
        productName: 'Test Product',
        productImageUrl: null,
        price: 49.99,
        currency: 'USD',
        sizeWorn: 'M',
        fitRating: FitRating.TrueToSize,
        fitNotes: null,
        fitTags: [],
      },
    ],
    engagements: { views: 100, likes: 10, saves: 5, shares: 2 },
    similarityScore: 0.9,
    isLiked: false,
    isSaved: false,
    isFollowing: false,
    publishedAt: '2024-01-01T00:00:00Z',
  };

  const mockEvent = { stopPropagation: jasmine.createSpy(), preventDefault: jasmine.createSpy() } as unknown as Event;

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'delete']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);
    analyticsService = jasmine.createSpyObj('AnalyticsService', ['track']);
    actionSheetCtrl = jasmine.createSpyObj('ActionSheetController', ['create']);
    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);

    await TestBed.configureTestingModule({
      imports: [PostCardComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: ToastService, useValue: toastService },
        { provide: AnalyticsService, useValue: analyticsService },
        { provide: ActionSheetController, useValue: actionSheetCtrl },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PostCardComponent);
    component = fixture.componentInstance;
    component.post = JSON.parse(JSON.stringify(mockPost));
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should convert post media to MediaItem array', () => {
    const items = component.mediaItems;
    expect(items.length).toBe(2);
    expect(items[0].url).toBe('http://example.com/img1.jpg');
    expect(items[0].type).toBe(MediaType.Image);
    expect(items[0].thumbnailUrl).toBe('http://example.com/thumb1.jpg');
  });

  describe('formatCount', () => {
    it('should return "0" for 0', () => {
      expect(component.formatCount(0)).toBe('0');
    });

    it('should return "999" for 999', () => {
      expect(component.formatCount(999)).toBe('999');
    });

    it('should return "1.0K" for 1000', () => {
      expect(component.formatCount(1000)).toBe('1.0K');
    });

    it('should return "1.5M" for 1500000', () => {
      expect(component.formatCount(1500000)).toBe('1.5M');
    });
  });

  describe('toggleLike', () => {
    it('should optimistically like and call API', fakeAsync(() => {
      apiService.post.and.returnValue(of({ success: true, data: null }));
      spyOn(component.liked, 'emit');

      component.toggleLike(mockEvent);
      expect(component.post.isLiked).toBe(true);
      expect(component.post.engagements.likes).toBe(11);

      tick();
      expect(apiService.post).toHaveBeenCalled();
      expect(component.liked.emit).toHaveBeenCalledWith({ postId: 'post-1', liked: true });
    }));

    it('should revert on API error', fakeAsync(() => {
      apiService.post.and.returnValue(throwError(() => new Error('fail')));

      component.toggleLike(mockEvent);
      tick();

      expect(component.post.isLiked).toBe(false);
      expect(component.post.engagements.likes).toBe(10);
      expect(toastService.error).toHaveBeenCalledWith('Failed to update like');
    }));

    it('should unlike when already liked', fakeAsync(() => {
      component.post.isLiked = true;
      component.post.engagements.likes = 11;
      apiService.delete.and.returnValue(of({ success: true, data: null }));
      spyOn(component.liked, 'emit');

      component.toggleLike(mockEvent);
      expect(component.post.isLiked).toBe(false);
      expect(component.post.engagements.likes).toBe(10);

      tick();
      expect(apiService.delete).toHaveBeenCalled();
      expect(component.liked.emit).toHaveBeenCalledWith({ postId: 'post-1', liked: false });
    }));
  });

  describe('toggleSave', () => {
    it('should optimistically save and call API', fakeAsync(() => {
      apiService.post.and.returnValue(of({ success: true, data: null }));
      spyOn(component.saved, 'emit');

      component.toggleSave(mockEvent);
      expect(component.post.isSaved).toBe(true);
      expect(component.post.engagements.saves).toBe(6);

      tick();
      expect(component.saved.emit).toHaveBeenCalledWith({ postId: 'post-1', saved: true });
    }));

    it('should revert save on API error', fakeAsync(() => {
      apiService.post.and.returnValue(throwError(() => new Error('fail')));

      component.toggleSave(mockEvent);
      tick();

      expect(component.post.isSaved).toBe(false);
      expect(component.post.engagements.saves).toBe(5);
      expect(toastService.error).toHaveBeenCalledWith('Failed to save post');
    }));
  });

  it('should emit productTapped on product click', () => {
    spyOn(component.productTapped, 'emit');
    component.onProductTap(mockEvent, mockPost.products[0]);
    expect(component.productTapped.emit).toHaveBeenCalledWith({
      postId: 'post-1',
      product: mockPost.products[0],
    });
  });

  describe('share', () => {
    it('should copy to clipboard when navigator.share not available', () => {
      const originalShare = navigator.share;
      Object.defineProperty(navigator, 'share', { value: undefined, writable: true, configurable: true });
      spyOn(navigator.clipboard, 'writeText');

      component.share(mockEvent);

      expect(navigator.clipboard.writeText).toHaveBeenCalled();
      expect(toastService.success).toHaveBeenCalledWith('Link copied!');
      expect(analyticsService.track).toHaveBeenCalled();
      Object.defineProperty(navigator, 'share', { value: originalShare, writable: true, configurable: true });
    });
  });

  describe('reportPost', () => {
    it('should open action sheet', async () => {
      const mockActionSheet = { present: jasmine.createSpy('present').and.returnValue(Promise.resolve()) };
      actionSheetCtrl.create.and.returnValue(Promise.resolve(mockActionSheet as any));

      await component.reportPost(mockEvent);

      expect(actionSheetCtrl.create).toHaveBeenCalled();
      expect(mockActionSheet.present).toHaveBeenCalled();
    });
  });

  it('should submit report to API', fakeAsync(() => {
    apiService.post.and.returnValue(of({ success: true, data: null }));

    // Access private method via bracket notation for testing
    (component as any).submitReport({ reason: 'Spam' as any });
    tick();

    expect(apiService.post).toHaveBeenCalled();
    expect(toastService.success).toHaveBeenCalledWith('Report submitted');
  }));

  it('should show duplicate report toast', fakeAsync(() => {
    const error = { details: { Report: ['You have already reported this post'] } };
    apiService.post.and.returnValue(throwError(() => error));

    (component as any).submitReport({ reason: 'Spam' as any });
    tick();

    expect(toastService.error).toHaveBeenCalledWith("You've already reported this post");
  }));

  it('should track analytics on like', fakeAsync(() => {
    apiService.post.and.returnValue(of({ success: true, data: null }));
    component.toggleLike(mockEvent);
    tick();
    expect(analyticsService.track).toHaveBeenCalled();
  }));

  it('should track analytics on save', fakeAsync(() => {
    apiService.post.and.returnValue(of({ success: true, data: null }));
    component.toggleSave(mockEvent);
    tick();
    expect(analyticsService.track).toHaveBeenCalled();
  }));
});
