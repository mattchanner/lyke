import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { ContentReportsPage } from './content-reports.page';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core';
import { AlertController } from '@ionic/angular/standalone';
import { ReportStatus, ReportReason } from '../../../models';

describe('ContentReportsPage', () => {
  let component: ContentReportsPage;
  let fixture: ComponentFixture<ContentReportsPage>;
  let adminService: jasmine.SpyObj<AdminService>;
  let toast: jasmine.SpyObj<ToastService>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockReport = {
    id: 'r1', postId: 'p1', postTitle: 'Post 1', reportedByUserId: 'u1',
    reason: ReportReason.InappropriateContent, additionalDetails: 'Details',
    status: ReportStatus.Pending, reviewedByUserId: null, reviewedAt: null,
    reviewNotes: null, createdAt: '2026-01-01T00:00:00Z',
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getContentReports', 'reviewContentReport']);
    adminService.getContentReports.and.returnValue(of({ success: true, data: [mockReport], meta: { hasNextPage: false, page: 1, pageSize: 20, totalCount: 1 } }) as any);
    adminService.reviewContentReport.and.returnValue(of({ success: true, data: { ...mockReport, status: ReportStatus.Dismissed } }));

    toast = jasmine.createSpyObj('ToastService', ['success', 'error']);
    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);
    alertCtrl.create.and.returnValue(Promise.resolve({ present: () => Promise.resolve() } as any));

    await TestBed.configureTestingModule({
      imports: [ContentReportsPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ToastService, useValue: toast },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ContentReportsPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load reports on init', () => {
    fixture.detectChanges();
    expect(adminService.getContentReports).toHaveBeenCalled();
    expect(component.reports().length).toBe(1);
  });

  it('should set isLoading false after load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBeFalse();
  });

  it('should change tab and reload', () => {
    fixture.detectChanges();
    adminService.getContentReports.calls.reset();
    component.onTabChange({ detail: { value: ReportStatus.Dismissed } } as any);
    expect(component.activeStatusTab()).toBe(ReportStatus.Dismissed);
    expect(adminService.getContentReports).toHaveBeenCalled();
  });

  it('should change reason filter and reload', () => {
    fixture.detectChanges();
    adminService.getContentReports.calls.reset();
    component.onReasonFilterChange(ReportReason.Spam);
    expect(component.activeReasonFilter()).toBe(ReportReason.Spam);
    expect(adminService.getContentReports).toHaveBeenCalled();
  });

  it('should load more reports', () => {
    fixture.detectChanges();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    expect(component.currentPage()).toBe(2);
  });

  it('should show alert on dismiss', fakeAsync(() => {
    fixture.detectChanges();
    component.onDismiss(mockReport);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should show alert on take action', fakeAsync(() => {
    fixture.detectChanges();
    component.onTakeAction(mockReport);
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should clear reviewPost and modal on onReviewDismissed', () => {
    component.reviewPost.set({ id: 'p1' } as any);
    component.reviewModalOpen.set(true);
    component.onReviewDismissed();
    expect(component.reviewPost()).toBeNull();
    expect(component.reviewModalOpen()).toBeFalse();
  });

  it('should return correct status colors', () => {
    expect(component.getStatusColor(ReportStatus.Pending)).toBe('warning');
    expect(component.getStatusColor(ReportStatus.Dismissed)).toBe('medium');
    expect(component.getStatusColor(ReportStatus.ActionTaken)).toBe('success');
  });

  it('should return correct reason colors', () => {
    expect(component.getReasonColor(ReportReason.InappropriateContent)).toBe('warning');
    expect(component.getReasonColor(ReportReason.Spam)).toBe('tertiary');
    expect(component.getReasonColor(ReportReason.Copyright)).toBe('primary');
  });

  it('should reset page on refresh', () => {
    component.currentPage.set(3);
    fixture.detectChanges();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.onRefresh(event);
    expect(component.currentPage()).toBe(1);
  });

  it('should set hasMore from meta', () => {
    adminService.getContentReports.and.returnValue(of({ success: true, data: [mockReport], meta: { hasNextPage: true, page: 1, pageSize: 20, totalCount: 30 } }) as any);
    fixture.detectChanges();
    expect(component.hasMore()).toBeTrue();
  });
});
