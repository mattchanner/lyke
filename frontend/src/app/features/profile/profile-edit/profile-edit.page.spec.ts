import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { ProfileEditPage } from './profile-edit.page';
import { ApiService, AuthService, ToastService } from '../../../core';
import { ModalController } from '@ionic/angular/standalone';

describe('ProfileEditPage', () => {
  let component: ProfileEditPage;
  let fixture: ComponentFixture<ProfileEditPage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let authService: jasmine.SpyObj<AuthService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let router: jasmine.SpyObj<Router>;
  let modalCtrl: jasmine.SpyObj<ModalController>;

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get', 'put', 'uploadFile']);
    authService = jasmine.createSpyObj('AuthService', ['setProfileImageUrl'], {
      profileImageUrl: signal(null),
    });
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');
    modalCtrl = jasmine.createSpyObj('ModalController', ['create']);

    apiService.get.and.returnValue(of({
      success: true,
      data: { email: 'original@test.com', profileImageUrl: null, hasBodyProfile: false },
    }));

    await TestBed.configureTestingModule({
      imports: [ProfileEditPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: AuthService, useValue: authService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: ModalController, useValue: modalCtrl },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProfileEditPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load profile into form', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.form.value.email).toBe('original@test.com');
  }));

  it('should validate email format', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.form.controls.email.setValue('invalid');
    expect(component.form.controls.email.hasError('email')).toBe(true);
  }));

  it('should detect isDirty when email changes', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.isDirty).toBe(false);
    component.form.controls.email.setValue('new@test.com');
    expect(component.isDirty).toBe(true);
  }));

  it('should not submit when form is not dirty', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.onSubmit();
    expect(apiService.put).not.toHaveBeenCalled();
  }));

  it('should not submit when form is invalid', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.form.controls.email.setValue('invalid');
    component.onSubmit();
    expect(apiService.put).not.toHaveBeenCalled();
  }));

  it('should submit and navigate on success', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(of({ success: true, data: { email: 'new@test.com' } }));
    component.form.controls.email.setValue('new@test.com');
    component.onSubmit();
    tick();
    expect(apiService.put).toHaveBeenCalledWith('profile', '', { email: 'new@test.com' });
    expect(toastService.success).toHaveBeenCalledWith('Profile updated');
    expect(router.navigate).toHaveBeenCalledWith(['/profile']);
  }));

  it('should reject files larger than 5MB', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    const mockFile = new File(['x'.repeat(6 * 1024 * 1024)], 'big.jpg', { type: 'image/jpeg' });
    Object.defineProperty(mockFile, 'size', { value: 6 * 1024 * 1024 });
    const mockInput = { files: [mockFile], value: '' } as unknown as HTMLInputElement;
    const mockEvent = { target: mockInput } as unknown as Event;

    component.onFileSelected(mockEvent);
    tick();

    expect(toastService.error).toHaveBeenCalledWith('Image must be 5MB or smaller');
  }));

  it('should set isSaving during submit', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(of({ success: true, data: {} }));
    component.form.controls.email.setValue('new@test.com');
    component.onSubmit();
    expect(component.isSaving()).toBe(true);
    tick();
    expect(component.isSaving()).toBe(false);
  }));

  it('should show error on submit failure response', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(of({ success: false, error: { code: 'VALIDATION_ERROR', message: 'Email taken' } }));
    component.form.controls.email.setValue('new@test.com');
    component.onSubmit();
    tick();
    expect(toastService.error).toHaveBeenCalledWith('Email taken');
  }));
});
