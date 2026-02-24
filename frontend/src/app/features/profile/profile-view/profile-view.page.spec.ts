import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { ProfileViewPage } from './profile-view.page';
import { ApiService, AuthService } from '../../../core';
import { UserType } from '../../../models';

describe('ProfileViewPage', () => {
  let component: ProfileViewPage;
  let fixture: ComponentFixture<ProfileViewPage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get']);
    authService = jasmine.createSpyObj('AuthService', ['logout'], {
      isAuthenticated: signal(true),
      userType: signal(UserType.Shopper),
      profileImageUrl: signal(null),
      isCreator: signal(false),
      isRetailer: signal(false),
      isAdmin: signal(false),
    });

    apiService.get.and.returnValue(of({
      success: true,
      data: { email: 'test@test.com', hasBodyProfile: true, profileImageUrl: null },
    }));

    await TestBed.configureTestingModule({
      imports: [ProfileViewPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: AuthService, useValue: authService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProfileViewPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load profile on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(apiService.get).toHaveBeenCalledWith('profile', 'me');
    expect(component.profile()).toBeTruthy();
  }));

  it('should load body profile when hasBodyProfile is true', fakeAsync(() => {
    apiService.get.and.callFake(((resource: string, endpoint: string) => {
      if (endpoint === 'me') return of({ success: true, data: { email: 'test@test.com', hasBodyProfile: true, profileImageUrl: null } });
      if (endpoint === 'body') return of({ success: true, data: { heightCm: 170, weightKg: 65 } });
      return of({ success: true, data: null });
    }) as any);
    fixture.detectChanges();
    tick();
    expect(component.bodyProfile()).toBeTruthy();
  }));

  it('should not load body profile when hasBodyProfile is false', fakeAsync(() => {
    apiService.get.and.returnValue(of({ success: true, data: { email: 'test@test.com', hasBodyProfile: false, profileImageUrl: null } }));
    fixture.detectChanges();
    tick();
    expect(component.bodyProfile()).toBeNull();
  }));

  it('should call authService.logout on logout', () => {
    fixture.detectChanges();
    component.logout();
    expect(authService.logout).toHaveBeenCalled();
  });

  it('should set isLoading false after load', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.isLoading()).toBe(false);
  }));
});
