import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of, throwError } from 'rxjs';
import { PrivacyPage } from './privacy.page';
import { ApiService, ToastService } from '../../../core/services';

describe('PrivacyPage', () => {
  let component: PrivacyPage;
  let fixture: ComponentFixture<PrivacyPage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let toastService: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get', 'put']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning']);

    apiService.get.and.callFake(((resource: string, endpoint: string) => {
      if (endpoint === 'consent') return of({ success: true, data: { needsReconsent: false, marketingOptIn: false, lastConsentDate: '2024-01-01' } });
      if (endpoint === 'export') return of({ success: true, data: { profile: {}, bodyProfile: {} } });
      return of({ success: true, data: null });
    }) as any);

    await TestBed.configureTestingModule({
      imports: [PrivacyPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PrivacyPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load consent status on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.consentStatus()).toBeTruthy();
    expect(component.consentStatus()!.needsReconsent).toBe(false);
  }));

  it('should request data export', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    spyOn(document, 'createElement').and.returnValue({ href: '', download: '', click: jasmine.createSpy() } as any);
    spyOn(URL, 'createObjectURL').and.returnValue('blob:test');
    spyOn(URL, 'revokeObjectURL');

    component.requestDataExport();
    tick();

    expect(toastService.success).toHaveBeenCalledWith('Data export downloaded');
    expect(component.isExporting()).toBe(false);
  }));

  it('should accept privacy policy', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(of({ success: true }));

    component.acceptPrivacyPolicy();
    tick();

    expect(apiService.put).toHaveBeenCalledWith('privacy', 'consent', { acceptPrivacyPolicy: true });
    expect(toastService.success).toHaveBeenCalledWith('Privacy policy accepted');
    expect(component.consentStatus()!.needsReconsent).toBe(false);
  }));

  it('should toggle marketing consent', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(of({ success: true }));

    component.onMarketingToggle({ detail: { checked: true } } as CustomEvent);
    tick();

    expect(apiService.put).toHaveBeenCalledWith('privacy', 'consent', { marketingOptIn: true });
    expect(component.consentStatus()!.marketingOptIn).toBe(true);
  }));

  it('should show error on marketing toggle failure', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(throwError(() => new Error('fail')));

    component.onMarketingToggle({ detail: { checked: true } } as CustomEvent);
    tick();

    expect(toastService.error).toHaveBeenCalledWith('Failed to update preference');
  }));

  it('should show error on export failure', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.get.and.returnValue(throwError(() => new Error('fail')));

    component.requestDataExport();
    tick();

    expect(toastService.error).toHaveBeenCalledWith('Failed to export data');
    expect(component.isExporting()).toBe(false);
  }));

  it('should show error on accept policy failure', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(throwError(() => new Error('fail')));

    component.acceptPrivacyPolicy();
    tick();

    expect(toastService.error).toHaveBeenCalledWith('Failed to accept privacy policy');
  }));
});
