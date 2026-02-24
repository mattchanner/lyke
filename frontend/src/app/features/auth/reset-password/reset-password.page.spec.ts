import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { ResetPasswordPage } from './reset-password.page';
import { AuthService, ToastService } from '../../../core';

describe('ResetPasswordPage', () => {
  let component: ResetPasswordPage;
  let fixture: ComponentFixture<ResetPasswordPage>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let toastService: jasmine.SpyObj<ToastService>;

  function createComponent(queryParams: Record<string, string> = { email: 'test@test.com', token: 'abc123' }) {
    TestBed.overrideProvider(ActivatedRoute, {
      useValue: { queryParams: of(queryParams) },
    });
    fixture = TestBed.createComponent(ResetPasswordPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(async () => {
    authService = jasmine.createSpyObj('AuthService', ['resetPassword']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    await TestBed.configureTestingModule({
      imports: [ResetPasswordPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({ email: 'test@test.com', token: 'abc123' }) } },
      ],
    }).compileComponents();
  });

  it('should create with valid params', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  it('should redirect when params missing', fakeAsync(() => {
    createComponent({});
    tick();
    expect(router.navigate).toHaveBeenCalledWith(['/auth/forgot-password']);
  }));

  it('should have newPassword and confirmPassword controls', () => {
    createComponent();
    expect(component.form.contains('newPassword')).toBe(true);
    expect(component.form.contains('confirmPassword')).toBe(true);
  });

  it('should require newPassword min length 8', () => {
    createComponent();
    component.form.controls.newPassword.setValue('short');
    expect(component.form.controls.newPassword.hasError('minlength')).toBe(true);
  });

  it('should detect password mismatch', () => {
    createComponent();
    component.form.controls.newPassword.setValue('password123');
    component.form.controls.confirmPassword.setValue('different');
    component.form.updateValueAndValidity();
    expect(component.form.hasError('passwordMismatch')).toBe(true);
  });

  it('should not submit when form invalid', () => {
    createComponent();
    component.onSubmit();
    expect(authService.resetPassword).not.toHaveBeenCalled();
  });

  it('should call resetPassword on valid submit', fakeAsync(() => {
    createComponent();
    authService.resetPassword.and.returnValue(of(undefined));

    component.form.setValue({ newPassword: 'newpass123', confirmPassword: 'newpass123' });
    component.onSubmit();
    tick();

    expect(authService.resetPassword).toHaveBeenCalledWith({
      email: 'test@test.com',
      token: 'abc123',
      newPassword: 'newpass123',
      confirmPassword: 'newpass123',
    });
    expect(component.resetComplete()).toBe(true);
  }));

  it('should set isLoading during submit', fakeAsync(() => {
    createComponent();
    authService.resetPassword.and.returnValue(of(undefined));
    component.form.setValue({ newPassword: 'newpass123', confirmPassword: 'newpass123' });

    component.onSubmit();
    expect(component.isLoading()).toBe(true);
    tick();
    expect(component.isLoading()).toBe(false);
  }));

  it('should toggle password visibility', () => {
    createComponent();
    expect(component.showPassword()).toBe(false);
    component.togglePassword();
    expect(component.showPassword()).toBe(true);
  });

  it('should toggle confirm password visibility', () => {
    createComponent();
    expect(component.showConfirmPassword()).toBe(false);
    component.toggleConfirmPassword();
    expect(component.showConfirmPassword()).toBe(true);
  });

  it('should show success toast on reset', fakeAsync(() => {
    createComponent();
    authService.resetPassword.and.returnValue(of(undefined));
    component.form.setValue({ newPassword: 'newpass123', confirmPassword: 'newpass123' });
    component.onSubmit();
    tick();
    expect(toastService.success).toHaveBeenCalledWith('Password reset successfully!');
  }));
});
