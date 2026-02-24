import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ForgotPasswordPage } from './forgot-password.page';
import { AuthService, ToastService } from '../../../core';

describe('ForgotPasswordPage', () => {
  let component: ForgotPasswordPage;
  let fixture: ComponentFixture<ForgotPasswordPage>;
  let authService: jasmine.SpyObj<AuthService>;
  let toastService: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('AuthService', ['forgotPassword']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    await TestBed.configureTestingModule({
      imports: [ForgotPasswordPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ForgotPasswordPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should require email', () => {
    component.form.controls.email.setValue('');
    expect(component.form.controls.email.hasError('required')).toBe(true);
  });

  it('should validate email format', () => {
    component.form.controls.email.setValue('notvalid');
    expect(component.form.controls.email.hasError('email')).toBe(true);
  });

  it('should not submit when form invalid', () => {
    component.onSubmit();
    expect(authService.forgotPassword).not.toHaveBeenCalled();
  });

  it('should call forgotPassword and set emailSent on success', fakeAsync(() => {
    authService.forgotPassword.and.returnValue(of(undefined));
    component.form.controls.email.setValue('test@test.com');

    component.onSubmit();
    tick();

    expect(authService.forgotPassword).toHaveBeenCalledWith({ email: 'test@test.com' });
    expect(component.emailSent()).toBe(true);
    expect(toastService.success).toHaveBeenCalledWith('Reset instructions sent!');
  }));

  it('should set isLoading during submit', fakeAsync(() => {
    authService.forgotPassword.and.returnValue(of(undefined));
    component.form.controls.email.setValue('test@test.com');

    component.onSubmit();
    expect(component.isLoading()).toBe(true);
    tick();
    expect(component.isLoading()).toBe(false);
  }));
});
