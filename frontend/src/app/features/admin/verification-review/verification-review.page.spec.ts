import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { VerificationReviewPage } from './verification-review.page';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core';
import { AlertController } from '@ionic/angular/standalone';
import { VerificationStatus } from '../../../models';

describe('VerificationReviewPage', () => {
  let component: VerificationReviewPage;
  let fixture: ComponentFixture<VerificationReviewPage>;
  let adminService: jasmine.SpyObj<AdminService>;
  let toast: jasmine.SpyObj<ToastService>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockVerification = {
    creatorId: 'c1', displayName: 'Creator 1', bio: 'Bio', socialLinks: { instagram: 'http://ig.com/creator1' },
    status: VerificationStatus.Pending, notes: null, documentUrls: ['http://doc1.pdf'],
    requestedAt: '2026-01-01T00:00:00Z', totalPosts: 10, publishedPosts: 5, createdAt: '2025-12-01T00:00:00Z',
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getPendingVerifications', 'reviewVerification']);
    adminService.getPendingVerifications.and.returnValue(of({ success: true, data: [mockVerification], meta: { hasNextPage: false, page: 1, pageSize: 20, totalCount: 1 } }) as any);
    adminService.reviewVerification.and.returnValue(of({ success: true }));

    toast = jasmine.createSpyObj('ToastService', ['success', 'error']);
    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);
    alertCtrl.create.and.returnValue(Promise.resolve({ present: () => Promise.resolve() } as any));

    await TestBed.configureTestingModule({
      imports: [VerificationReviewPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ToastService, useValue: toast },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(VerificationReviewPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load verifications on init', () => {
    fixture.detectChanges();
    expect(adminService.getPendingVerifications).toHaveBeenCalled();
    expect(component.verifications().length).toBe(1);
  });

  it('should set isLoading false after load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBeFalse();
  });

  it('should change tab and reload', () => {
    fixture.detectChanges();
    adminService.getPendingVerifications.calls.reset();
    component.onTabChange({ detail: { value: VerificationStatus.Approved } } as any);
    expect(component.activeTab()).toBe(VerificationStatus.Approved);
    expect(adminService.getPendingVerifications).toHaveBeenCalled();
    expect(component.expandedId()).toBeNull();
  });

  it('should toggle expandedId', () => {
    component.toggleExpand('c1');
    expect(component.expandedId()).toBe('c1');
    component.toggleExpand('c1');
    expect(component.expandedId()).toBeNull();
  });

  it('should load more', () => {
    fixture.detectChanges();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    expect(component.currentPage()).toBe(2);
  });

  it('should show alert on approve', fakeAsync(() => {
    component.onApprove(mockVerification);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should show alert on reject', fakeAsync(() => {
    component.onReject(mockVerification);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should return correct status colors', () => {
    expect(component.getStatusColor(VerificationStatus.Approved)).toBe('success');
    expect(component.getStatusColor(VerificationStatus.Pending)).toBe('warning');
    expect(component.getStatusColor(VerificationStatus.Rejected)).toBe('danger');
    expect(component.getStatusColor(VerificationStatus.NotSubmitted)).toBe('medium');
  });

  it('should format dates', () => {
    const result = component.formatDate('2026-01-15T00:00:00Z');
    expect(result).toContain('Jan');
    expect(result).toContain('15');
    expect(result).toContain('2026');
  });

  it('should return N/A for null date', () => {
    expect(component.formatDate(null)).toBe('N/A');
  });

  it('should extract social link keys', () => {
    expect(component.getSocialLinkKeys({ instagram: 'url', tiktok: 'url2' })).toEqual(['instagram', 'tiktok']);
    expect(component.getSocialLinkKeys(null)).toEqual([]);
  });
});
