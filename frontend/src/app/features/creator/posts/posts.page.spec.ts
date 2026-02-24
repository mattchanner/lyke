import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { PostsPage } from './posts.page';
import { CreatorService, ToastService } from '../../../core';
import { PostStatus } from '../../../models';

describe('PostsPage', () => {
  let component: PostsPage;
  let fixture: ComponentFixture<PostsPage>;
  let creatorService: jasmine.SpyObj<CreatorService>;
  let toastService: jasmine.SpyObj<ToastService>;

  const mockResponse = {
    success: true,
    data: [
      { id: 'p1', title: 'Post 1', status: PostStatus.Published, mediaType: 'Image', mediaUrls: [], thumbnailUrls: [], products: [], engagements: { views: 10, likes: 5, saves: 2, shares: 1 }, createdAt: '2024-01-01', publishedAt: '2024-01-01' },
    ],
    meta: { hasNextPage: true },
  };

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', ['getPosts', 'deletePost']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    creatorService.getPosts.and.returnValue(of(mockResponse as any));

    await TestBed.configureTestingModule({
      imports: [PostsPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PostsPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load posts on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getPosts).toHaveBeenCalled();
    expect(component.posts().length).toBe(1);
  }));

  it('should filter by status on tab change', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.getPosts.calls.reset();
    component.onTabChange('draft');
    tick();
    expect(component.activeTab()).toBe('draft');
    expect(creatorService.getPosts).toHaveBeenCalled();
  }));

  it('should not reload on same tab', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.getPosts.calls.reset();
    component.onTabChange('all');
    expect(creatorService.getPosts).not.toHaveBeenCalled();
  }));

  it('should increment page on loadMore', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    tick(1000);
    expect(component.currentPage()).toBe(2);
  }));

  it('should delete post and remove from list', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.deletePost.and.returnValue(of({ success: true } as any));

    component.deletePost(component.posts()[0] as any);
    tick();

    expect(component.posts().length).toBe(0);
    expect(toastService.success).toHaveBeenCalledWith('Post deleted');
  }));

  it('should show error on delete failure', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.deletePost.and.returnValue(of({ success: false, error: { message: 'Cannot delete' } } as any));
    component.deletePost(component.posts()[0] as any);
    tick();
    expect(toastService.error).toHaveBeenCalled();
  }));

  describe('getEmptyMessage', () => {
    it('should return draft message', () => {
      component['activeTab'].set('draft');
      expect(component.getEmptyMessage()).toBe('No draft posts');
    });
    it('should return pending message', () => {
      component['activeTab'].set('pending');
      expect(component.getEmptyMessage()).toBe('No posts pending review');
    });
    it('should return published message', () => {
      component['activeTab'].set('published');
      expect(component.getEmptyMessage()).toBe('No published posts yet');
    });
    it('should return default message', () => {
      component['activeTab'].set('all');
      expect(component.getEmptyMessage()).toBe('No posts yet. Create your first post!');
    });
  });

  it('should set hasMore from response', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.hasMore()).toBe(true);
  }));

  it('should map status colors', () => {
    expect(component.getStatusColor(PostStatus.Draft)).toBe('medium');
    expect(component.getStatusColor(PostStatus.Published)).toBe('success');
  });
});
