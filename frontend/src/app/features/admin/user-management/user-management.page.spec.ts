import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { UserManagementPage } from './user-management.page';
import { AdminService } from '../../../core/services/admin.service';
import { ToastService } from '../../../core';
import { AlertController } from '@ionic/angular/standalone';
import { UserType } from '../../../models';

describe('UserManagementPage', () => {
  let component: UserManagementPage;
  let fixture: ComponentFixture<UserManagementPage>;
  let adminService: jasmine.SpyObj<AdminService>;
  let router: jasmine.SpyObj<Router>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  const mockUser = {
    id: 'u1', email: 'test@test.com', userName: 'testuser', userType: UserType.Shopper,
    isActive: true, emailConfirmed: true, createdAt: '2026-01-01T00:00:00Z',
    suspendedAt: null, suspensionReason: null,
  };

  beforeEach(async () => {
    adminService = jasmine.createSpyObj('AdminService', ['getUsers', 'bulkSuspendUsers']);
    adminService.getUsers.and.returnValue(of({ success: true, data: [mockUser], meta: { hasNextPage: false, page: 1, pageSize: 20, totalCount: 1 } }) as any);
    adminService.bulkSuspendUsers.and.returnValue(of({ success: true, data: { successCount: 1, failureCount: 0, errors: [] } }));

    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');
    const toast = jasmine.createSpyObj('ToastService', ['success', 'error']);
    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);
    alertCtrl.create.and.returnValue(Promise.resolve({ present: () => Promise.resolve() } as any));

    await TestBed.configureTestingModule({
      imports: [UserManagementPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AdminService, useValue: adminService },
        { provide: ToastService, useValue: toast },
        { provide: Router, useValue: router },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(UserManagementPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load users on init', () => {
    fixture.detectChanges();
    expect(adminService.getUsers).toHaveBeenCalled();
    expect(component.users().length).toBe(1);
  });

  it('should debounce search input', fakeAsync(() => {
    fixture.detectChanges();
    adminService.getUsers.calls.reset();
    component.onSearchInput({ detail: { value: 'test' } } as any);
    expect(adminService.getUsers).not.toHaveBeenCalled();
    tick(300);
    expect(adminService.getUsers).toHaveBeenCalled();
  }));

  it('should change type filter and reload', () => {
    fixture.detectChanges();
    adminService.getUsers.calls.reset();
    component.onFilterChange(UserType.Creator);
    expect(component.filterType()).toBe(UserType.Creator);
    expect(adminService.getUsers).toHaveBeenCalled();
  });

  it('should change status filter and reload', () => {
    fixture.detectChanges();
    adminService.getUsers.calls.reset();
    component.onFilterChange('suspended');
    expect(adminService.getUsers).toHaveBeenCalled();
  });

  it('should navigate to user detail', () => {
    component.navigateToUser(mockUser);
    expect(router.navigate).toHaveBeenCalledWith(['/admin/users', 'u1']);
  });

  it('should toggle selection mode', () => {
    expect(component.selectionMode()).toBeFalse();
    component.toggleSelectionMode();
    expect(component.selectionMode()).toBeTrue();
  });

  it('should toggle select user', () => {
    component.toggleSelect('u1');
    expect(component.selectedIds().has('u1')).toBeTrue();
    component.toggleSelect('u1');
    expect(component.selectedIds().has('u1')).toBeFalse();
  });

  it('should select all and deselect all', () => {
    fixture.detectChanges();
    component.selectAll();
    expect(component.selectedIds().size).toBe(1);
    component.deselectAll();
    expect(component.selectedIds().size).toBe(0);
  });

  it('should show alert on bulk suspend', fakeAsync(() => {
    component.toggleSelect('u1');
    component.onBulkSuspend();
    tick();
    expect(alertCtrl.create).toHaveBeenCalled();
  }));

  it('should return correct user type colors', () => {
    expect(component.getUserTypeColor(UserType.Admin)).toBe('danger');
    expect(component.getUserTypeColor(UserType.Creator)).toBe('primary');
    expect(component.getUserTypeColor(UserType.Retailer)).toBe('tertiary');
    expect(component.getUserTypeColor(UserType.Shopper)).toBe('medium');
  });

  it('should load more users', () => {
    fixture.detectChanges();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    expect(component.currentPage()).toBe(2);
  });

  it('should cleanup on destroy', () => {
    fixture.detectChanges();
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});
