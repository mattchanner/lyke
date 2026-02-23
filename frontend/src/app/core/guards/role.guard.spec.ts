import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { signal } from '@angular/core';
import { roleGuard, creatorGuard, adminGuard, creatorOrAdminGuard } from './role.guard';
import { AuthService } from '../services/auth.service';
import { UserType } from '../../models';

describe('Role Guards', () => {
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

  describe('roleGuard(Creator)', () => {
    it('should allow Creator access', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Creator });

      const guard = roleGuard(UserType.Creator);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeTrue();
    });

    it('should redirect Admin to /admin when Creator required', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Admin });

      const guard = roleGuard(UserType.Creator);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/admin']);
    });

    it('should redirect Shopper to /feed when Creator required', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Shopper });

      const guard = roleGuard(UserType.Creator);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/feed']);
    });

    it('should redirect Retailer to /retailer when Creator required', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Retailer });

      const guard = roleGuard(UserType.Creator);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/retailer']);
    });
  });

  describe('roleGuard(Admin)', () => {
    it('should allow Admin access', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Admin });

      const guard = roleGuard(UserType.Admin);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeTrue();
    });
  });

  describe('roleGuard(Creator, Admin)', () => {
    it('should allow Creator', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Creator });

      const guard = roleGuard(UserType.Creator, UserType.Admin);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeTrue();
    });

    it('should allow Admin', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Admin });

      const guard = roleGuard(UserType.Creator, UserType.Admin);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeTrue();
    });
  });

  describe('unauthenticated', () => {
    it('should redirect to /auth/login when not authenticated', () => {
      setupMocks({ isAuthenticated: false });

      const guard = roleGuard(UserType.Creator);
      const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));
      expect(result).toBeFalse();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/auth/login']);
    });
  });

  describe('loading state', () => {
    it('should wait for loading then check role', fakeAsync(() => {
      setupMocks({ isLoading: true, isAuthenticated: false });

      const guard = roleGuard(UserType.Creator);
      let result: boolean | undefined;
      const promise = TestBed.runInInjectionContext(() => guard({} as any, {} as any)) as Promise<boolean>;
      promise.then((r) => (result = r));

      tick(100);

      authServiceMock.isLoading.set(false);
      authServiceMock.isAuthenticated.set(true);
      authServiceMock.userType.set(UserType.Creator);
      tick(100);

      expect(result).toBeTrue();
    }));
  });

  describe('convenience guards', () => {
    it('creatorGuard should allow Creator', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Creator });
      const result = TestBed.runInInjectionContext(() => creatorGuard({} as any, {} as any));
      expect(result).toBeTrue();
    });

    it('adminGuard should allow Admin', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Admin });
      const result = TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any));
      expect(result).toBeTrue();
    });

    it('creatorOrAdminGuard should allow Creator', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Creator });
      const result = TestBed.runInInjectionContext(() => creatorOrAdminGuard({} as any, {} as any));
      expect(result).toBeTrue();
    });

    it('creatorOrAdminGuard should allow Admin', () => {
      setupMocks({ isAuthenticated: true, userType: UserType.Admin });
      const result = TestBed.runInInjectionContext(() => creatorOrAdminGuard({} as any, {} as any));
      expect(result).toBeTrue();
    });
  });
});
