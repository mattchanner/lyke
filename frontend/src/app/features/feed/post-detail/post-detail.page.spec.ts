import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { PostDetailPage } from './post-detail.page';
import { ApiService, ToastService, PostEngagementService, AnalyticsService } from '../../../core';
import { ActionSheetController, AlertController } from '@ionic/angular/standalone';
import { MediaType } from '../../../models';

describe('PostDetailPage', () => {
  let component: PostDetailPage;
  let fixture: ComponentFixture<PostDetailPage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let analyticsService: jasmine.SpyObj<AnalyticsService>;
  let engagementService: jasmine.SpyObj<PostEngagementService>;
  let actionSheetCtrl: jasmine.SpyObj<ActionSheetController>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockPost = {
    id: 'post-1', title: 'Test Post', description: 'Desc', mediaType: MediaType.Image,
    mediaUrls: ['http://example.com/img1.jpg'], thumbnailUrls: ['http://example.com/thumb1.jpg'],
    creator: { id: 'c1', displayName: 'Creator', bio: null, isVerified: true, bodyProfile: null, totalPosts: 10, profileImageUrl: null },
    products: [], engagements: { views: 100, likes: 10, saves: 5, shares: 2 },
    similarityScore: 0.9, isLiked: false, isSaved: false, publishedAt: '2024-01-01', createdAt: '2024-01-01',
  };

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'delete']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);
    analyticsService = jasmine.createSpyObj('AnalyticsService', ['track']);
    engagementService = jasmine.createSpyObj('PostEngagementService', ['notify']);
    actionSheetCtrl = jasmine.createSpyObj('ActionSheetController', ['create']);
    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);

    apiService.get.and.returnValue(of({ success: true, data: mockPost }));

    await TestBed.configureTestingModule({
      imports: [PostDetailPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: ToastService, useValue: toastService },
        { provide: AnalyticsService, useValue: analyticsService },
        { provide: PostEngagementService, useValue: engagementService },
        { provide: ActionSheetController, useValue: actionSheetCtrl },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'post-1' } } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PostDetailPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load post from route param', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.post()).toBeTruthy();
    expect(component.post()!.id).toBe('post-1');
  }));

  it('should track view analytics on load', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(analyticsService.track).toHaveBeenCalledWith('post.view', jasmine.objectContaining({ postId: 'post-1' }), 'post-1', 'Post');
  }));

  it('should compute mediaItems from post', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.mediaItems().length).toBe(1);
    expect(component.mediaItems()[0].url).toBe('http://example.com/img1.jpg');
    expect(component.mediaItems()[0].type).toBe(MediaType.Image);
  }));

  it('should return empty mediaItems when no post', () => {
    fixture.detectChanges();
    component['post'].set(null);
    expect(component.mediaItems().length).toBe(0);
  });

  it('should toggle like and notify engagement service', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.post.and.returnValue(of({ success: true, data: null }));

    component.toggleLike();
    tick();

    expect(component.post()!.isLiked).toBe(true);
    expect(component.post()!.engagements.likes).toBe(11);
    expect(engagementService.notify).toHaveBeenCalledWith({ postId: 'post-1', type: 'like', state: true });
  }));

  it('should toggle save and notify engagement service', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.post.and.returnValue(of({ success: true, data: null }));

    component.toggleSave();
    tick();

    expect(component.post()!.isSaved).toBe(true);
    expect(component.post()!.engagements.saves).toBe(6);
    expect(engagementService.notify).toHaveBeenCalledWith({ postId: 'post-1', type: 'save', state: true });
    expect(toastService.success).toHaveBeenCalledWith('Saved!');
  }));

  it('should copy to clipboard on share when navigator.share unavailable', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    const origShare = navigator.share;
    Object.defineProperty(navigator, 'share', { value: undefined, writable: true, configurable: true });
    spyOn(navigator.clipboard, 'writeText');

    component.share();

    expect(navigator.clipboard.writeText).toHaveBeenCalled();
    expect(toastService.success).toHaveBeenCalledWith('Link copied!');
    Object.defineProperty(navigator, 'share', { value: origShare, writable: true, configurable: true });
  }));

  it('should track analytics on share', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    Object.defineProperty(navigator, 'share', { value: undefined, writable: true, configurable: true });
    spyOn(navigator.clipboard, 'writeText');

    component.share();
    expect(analyticsService.track).toHaveBeenCalledWith('post.share', jasmine.objectContaining({ postId: 'post-1' }), 'post-1', 'Post');
  }));

  it('should open action sheet on reportPost', async () => {
    fixture.detectChanges();
    const mockSheet = { present: jasmine.createSpy().and.returnValue(Promise.resolve()) };
    actionSheetCtrl.create.and.returnValue(Promise.resolve(mockSheet as any));

    await component.reportPost();
    expect(actionSheetCtrl.create).toHaveBeenCalled();
    expect(mockSheet.present).toHaveBeenCalled();
  });

  it('should submit report to API', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.post.and.returnValue(of({ success: true, data: null }));

    (component as any).submitReport('post-1', { reason: 'Spam' });
    tick();

    expect(apiService.post).toHaveBeenCalled();
    expect(toastService.success).toHaveBeenCalledWith('Report submitted');
  }));

  it('should show duplicate report toast', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    const err = { details: { Report: ['already reported'] } };
    apiService.post.and.returnValue(throwError(() => err));

    (component as any).submitReport('post-1', { reason: 'Spam' });
    tick();

    expect(toastService.error).toHaveBeenCalledWith("You've already reported this post");
  }));

  it('should not toggle like when post is null', fakeAsync(() => {
    fixture.detectChanges();
    component['post'].set(null);
    component.toggleLike();
    expect(apiService.post).not.toHaveBeenCalled();
  }));

  it('should track like analytics', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.post.and.returnValue(of({ success: true, data: null }));
    component.toggleLike();
    tick();
    expect(analyticsService.track).toHaveBeenCalledWith('post.like', jasmine.any(Object), 'post-1', 'Post');
  }));
});
