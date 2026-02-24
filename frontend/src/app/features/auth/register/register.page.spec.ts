import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { RegisterPage } from './register.page';
import { AuthService, ToastService, SocialAuthService } from '../../../core';
import { UserType } from '../../../models';

describe('RegisterPage', () => {
  let component: RegisterPage;
  let fixture: ComponentFixture<RegisterPage>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let toastService: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('AuthService', ['register', 'socialLogin'], {
      userType: signal(UserType.Shopper),
      isAuthenticated: signal(false),
    });
    const socialAuthService = jasmine.createSpyObj('SocialAuthService', ['googleSignIn', 'appleSignIn']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    await TestBed.configureTestingModule({
      imports: [RegisterPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: SocialAuthService, useValue: socialAuthService },
        { provide: Router, useValue: router },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterPage);
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
    component.form.controls.email.setValue('bad');
    expect(component.form.controls.email.hasError('email')).toBe(true);
  });

  it('should require password with min length 8', () => {
    component.form.controls.password.setValue('short');
    expect(component.form.controls.password.hasError('minlength')).toBe(true);
  });

  it('should detect password mismatch', () => {
    component.form.controls.password.setValue('password123');
    component.form.controls.confirmPassword.setValue('different');
    component.form.updateValueAndValidity();
    expect(component.form.hasError('passwordMismatch')).toBe(true);
  });

  it('should require acceptPrivacyPolicy to be true', () => {
    component.form.controls.acceptPrivacyPolicy.setValue(false);
    expect(component.form.controls.acceptPrivacyPolicy.hasError('required')).toBe(true);
  });

  it('should not submit when form invalid', () => {
    component.onSubmit();
    expect(authService.register).not.toHaveBeenCalled();
  });

  it('should call register and navigate on valid submit', fakeAsync(() => {
    authService.register.and.returnValue(of({ accessToken: 't', refreshToken: 'r', accessTokenExpiry: '', userId: '1', email: 'test@test.com', userType: UserType.Shopper }));

    component.form.setValue({
      email: 'test@test.com',
      password: 'password123',
      confirmPassword: 'password123',
      acceptPrivacyPolicy: true,
    });
    component.onSubmit();
    tick();

    expect(authService.register).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/onboarding']);
  }));

  it('should toggle password visibility', () => {
    expect(component.showPassword()).toBe(false);
    component.togglePassword();
    expect(component.showPassword()).toBe(true);
  });

  it('should toggle confirm password visibility', () => {
    expect(component.showConfirmPassword()).toBe(false);
    component.toggleConfirmPassword();
    expect(component.showConfirmPassword()).toBe(true);
  });
});
