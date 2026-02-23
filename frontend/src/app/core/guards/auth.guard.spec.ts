import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { signal } from '@angular/core';
import { authGuard, noAuthGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';
import { UserType } from '../../models';

describe('Auth Guards', () => {
  let authServiceMock: any;
  let routerSpy: jasmine.SpyObj<Router>;

  function setupMocks(overrides: {
    isLoading?: boolean;
    isAuthenticated?: boolean;
    userType?: UserType | null;
  } = {}) {
    const isLoadingSig = signal(overrides.isLoading ?? false);
    const isAuthenticatedSig = signal(overrides.isAuthenticated ?? false);
    const userTypeSig = signal(overrides.userType ?? null);

    authServiceMock = {
      isLoading: isLoadingSig,
      isAuthenticated: isAuthenticatedSig,
      userType: userTypeSig,
    };

    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    routerSpy.navigate.and.returnValue(Promise.resolve(true));

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerSpy },
      ],
    });
  }

  describe('authGuard', () => {
    it('should allow access when authenticated', () => {
      setupMocks({ isAuthenticated: true });

      const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
      expect(result).toBeTrue();
    });

    it('should redirect to /auth/login when not authenticated', () => {
      setupMocks({ isAuthenticated: false });

      const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
    });

    it('should wait for loading to finish then allow', fakeAsync(() => {
      setupMocks({ isLoading: true, isAuthenticated: false });

      let result: boolean | undefined;
      const promise = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any)) as Promise<boolean>;
      promise.then((r) => (result = r));

      tick(100);
      expect(result).toBeUndefined(); // still loading

      // Simulate loading done
      authServiceMock.isLoading.set(false);
      authServiceMock.isAuthenticated.set(true);
      tick(100);

      expect(result).toBeTrue();
    }));

    it('should wait for loading then redirect when not authenticated', fakeAsync(() => {
      setupMocks({ isLoading: true, isAuthenticated: false });

      let result: boolean | undefined;
      const promise = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any)) as Promise<boolean>;
      promise.then((r) => (result = r));

      tick(100);

      authServiceMock.isLoading.set(false);
      // isAuthenticated stays false
      tick(100);

      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
    }));
  });

  describe('noAuthGuard', () => {
    it('should allow access when not authenticated', () => {
      setupMocks({ isAuthenticated: false });

      const result = TestBed.runInInjectionContext(() => noAuthGuard({} as any, {} as any));
      expect(result).toBeTrue();
    });

    it('should redirect Admin to /admin', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Admin });

      const result = TestBed.runInInjectionContext(() => noAuthGuard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin']);
    });

    it('should redirect Creator to /creator', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Creator });

      const result = TestBed.runInInjectionContext(() => noAuthGuard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/creator']);
    });

    it('should redirect Shopper to /feed', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Shopper });

      const result = TestBed.runInInjectionContext(() => noAuthGuard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/feed']);
    });

    it('should wait for loading then allow when not authenticated', fakeAsync(() => {
      setupMocks({ isLoading: true, isAuthenticated: false });

      let result: boolean | undefined;
      const promise = TestBed.runInInjectionContext(() => noAuthGuard({} as any, {} as any)) as Promise<boolean>;
      promise.then((r) => (result = r));

      tick(100);

      authServiceMock.isLoading.set(false);
      tick(100);

      expect(result).toBeTrue();
    }));

    it('should wait for loading then redirect when authenticated', fakeAsync(() => {
      setupMocks({ isLoading: true, isAuthenticated: false });

      let result: boolean | undefined;
      const promise = TestBed.runInInjectionContext(() => noAuthGuard({} as any, {} as any)) as Promise<boolean>;
      promise.then((r) => (result = r));

      tick(100);

      authServiceMock.isLoading.set(false);
      authServiceMock.isAuthenticated.set(true);
      authServiceMock.userType.set(UserType.Admin);
      tick(100);

      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin']);
    }));
  });
});
