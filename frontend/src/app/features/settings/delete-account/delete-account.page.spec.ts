import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { DeleteAccountPage } from './delete-account.page';
import { AuthService, ToastService } from '../../../core/services';
import { AlertController } from '@ionic/angular/standalone';

describe('DeleteAccountPage', () => {
  let component: DeleteAccountPage;
  let fixture: ComponentFixture<DeleteAccountPage>;
  let authService: jasmine.SpyObj<AuthService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let alertCtrl: jasmine.SpyObj<AlertController>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('AuthService', ['deleteAccount', 'logout']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning']);
    alertCtrl = jasmine.createSpyObj('AlertController', ['create']);

    await TestBed.configureTestingModule({
      imports: [DeleteAccountPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: ToastService, useValue: toastService },
        { provide: AlertController, useValue: alertCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeleteAccountPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should compute isConfirmed when text is DELETE', () => {
    expect(component.isConfirmed()).toBe(false);
    component['confirmationText'].set('DELETE');
    expect(component.isConfirmed()).toBe(true);
  });

  it('should be case insensitive for confirmation', () => {
    component['confirmationText'].set('delete');
    expect(component.isConfirmed()).toBe(true);
  });

  it('should show confirmation alert on onDelete', async () => {
    const mockAlert = { present: jasmine.createSpy().and.returnValue(Promise.resolve()) };
    alertCtrl.create.and.returnValue(Promise.resolve(mockAlert as any));

    await component.onDelete();

    expect(alertCtrl.create).toHaveBeenCalled();
    expect(mockAlert.present).toHaveBeenCalled();
  });

  it('should call deleteAccount and logout on performDeletion', fakeAsync(() => {
    authService.deleteAccount.and.returnValue(of(undefined));
    authService.logout.and.returnValue(Promise.resolve());
    toastService.success.and.returnValue(Promise.resolve() as any);

    (component as any).performDeletion();
    tick();

    expect(authService.deleteAccount).toHaveBeenCalled();
    expect(toastService.success).toHaveBeenCalledWith('Your account has been deleted');
  }));
});
