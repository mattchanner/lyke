import { TestBed, fakeAsync, flushMicrotasks, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { ToastService } from './toast.service';
import { AppInsightsService } from './app-insights.service';
import { UserType } from '../../models';

// Helper to create a fake JWT (base64-encoded payload)
function fakeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  const body = btoa(JSON.stringify(payload));
  const sig = btoa('signature');
  return `${header}.${body}.${sig}`;
}

describe('AuthService', () => {
  let service: AuthService;
  let apiSpy: jasmine.SpyObj<ApiService>;
  let storageSpy: jasmine.SpyObj<StorageService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let toastSpy: jasmine.SpyObj<ToastService>;
  let appInsightsSpy: jasmine.SpyObj<AppInsightsService>;

  function setup(storageOverrides?: {
    accessToken?: string | null;
    refreshToken?: string | null;
    tokenExpiry?: string | null;
  }) {
    const defaults = {
      accessToken: null,
      refreshToken: null,
      tokenExpiry: null,
    };
    const config = { ...defaults, ...storageOverrides };

    apiSpy = jasmine.createSpyObj('ApiService', ['get', 'post', 'put', 'delete']);
    apiSpy.get.and.returnValue(of({ success: true, data: {} }));
    apiSpy.post.and.returnValue(of({ success: true, data: {} }));
    apiSpy.delete.and.returnValue(of({ success: true }));

    storageSpy = jasmine.createSpyObj('StorageService', [
      'getAccessToken', 'setAccessToken',
      'getRefreshToken', 'setRefreshToken',
      'getTokenExpiry', 'setTokenExpiry',
      'getUserId', 'setUserId',
      'clearAuthData',
    ]);
    storageSpy.getAccessToken.and.returnValue(Promise.resolve(config.accessToken));
    storageSpy.getRefreshToken.and.returnValue(Promise.resolve(config.refreshToken));
    storageSpy.getTokenExpiry.and.returnValue(Promise.resolve(config.tokenExpiry));
    storageSpy.getUserId.and.returnValue(Promise.resolve(null));
    storageSpy.setAccessToken.and.returnValue(Promise.resolve());
    storageSpy.setRefreshToken.and.returnValue(Promise.resolve());
    storageSpy.setTokenExpiry.and.returnValue(Promise.resolve());
    storageSpy.setUserId.and.returnValue(Promise.resolve());
    storageSpy.clearAuthData.and.returnValue(Promise.resolve());

    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    routerSpy.navigate.and.returnValue(Promise.resolve(true));

    toastSpy = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    appInsightsSpy = jasmine.createSpyObj('AppInsightsService', [
      'trackEvent', 'trackException', 'setAuthenticatedUser', 'clearAuthenticatedUser',
    ]);

    TestBed.configureTestingModule({
      providers: [
        { provide: ApiService, useValue: apiSpy },
        { provide: StorageService, useValue: storageSpy },
        { provide: Router, useValue: routerSpy },
        { provide: ToastService, useValue: toastSpy },
        { provide: AppInsightsService, useValue: appInsightsSpy },
      ],
    });

    service = TestBed.inject(AuthService);
  }

  // Initialization tests
  describe('initialization', () => {
    it('should start with isLoading true', () => {
      setup();
      // Before async init resolves, isLoading is true
      expect(service.isLoading()).toBeTrue();
    });

    it('should resolve to unauthenticated when no stored token', fakeAsync(() => {
      setup();
      flushMicrotasks();

      expect(service.isLoading()).toBeFalse();
      expect(service.isAuthenticated()).toBeFalse();
      expect(service.userId()).toBeNull();
    }));

    it('should restore state from valid stored JWT', fakeAsync(() => {
      const futureExpiry = new Date(Date.now() + 3600 * 1000).toISOString();
      const token = fakeJwt({ sub: 'user1', email: 'a@b.com', user_type: 'Creator', exp: Date.now() / 1000 + 3600 });

      setup({ accessToken: token, tokenExpiry: futureExpiry });
      flushMicrotasks();
      tick(100);

      expect(service.isAuthenticated()).toBeTrue();
      expect(service.userId()).toBe('user1');
      expect(service.email()).toBe('a@b.com');
      expect(service.userType()).toBe('Creator' as UserType);
      expect(appInsightsSpy.setAuthenticatedUser).toHaveBeenCalledWith('user1');
    }));

    it('should try refresh when token is expired', fakeAsync(() => {
      const pastExpiry = new Date(Date.now() - 1000).toISOString();
      const token = fakeJwt({ sub: 'user1', email: 'a@b.com', user_type: 'Creator', exp: Date.now() / 1000 - 100 });

      const authResponse = {
        userId: 'user1',
        email: 'a@b.com',
        userType: UserType.Creator,
        accessToken: 'newToken',
        refreshToken: 'newRefresh',
        accessTokenExpiry: new Date(Date.now() + 3600 * 1000).toISOString(),
      };

      setup({ accessToken: token, refreshToken: 'oldRefresh', tokenExpiry: pastExpiry });
      apiSpy.post.and.returnValue(of({ success: true, data: authResponse }));

      flushMicrotasks();
      tick(100);

      expect(apiSpy.post).toHaveBeenCalledWith('auth', 'refresh', jasmine.objectContaining({ refreshToken: 'oldRefresh' }));
    }));

    it('should clear state when no refresh token and token expired', fakeAsync(() => {
      const pastExpiry = new Date(Date.now() - 1000).toISOString();
      const token = fakeJwt({ sub: 'user1', email: 'a@b.com', user_type: 'Creator', exp: Date.now() / 1000 - 100 });

      setup({ accessToken: token, refreshToken: null, tokenExpiry: pastExpiry });
      flushMicrotasks();

      expect(service.isAuthenticated()).toBeFalse();
      expect(service.isLoading()).toBeFalse();
    }));
  });

  // Computed signals
  describe('computed signals', () => {
    it('should compute isCreator correctly', fakeAsync(() => {
      const futureExpiry = new Date(Date.now() + 3600 * 1000).toISOString();
      const token = fakeJwt({ sub: 'u1', email: 'a@b.com', user_type: 'Creator', exp: Date.now() / 1000 + 3600 });

      setup({ accessToken: token, tokenExpiry: futureExpiry });
      flushMicrotasks();
      tick(100);

      expect(service.isCreator()).toBeTrue();
      expect(service.isAdmin()).toBeFalse();
      expect(service.isShopper()).toBeFalse();
    }));

    it('should compute isAdmin correctly', fakeAsync(() => {
      const futureExpiry = new Date(Date.now() + 3600 * 1000).toISOString();
      const token = fakeJwt({ sub: 'u1', email: 'a@b.com', user_type: 'Admin', exp: Date.now() / 1000 + 3600 });

      setup({ accessToken: token, tokenExpiry: futureExpiry });
      flushMicrotasks();
      tick(100);

      expect(service.isAdmin()).toBeTrue();
      expect(service.isCreator()).toBeFalse();
    }));
  });

  // Auth flows
  describe('login', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should call auth/login and store tokens', fakeAsync(() => {
      const authResponse = {
        userId: 'u1',
        email: 'a@b.com',
        userType: UserType.Shopper,
        accessToken: 'at',
        refreshToken: 'rt',
        accessTokenExpiry: new Date(Date.now() + 3600 * 1000).toISOString(),
      };
      apiSpy.post.and.returnValue(of({ success: true, data: authResponse }));

      let result: any;
      service.login({ email: 'a@b.com', password: 'pass' }).subscribe((r) => (result = r));
      flushMicrotasks();
      tick(100);

      expect(apiSpy.post).toHaveBeenCalledWith('auth', 'login', { email: 'a@b.com', password: 'pass' });
      expect(storageSpy.setAccessToken).toHaveBeenCalledWith('at');
      expect(storageSpy.setRefreshToken).toHaveBeenCalledWith('rt');
      expect(result.userId).toBe('u1');
      expect(service.isAuthenticated()).toBeTrue();
    }));

    it('should throw on login failure', fakeAsync(() => {
      apiSpy.post.and.returnValue(of({ success: false, error: { code: 'ERR', message: 'Bad creds' } }));

      let error: any;
      service.login({ email: 'a@b.com', password: 'wrong' }).subscribe({
        error: (e) => (error = e),
      });
      flushMicrotasks();

      expect(error.message).toBe('Bad creds');
    }));
  });

  describe('register', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should call auth/register and store tokens', fakeAsync(() => {
      const authResponse = {
        userId: 'u1',
        email: 'a@b.com',
        userType: UserType.Shopper,
        accessToken: 'at',
        refreshToken: 'rt',
        accessTokenExpiry: new Date(Date.now() + 3600 * 1000).toISOString(),
      };
      apiSpy.post.and.returnValue(of({ success: true, data: authResponse }));

      service.register({ email: 'a@b.com', password: 'pass', confirmPassword: 'pass' }).subscribe();
      flushMicrotasks();
      tick(100);

      expect(apiSpy.post).toHaveBeenCalledWith('auth', 'register', jasmine.objectContaining({ email: 'a@b.com' }));
      expect(storageSpy.setAccessToken).toHaveBeenCalled();
    }));
  });

  describe('socialLogin', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should call auth/social-login', fakeAsync(() => {
      const authResponse = {
        userId: 'u1',
        email: 'a@b.com',
        userType: UserType.Shopper,
        accessToken: 'at',
        refreshToken: 'rt',
        accessTokenExpiry: new Date(Date.now() + 3600 * 1000).toISOString(),
      };
      apiSpy.post.and.returnValue(of({ success: true, data: authResponse }));

      service.socialLogin({ provider: 'Google', idToken: 'tok' }).subscribe();
      flushMicrotasks();
      tick(100);

      expect(apiSpy.post).toHaveBeenCalledWith('auth', 'social-login', { provider: 'Google', idToken: 'tok' });
    }));
  });

  // Token refresh
  describe('refreshToken', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should call auth/refresh endpoint', fakeAsync(() => {
      const authResponse = {
        userId: 'u1',
        email: 'a@b.com',
        userType: UserType.Shopper,
        accessToken: 'newAt',
        refreshToken: 'newRt',
        accessTokenExpiry: new Date(Date.now() + 3600 * 1000).toISOString(),
      };
      apiSpy.post.and.returnValue(of({ success: true, data: authResponse }));

      service.refreshToken('oldRt').subscribe();
      flushMicrotasks();
      tick(100);

      expect(apiSpy.post).toHaveBeenCalledWith('auth', 'refresh', { refreshToken: 'oldRt' });
      expect(storageSpy.setAccessToken).toHaveBeenCalledWith('newAt');
    }));

    it('should call logout on refresh failure', fakeAsync(() => {
      apiSpy.post.and.returnValue(throwError(() => new Error('refresh failed')));

      service.refreshToken('badRt').subscribe({ error: () => {} });
      flushMicrotasks();
      tick(100);

      expect(storageSpy.clearAuthData).toHaveBeenCalled();
    }));
  });

  // Utility methods
  describe('forgotPassword', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should call auth/forgot-password', fakeAsync(() => {
      apiSpy.post.and.returnValue(of({ success: true }));

      service.forgotPassword({ email: 'a@b.com' }).subscribe();
      flushMicrotasks();

      expect(apiSpy.post).toHaveBeenCalledWith('auth', 'forgot-password', { email: 'a@b.com' });
    }));
  });

  describe('resetPassword', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should call auth/reset-password', fakeAsync(() => {
      apiSpy.post.and.returnValue(of({ success: true }));

      const req = { email: 'a@b.com', token: 'tok', newPassword: 'p', confirmPassword: 'p' };
      service.resetPassword(req).subscribe();
      flushMicrotasks();

      expect(apiSpy.post).toHaveBeenCalledWith('auth', 'reset-password', req);
    }));
  });

  describe('deleteAccount', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should call auth/account DELETE', fakeAsync(() => {
      apiSpy.delete.and.returnValue(of({ success: true }));

      service.deleteAccount().subscribe();
      flushMicrotasks();

      expect(apiSpy.delete).toHaveBeenCalledWith('auth', 'account');
    }));
  });

  // Logout
  describe('logout', () => {
    beforeEach(fakeAsync(() => {
      const futureExpiry = new Date(Date.now() + 3600 * 1000).toISOString();
      const token = fakeJwt({ sub: 'u1', email: 'a@b.com', user_type: 'Creator', exp: Date.now() / 1000 + 3600 });
      setup({ accessToken: token, tokenExpiry: futureExpiry });
      flushMicrotasks();
      tick(100);
    }));

    it('should clear storage, reset state, navigate, and show toast', fakeAsync(() => {
      service.logout();
      flushMicrotasks();
      tick(100);

      expect(appInsightsSpy.clearAuthenticatedUser).toHaveBeenCalled();
      expect(storageSpy.clearAuthData).toHaveBeenCalled();
      expect(service.isAuthenticated()).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
      expect(toastSpy.info).toHaveBeenCalledWith('You have been logged out');
    }));
  });

  // getAccessToken
  describe('getAccessToken', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should return null when no token stored', fakeAsync(() => {
      storageSpy.getAccessToken.and.returnValue(Promise.resolve(null));
      storageSpy.getTokenExpiry.and.returnValue(Promise.resolve(null));

      let result: string | null | undefined = 'initial';
      service.getAccessToken().then((t) => (result = t));
      flushMicrotasks();

      expect(result).toBeNull();
    }));

    it('should return token when not expiring soon', fakeAsync(() => {
      const futureExpiry = new Date(Date.now() + 30 * 60 * 1000).toISOString();
      storageSpy.getAccessToken.and.returnValue(Promise.resolve('validToken'));
      storageSpy.getTokenExpiry.and.returnValue(Promise.resolve(futureExpiry));

      let result: string | null | undefined;
      service.getAccessToken().then((t) => (result = t));
      flushMicrotasks();

      expect(result).toBe('validToken' as any);
    }));

    it('should trigger refresh when token expires within 5 minutes', fakeAsync(() => {
      const nearExpiry = new Date(Date.now() + 2 * 60 * 1000).toISOString(); // 2 min from now
      storageSpy.getAccessToken.and.returnValue(Promise.resolve('expiringToken'));
      storageSpy.getTokenExpiry.and.returnValue(Promise.resolve(nearExpiry));
      storageSpy.getRefreshToken.and.returnValue(Promise.resolve('rt'));

      const authResponse = {
        userId: 'u1',
        email: 'a@b.com',
        userType: UserType.Shopper,
        accessToken: 'freshToken',
        refreshToken: 'freshRt',
        accessTokenExpiry: new Date(Date.now() + 3600 * 1000).toISOString(),
      };
      apiSpy.post.and.returnValue(of({ success: true, data: authResponse }));

      let result: string | null | undefined;
      service.getAccessToken().then((t) => (result = t));
      flushMicrotasks();
      tick(100);

      expect(result).toBe('freshToken' as any);
    }));
  });

  // setProfileImageUrl
  describe('setProfileImageUrl', () => {
    beforeEach(fakeAsync(() => {
      setup();
      flushMicrotasks();
    }));

    it('should update profileImageUrl signal', () => {
      service.setProfileImageUrl('https://img.com/pic.jpg');
      expect(service.profileImageUrl()).toBe('https://img.com/pic.jpg');
    });
  });
});
