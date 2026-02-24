import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { PostModerationPage } from './post-moderation.page';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core';
import { AlertController } from '@ionic/angular/standalone';
import { PostStatus, MediaType, BulkPostAction } from '../../../models';

describe('PostModerationPage', () => {
  let component: PostModerationPage;
  let fixture: ComponentFixture<PostModerationPage>;
  let adminService: jasmine.SpyObj<AdminService>;
  let toast: jasmine.SpyObj<ToastService>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockPost = {
    id: 'p1', title: 'Post 1', description: 'Desc', mediaType: MediaType.Image,
    mediaUrls: ['url.jpg'], thumbnailUrls: ['thumb.jpg'], status: PostStatus.PendingReview,
    createdAt: '2026-01-01T00:00:00Z', submittedAt: '2026-01-01T00:00:00Z',
    creator: { id: 'c1', displayName: 'Creator', isVerified: false, totalPosts: 5, publishedPosts: 3 },
    products: [],
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getPendingPosts', 'moderatePost', 'bulkModeratePosts']);
    adminService.getPendingPosts.and.returnValue(of({ success: true, data: [mockPost], meta: { hasNextPage: false, page: 1, pageSize: 20, totalCount: 1 } }) as any);
    adminService.moderatePost.and.returnValue(of({ success: true, data: { id: 'p1', status: PostStatus.Published, moderationNotes: null, moderatedAt: null, moderatedByUserId: null } }));
    adminService.bulkModeratePosts.and.returnValue(of({ success: true, data: { successCount: 1, failureCount: 0, errors: [] } }));

    toast = jasmine.createSpyObj('ToastService', ['success', 'error']);
    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);
    alertCtrl.create.and.returnValue(Promise.resolve({ present: () => Promise.resolve(), onDidDismiss: () => Promise.resolve({ role: 'cancel' }) } as any));

    await TestBed.configureTestingModule({
      imports: [PostModerationPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ToastService, useValue: toast },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PostModerationPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load posts on init', () => {
    fixture.detectChanges();
    expect(adminService.getPendingPosts).toHaveBeenCalled();
    expect(component.posts().length).toBe(1);
  });

  it('should set isLoading false after load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBeFalse();
  });

  it('should change tab and reload', () => {
    fixture.detectChanges();
    component.onTabChange({ detail: { value: PostStatus.Published } } as any);
    expect(component.activeTab()).toBe(PostStatus.Published);
    expect(adminService.getPendingPosts).toHaveBeenCalledTimes(2);
  });

  it('should toggle selection mode', () => {
    expect(component.selectionMode()).toBeFalse();
    component.toggleSelectionMode();
    expect(component.selectionMode()).toBeTrue();
    component.toggleSelectionMode();
    expect(component.selectionMode()).toBeFalse();
  });

  it('should toggle select a post', () => {
    component.toggleSelect('p1');
    expect(component.selectedIds().has('p1')).toBeTrue();
    component.toggleSelect('p1');
    expect(component.selectedIds().has('p1')).toBeFalse();
  });

  it('should select all posts', () => {
    fixture.detectChanges();
    component.selectAll();
    expect(component.selectedIds().size).toBe(1);
  });

  it('should deselect all posts', () => {
    component.toggleSelect('p1');
    component.deselectAll();
    expect(component.selectedIds().size).toBe(0);
  });

  it('should compute selectedCount', () => {
    component.toggleSelect('p1');
    component.toggleSelect('p2');
    expect(component.selectedCount()).toBe(2);
  });

  it('should show alert on approve', fakeAsync(() => {
    fixture.detectChanges();
    component.onApprove(mockPost as any);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should show alert on reject', fakeAsync(() => {
    fixture.detectChanges();
    component.onReject(mockPost as any);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should show alert on bulk action', fakeAsync(() => {
    component.toggleSelect('p1');
    component.onBulkAction(BulkPostAction.Approve);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should load more posts', () => {
    fixture.detectChanges();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    expect(component.currentPage()).toBe(2);
  });

  it('should return correct status colors', () => {
    expect(component.getStatusColor(PostStatus.Published)).toBe('success');
    expect(component.getStatusColor(PostStatus.PendingReview)).toBe('warning');
    expect(component.getStatusColor(PostStatus.Rejected)).toBe('danger');
    expect(component.getStatusColor(PostStatus.Draft)).toBe('medium');
  });

  it('should open and close review modal', () => {
    component.openReview(mockPost as any);
    expect(component.reviewPost()).toBeTruthy();
    component.onReviewDismissed();
    expect(component.reviewPost()).toBeNull();
  });
});
