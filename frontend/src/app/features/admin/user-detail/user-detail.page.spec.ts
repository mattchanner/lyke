import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { UserDetailPage } from './user-detail.page';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core';
import { AlertController } from '@ionic/angular/standalone';
import { UserType, VerificationStatus } from '../../../models';

describe('UserDetailPage', () => {
  let component: UserDetailPage;
  let fixture: ComponentFixture<UserDetailPage>;
  let adminService: jasmine.SpyObj<AdminService>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockUser = {
    id: 'u1', email: 'test@test.com', userName: 'testuser', userType: UserType.Creator,
    isActive: true, emailConfirmed: true, createdAt: '2026-01-01T00:00:00Z',
    updatedAt: null, suspendedAt: null, suspendedByUserId: null, suspensionReason: null,
    hasBodyProfile: true, isCreator: true,
    creatorInfo: { creatorId: 'c1', displayName: 'Creator', isVerified: true, verificationStatus: VerificationStatus.Approved, totalPosts: 10, publishedPosts: 5 },
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getUserDetail', 'suspendUser', 'unsuspendUser']);
    adminService.getUserDetail.and.returnValue(of(mockUser));
    adminService.suspendUser.and.returnValue(of({ success: true, data: { userId: 'u1', isActive: false, suspendedAt: '2026-01-01T00:00:00Z', suspendedByUserId: 'admin1', suspensionReason: 'Violation' } }));
    adminService.unsuspendUser.and.returnValue(of({ success: true, data: { userId: 'u1', isActive: true, suspendedAt: null, suspendedByUserId: null, suspensionReason: null } }));

    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);
    alertCtrl.create.and.returnValue(Promise.resolve({ present: () => Promise.resolve() } as any));

    await TestBed.configureTestingModule({
      imports: [UserDetailPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ToastService, useValue: jasmine.createSpyObj('ToastService', ['success', 'error']) },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'u1' } } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserDetailPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load user on init', () => {
    fixture.detectChanges();
    expect(adminService.getUserDetail).toHaveBeenCalledWith('u1');
    expect(component.user()).toEqual(mockUser);
  });

  it('should set isLoading false after load', () => {
    fixture.detectChanges();
    expect(component.isLoading()).toBeFalse();
  });

  it('should show alert on suspend', fakeAsync(() => {
    fixture.detectChanges();
    component.onSuspend();
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should show alert on unsuspend', fakeAsync(() => {
    fixture.detectChanges();
    component.onUnsuspend();
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should return correct user type colors', () => {
    expect(component.getUserTypeColor(UserType.Admin)).toBe('danger');
    expect(component.getUserTypeColor(UserType.Creator)).toBe('primary');
    expect(component.getUserTypeColor(UserType.Retailer)).toBe('tertiary');
    expect(component.getUserTypeColor(UserType.Shopper)).toBe('medium');
  });

  it('should return correct verification colors', () => {
    expect(component.getVerificationColor(VerificationStatus.Approved)).toBe('success');
    expect(component.getVerificationColor(VerificationStatus.Pending)).toBe('warning');
    expect(component.getVerificationColor(VerificationStatus.Rejected)).toBe('danger');
    expect(component.getVerificationColor(VerificationStatus.NotSubmitted)).toBe('medium');
  });
});
