import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AdminService } from './admin.service';
import { ApiService } from './api.service';

describe('AdminService', () => {
  let service: AdminService;
  let apiSpy: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    apiSpy = jasmine.createSpyObj('ApiService', ['get', 'post', 'put', 'delete']);
    apiSpy.get.and.returnValue(of({ success: true, data: {} }));
    apiSpy.post.and.returnValue(of({ success: true, data: {} }));

    TestBed.configureTestingModule({
      providers: [{ provide: ApiService, useValue: apiSpy }],
    });
    service = TestBed.inject(AdminService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // Platform stats
  it('should call getPlatformStats', (done) => {
    const stats = { totalUsers: 100 };
    apiSpy.get.and.returnValue(of({ success: true, data: stats } as any));

    service.getPlatformStats().subscribe((result) => {
      expect(result).toEqual(stats as any);
      expect(apiSpy.get).toHaveBeenCalledWith('admin', 'stats');
      done();
    });
  });

  it('should return null from getPlatformStats on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));
    service.getPlatformStats().subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  // Analytics
  it('should call getPlatformAnalytics with date params', () => {
    service.getPlatformAnalytics({ startDate: '2026-01-01', endDate: '2026-01-31' }).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('admin', 'analytics', {
      startDate: '2026-01-01',
      endDate: '2026-01-31',
    });
  });

  it('should return null from getPlatformAnalytics on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));
    service.getPlatformAnalytics().subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  // Posts
  it('should call getPendingPosts with params', () => {
    service.getPendingPosts({ status: 'PendingReview' as any, page: 1, pageSize: 20 }).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('admin', 'posts', {
      status: 'PendingReview',
      page: 1,
      pageSize: 20,
    });
  });

  it('should call getPendingPosts with empty params when none provided', () => {
    service.getPendingPosts().subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('admin', 'posts', {});
  });

  it('should call getPostForModeration', (done) => {
    const post = { id: 'p1' };
    apiSpy.get.and.returnValue(of({ success: true, data: post } as any));

    service.getPostForModeration('p1').subscribe((r) => {
      expect(r).toEqual(post as any);
      expect(apiSpy.get).toHaveBeenCalledWith('admin', 'posts/p1');
      done();
    });
  });

  it('should call moderatePost', () => {
    const req = { action: 'Approve' } as any;
    service.moderatePost('p1', req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('admin', 'posts/p1/moderate', req);
  });

  // Users
  it('should call getUsers with search params', () => {
    service.getUsers({ search: 'john', page: 1, pageSize: 20 } as any).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('admin', 'users', jasmine.objectContaining({
      search: 'john',
      page: 1,
      pageSize: 20,
    }));
  });

  it('should call getUserDetail', (done) => {
    const user = { id: 'u1', email: 'a@b.com' };
    apiSpy.get.and.returnValue(of({ success: true, data: user } as any));

    service.getUserDetail('u1').subscribe((r) => {
      expect(r).toEqual(user as any);
      expect(apiSpy.get).toHaveBeenCalledWith('admin', 'users/u1');
      done();
    });
  });

  it('should return null from getUserDetail on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));
    service.getUserDetail('u1').subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  it('should call suspendUser', () => {
    const req = { reason: 'Spam' } as any;
    service.suspendUser('u1', req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('admin', 'users/u1/suspend', req);
  });

  it('should call unsuspendUser', () => {
    service.unsuspendUser('u1').subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('admin', 'users/u1/unsuspend');
  });

  // Verifications
  it('should call getPendingVerifications with params', () => {
    service.getPendingVerifications({ status: 'Pending' as any, page: 1, pageSize: 10 }).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('admin', 'verifications', {
      status: 'Pending',
      page: 1,
      pageSize: 10,
    });
  });

  it('should call getVerificationDetail', (done) => {
    const ver = { creatorId: 'c1', status: 'Pending' };
    apiSpy.get.and.returnValue(of({ success: true, data: ver } as any));

    service.getVerificationDetail('c1').subscribe((r) => {
      expect(r).toEqual(ver as any);
      done();
    });
  });

  it('should call reviewVerification', () => {
    const req = { decision: 'Approved' } as any;
    service.reviewVerification('c1', req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('admin', 'verifications/c1/review', req);
  });

  // Content reports
  it('should call getContentReports with params', () => {
    service.getContentReports({ status: 'Pending' as any, page: 1, pageSize: 20 }).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('admin', 'reports', jasmine.objectContaining({
      status: 'Pending',
      page: 1,
      pageSize: 20,
    }));
  });

  it('should call getContentReport', (done) => {
    const report = { id: 'r1' };
    apiSpy.get.and.returnValue(of({ success: true, data: report } as any));

    service.getContentReport('r1').subscribe((r) => {
      expect(r).toEqual(report as any);
      expect(apiSpy.get).toHaveBeenCalledWith('admin', 'reports/r1');
      done();
    });
  });

  it('should return null from getContentReport on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));
    service.getContentReport('r1').subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  it('should call reviewContentReport', () => {
    const req = { decision: 'Dismissed' } as any;
    service.reviewContentReport('r1', req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('admin', 'reports/r1/review', req);
  });

  // Moderation queue
  it('should call getModerationQueue with pagination', () => {
    service.getModerationQueue(2, 15).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('admin', 'moderation-queue', { page: 2, pageSize: 15 });
  });

  // Bulk actions
  it('should call bulkModeratePosts', () => {
    const req = { postIds: ['p1', 'p2'], action: 'Approve' } as any;
    service.bulkModeratePosts(req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('admin', 'posts/bulk-moderate', req);
  });

  it('should call bulkSuspendUsers', () => {
    const req = { userIds: ['u1', 'u2'], reason: 'Spam' } as any;
    service.bulkSuspendUsers(req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('admin', 'users/bulk-suspend', req);
  });
});
