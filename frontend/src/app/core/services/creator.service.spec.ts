import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CreatorService } from './creator.service';
import { ApiService } from './api.service';
import { ApiResponse } from '../../models';

describe('CreatorService', () => {
  let service: CreatorService;
  let apiSpy: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    apiSpy = jasmine.createSpyObj('ApiService', ['get', 'post', 'put', 'delete']);
    apiSpy.get.and.returnValue(of({ success: true, data: {} }));
    apiSpy.post.and.returnValue(of({ success: true, data: {} }));
    apiSpy.put.and.returnValue(of({ success: true, data: {} }));
    apiSpy.delete.and.returnValue(of({ success: true }));

    TestBed.configureTestingModule({
      providers: [{ provide: ApiService, useValue: apiSpy }],
    });
    service = TestBed.inject(CreatorService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call register with creators resource', () => {
    const req = { displayName: 'Test', bio: 'Bio' } as any;
    service.register(req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('creators', 'register', req);
  });

  it('should call getProfile and return data on success', (done) => {
    const profile = { id: '1', displayName: 'Creator' };
    apiSpy.get.and.returnValue(of({ success: true, data: profile } as any));

    service.getProfile().subscribe((result) => {
      expect(result).toEqual(profile as any);
      expect(apiSpy.get).toHaveBeenCalledWith('creators', 'profile');
      done();
    });
  });

  it('should return null from getProfile on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));

    service.getProfile().subscribe((result) => {
      expect(result).toBeNull();
      done();
    });
  });

  it('should call updateProfile with PUT', () => {
    const req = { displayName: 'New Name' } as any;
    service.updateProfile(req).subscribe();
    expect(apiSpy.put).toHaveBeenCalledWith('creators', 'profile', req);
  });

  it('should call getPosts with no params when request is empty', () => {
    service.getPosts().subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('creators', 'posts', {});
  });

  it('should call getPosts with status/page/pageSize params', () => {
    service.getPosts({ status: 'Published' as any, page: 2, pageSize: 10 }).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('creators', 'posts', {
      status: 'Published',
      page: 2,
      pageSize: 10,
    });
  });

  it('should call getPost and return data', (done) => {
    const post = { id: 'p1', title: 'Post' };
    apiSpy.get.and.returnValue(of({ success: true, data: post } as any));

    service.getPost('p1').subscribe((result) => {
      expect(result).toEqual(post as any);
      expect(apiSpy.get).toHaveBeenCalledWith('creators', 'posts/p1');
      done();
    });
  });

  it('should return null from getPost on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));

    service.getPost('p1').subscribe((result) => {
      expect(result).toBeNull();
      done();
    });
  });

  it('should call createPost with POST', () => {
    const req = { title: 'New Post' } as any;
    service.createPost(req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('creators', 'posts', req);
  });

  it('should call updatePost with PUT', () => {
    const req = { title: 'Updated' } as any;
    service.updatePost('p1', req).subscribe();
    expect(apiSpy.put).toHaveBeenCalledWith('creators', 'posts/p1', req);
  });

  it('should call deletePost with DELETE', () => {
    service.deletePost('p1').subscribe();
    expect(apiSpy.delete).toHaveBeenCalledWith('creators', 'posts/p1');
  });

  it('should call submitPost with POST', () => {
    service.submitPost('p1').subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('creators', 'posts/p1/submit');
  });

  it('should call getAnalytics with date params', (done) => {
    const analytics = { totalViews: 100 };
    apiSpy.get.and.returnValue(of({ success: true, data: analytics } as any));

    service.getAnalytics({ startDate: '2026-01-01', endDate: '2026-01-31' }).subscribe((result) => {
      expect(result).toEqual(analytics as any);
      expect(apiSpy.get).toHaveBeenCalledWith('creators', 'analytics', {
        startDate: '2026-01-01',
        endDate: '2026-01-31',
      });
      done();
    });
  });

  it('should return null from getAnalytics on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));

    service.getAnalytics().subscribe((result) => {
      expect(result).toBeNull();
      done();
    });
  });

  it('should call getEarnings and return data', (done) => {
    const earnings = { totalEarned: 500 };
    apiSpy.get.and.returnValue(of({ success: true, data: earnings } as any));

    service.getEarnings().subscribe((result) => {
      expect(result).toEqual(earnings as any);
      expect(apiSpy.get).toHaveBeenCalledWith('creators', 'earnings');
      done();
    });
  });

  it('should return null from getEarnings on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));

    service.getEarnings().subscribe((result) => {
      expect(result).toBeNull();
      done();
    });
  });

  it('should call getEarningsHistory with params', () => {
    service.getEarningsHistory({ status: 'Paid' as any, page: 1, pageSize: 10 }).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('creators', 'earnings/history', jasmine.objectContaining({
      status: 'Paid',
      page: 1,
      pageSize: 10,
    }));
  });

  it('should call getVerificationStatus', (done) => {
    const status = { status: 'Pending' };
    apiSpy.get.and.returnValue(of({ success: true, data: status } as any));

    service.getVerificationStatus().subscribe((result) => {
      expect(result).toEqual(status as any);
      done();
    });
  });

  it('should call submitVerification with POST', () => {
    const req = { documentUrls: ['url1'] } as any;
    service.submitVerification(req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('creators', 'verification', req);
  });
});
