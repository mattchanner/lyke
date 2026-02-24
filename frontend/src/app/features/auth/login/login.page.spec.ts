import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginPage } from './login.page';
import { AuthService, ToastService, SocialAuthService } from '../../../core';
import { UserType } from '../../../models';

describe('LoginPage', () => {
  let component: LoginPage;
  let fixture: ComponentFixture<LoginPage>;
  let authService: jasmine.SpyObj<AuthService>;
  let socialAuthService: jasmine.SpyObj<SocialAuthService>;
  let router: jasmine.SpyObj<Router>;
  let toastService: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('AuthService', ['login', 'socialLogin'], {
      userType: signal(UserType.Shopper),
      isAuthenticated: signal(false),
      isLoading: signal(false),
    });
    socialAuthService = jasmine.createSpyObj('SocialAuthService', ['googleSignIn', 'appleSignIn']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    await TestBed.configureTestingModule({
      imports: [LoginPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: SocialAuthService, useValue: socialAuthService },
        { provide: Router, useValue: router },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoginPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have form with email and password controls', () => {
    expect(component.form.contains('email')).toBe(true);
    expect(component.form.contains('password')).toBe(true);
  });

  it('should require email', () => {
    component.form.controls.email.setValue('');
    expect(component.form.controls.email.hasError('required')).toBe(true);
  });

  it('should validate email format', () => {
    component.form.controls.email.setValue('notanemail');
    expect(component.form.controls.email.hasError('email')).toBe(true);
  });

  it('should require password', () => {
    component.form.controls.password.setValue('');
    expect(component.form.controls.password.hasError('required')).toBe(true);
  });

  it('should toggle showPassword on togglePassword', () => {
    expect(component.showPassword()).toBe(false);
    component.togglePassword();
    expect(component.showPassword()).toBe(true);
    component.togglePassword();
    expect(component.showPassword()).toBe(false);
  });

  it('should not submit when form is invalid', () => {
    component.onSubmit();
    expect(authService.login).not.toHaveBeenCalled();
  });

  it('should call login and navigate on valid submit', fakeAsync(() => {
    authService.login.and.returnValue(of({ accessToken: 'token', refreshToken: 'ref', accessTokenExpiry: '', userId: '1', email: 'test@test.com', userType: UserType.Shopper }));

    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    tick();

    expect(authService.login).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/feed']);
  }));

  it('should navigate to /creator for Creator userType', fakeAsync(() => {
    (authService.userType as any) = signal(UserType.Creator);
    authService.login.and.returnValue(of({ accessToken: 'token', refreshToken: 'ref', accessTokenExpiry: '', userId: '1', email: 'test@test.com', userType: UserType.Creator }));

    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    tick();

    expect(router.navigate).toHaveBeenCalledWith(['/creator']);
  }));

  it('should navigate to /admin for Admin userType', fakeAsync(() => {
    (authService.userType as any) = signal(UserType.Admin);
    authService.login.and.returnValue(of({ accessToken: 'token', refreshToken: 'ref', accessTokenExpiry: '', userId: '1', email: 'test@test.com', userType: UserType.Admin }));

    component.form.setValue({ email: 'test@test.com', password: 'password123' });
    component.onSubmit();
    tick();

    expect(router.navigate).toHaveBeenCalledWith(['/admin']);
  }));

  it('should set isLoading during submit', fakeAsync(() => {
    authService.login.and.returnValue(of({ accessToken: 'token', refreshToken: 'ref', accessTokenExpiry: '', userId: '1', email: 'test@test.com', userType: UserType.Shopper }));
    component.form.setValue({ email: 'test@test.com', password: 'password123' });

    component.onSubmit();
    expect(component.isLoading()).toBe(true);
    tick();
    expect(component.isLoading()).toBe(false);
  }));

  it('should show toast on Google sign-in error', fakeAsync(() => {
    socialAuthService.googleSignIn.and.returnValue(Promise.reject('error'));

    component.onGoogleSignIn();
    tick();

    expect(toastService.error).toHaveBeenCalledWith('Google sign-in failed');
  }));

  it('should show toast on Apple sign-in error', fakeAsync(() => {
    socialAuthService.appleSignIn.and.returnValue(Promise.reject('error'));

    component.onAppleSignIn();
    tick();

    expect(toastService.error).toHaveBeenCalledWith('Apple sign-in failed');
  }));
});
