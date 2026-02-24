import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { ModerationQueuePage } from './moderation-queue.page';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core';
import { AlertController } from '@ionic/angular/standalone';
import { ModerationQueueItemResponse, PostStatus, MediaType, ReportReason } from '../../../models';

describe('ModerationQueuePage', () => {
  let component: ModerationQueuePage;
  let fixture: ComponentFixture<ModerationQueuePage>;
  let adminService: jasmine.SpyObj<AdminService>;
  let toast: jasmine.SpyObj<ToastService>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockItem: ModerationQueueItemResponse = {
    id: 'post-1',
    title: 'Test Post',
    description: 'Description',
    mediaType: MediaType.Image,
    mediaUrls: ['url1.jpg'],
    thumbnailUrls: ['thumb1.jpg'],
    status: PostStatus.PendingReview,
    createdAt: '2026-01-01T00:00:00Z',
    submittedAt: '2026-01-01T00:00:00Z',
    creator: { id: 'c1', displayName: 'Creator 1', isVerified: true, totalPosts: 10, publishedPosts: 5 },
    products: [],
    reportCount: 3,
    topReportReason: ReportReason.InappropriateContent,
    isFlagged: true,
    priority: 8,
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getModerationQueue', 'moderatePost']);
    adminService.getModerationQueue.and.returnValue(of({ success: true, data: [mockItem], meta: { hasNextPage: false, page: 1, pageSize: 20, totalCount: 1 } }) as any);
    adminService.moderatePost.and.returnValue(of({ success: true, data: { id: 'post-1', status: PostStatus.Published, moderationNotes: null, moderatedAt: null, moderatedByUserId: null } }));

    toast = jasmine.createSpyObj('ToastService', ['success', 'error']);

    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);
    alertCtrl.create.and.returnValue(Promise.resolve({
      present: () => Promise.resolve(),
      onDidDismiss: () => Promise.resolve({ role: 'cancel' }),
    } as any));

    await TestBed.configureTestingModule({
      imports: [ModerationQueuePage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ToastService, useValue: toast },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ModerationQueuePage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load queue on init', () => {
    fixture.detectChanges();
    expect(adminService.getModerationQueue).toHaveBeenCalledWith(1, 20);
    expect(component.items().length).toBe(1);
  });

  it('should set isLoading false after load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBeFalse();
  });

  it('should toggle expandedId', () => {
    expect(component.expandedId()).toBeNull();
    component.toggleExpand('post-1');
    expect(component.expandedId()).toBe('post-1');
    component.toggleExpand('post-1');
    expect(component.expandedId()).toBeNull();
  });

  it('should load more items', () => {
    fixture.detectChanges();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    expect(component.currentPage()).toBe(2);
    expect(adminService.getModerationQueue).toHaveBeenCalledWith(2, 20);
  });

  it('should reset on refresh', () => {
    component.currentPage.set(3);
    fixture.detectChanges();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.onRefresh(event);
    expect(component.currentPage()).toBe(1);
  });

  it('should show alert on approve', fakeAsync(() => {
    fixture.detectChanges();
    component.onApprove(mockItem);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should show alert on reject', fakeAsync(() => {
    fixture.detectChanges();
    component.onReject(mockItem);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should return correct priority levels', () => {
    expect(component.getPriorityLevel(80)).toBe('high');
    expect(component.getPriorityLevel(50)).toBe('medium');
    expect(component.getPriorityLevel(10)).toBe('low');
  });

  it('should return correct status colors', () => {
    expect(component.getStatusColor(PostStatus.Published)).toBe('success');
    expect(component.getStatusColor(PostStatus.PendingReview)).toBe('warning');
    expect(component.getStatusColor(PostStatus.Rejected)).toBe('medium');
  });

  it('should open and close review modal', () => {
    component.openReview(mockItem);
    expect(component.reviewItem()).toBeTruthy();
    component.onReviewDismissed();
    expect(component.reviewItem()).toBeNull();
  });

  it('should set hasMore from response meta', () => {
    adminService.getModerationQueue.and.returnValue(of({ success: true, data: [mockItem], meta: { hasNextPage: true, page: 1, pageSize: 20, totalCount: 30 } }) as any);
    fixture.detectChanges();
    expect(component.hasMore()).toBeTrue();
  });
});
