import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { RegisterPage } from './register.page';
import { CreatorService, AuthService, ToastService } from '../../../core';

describe('CreatorRegisterPage', () => {
  let component: RegisterPage;
  let fixture: ComponentFixture<RegisterPage>;
  let creatorService: jasmine.SpyObj<CreatorService>;
  let authService: jasmine.SpyObj<AuthService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', ['register']);
    authService = jasmine.createSpyObj('AuthService', ['refreshSession', 'logout'], {
      isAuthenticated: signal(true),
    });
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');

    await TestBed.configureTestingModule({
      imports: [RegisterPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: AuthService, useValue: authService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
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

  it('should require display name', () => {
    component['displayName'].set('');
    component.submit();
    expect(toastService.error).toHaveBeenCalledWith('Display name is required');
    expect(creatorService.register).not.toHaveBeenCalled();
  });

  it('should add social link', () => {
    component.addSocialLink();
    expect(component.socialLinks().length).toBe(1);
  });

  it('should remove social link', () => {
    component.addSocialLink();
    component.addSocialLink();
    component.removeSocialLink(0);
    expect(component.socialLinks().length).toBe(1);
  });

  it('should submit and navigate on success', fakeAsync(() => {
    creatorService.register.and.returnValue(of({ success: true } as any));
    authService.refreshSession.and.returnValue(of({} as any));

    component['displayName'].set('My Creator Name');
    component.submit();
    tick();

    expect(creatorService.register).toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/creator']);
    expect(toastService.success).toHaveBeenCalledWith('Welcome, Creator!');
  }));

  it('should build social links map from entries', fakeAsync(() => {
    creatorService.register.and.returnValue(of({ success: true } as any));
    authService.refreshSession.and.returnValue(of({} as any));

    component['displayName'].set('Creator');
    component['socialLinks'].set([{ platform: 'Instagram', url: 'https://instagram.com/test' }]);
    component.submit();
    tick();

    const callArgs = creatorService.register.calls.mostRecent().args[0];
    expect(callArgs.socialLinks).toEqual({ Instagram: 'https://instagram.com/test' });
  }));
});
