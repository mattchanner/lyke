import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { BodyProfileEditPage } from './body-profile-edit.page';
import { ApiService, ToastService } from '../../../core';
import { FitPreference } from '../../../models';

describe('BodyProfileEditPage', () => {
  let component: BodyProfileEditPage;
  let fixture: ComponentFixture<BodyProfileEditPage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let router: jasmine.SpyObj<Router>;

  const mockProfile = {
    heightCm: 170,
    weightKg: 65,
    bodyTypeId: 1,
    frameSizeId: 2,
    fitPreferences: [FitPreference.Regular],
    needsProfileUpdate: false,
  };

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get', 'put']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');

    apiService.get.and.callFake(((resource: string, endpoint: string) => {
      if (endpoint === 'body-types') return of({ success: true, data: [{ id: 1, name: 'Athletic' }, { id: 2, name: 'Slim' }] });
      if (endpoint === 'body') return of({ success: true, data: mockProfile });
      return of({ success: true, data: null });
    }) as any);

    await TestBed.configureTestingModule({
      imports: [BodyProfileEditPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BodyProfileEditPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load body types on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.bodyTypes().length).toBe(2);
  }));

  it('should populate form from existing profile', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.heightCmControl.value).toBe(170);
    expect(component.weightControl.value).toBe(65);
    expect(component.selectedBodyTypeId()).toBe(1);
    expect(component.selectedFitPreferences().has(FitPreference.Regular)).toBe(true);
  }));

  it('should convert height cm to ft/in', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.onHeightUnitChange({ detail: { value: 'ft' } } as CustomEvent);
    expect(component.heightUnit()).toBe('ft');
    expect(component.heightFtControl.value).toBe(5); // 170cm ~ 5ft 7in
  }));

  it('should convert height ft/in to cm', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.onHeightUnitChange({ detail: { value: 'ft' } } as CustomEvent);
    component.heightFtControl.setValue(6);
    component.heightInControl.setValue(0);
    component.onHeightUnitChange({ detail: { value: 'cm' } } as CustomEvent);
    expect(component.heightCmControl.value).toBe(183); // 6ft = 183cm
  }));

  it('should convert weight kg to lbs', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.onWeightUnitChange({ detail: { value: 'lbs' } } as CustomEvent);
    expect(component.weightUnit()).toBe('lbs');
    expect(component.weightControl.value).toBe(143); // 65kg ~ 143lbs
  }));

  it('should convert weight lbs to kg', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.onWeightUnitChange({ detail: { value: 'lbs' } } as CustomEvent);
    component.weightControl.setValue(154);
    component.onWeightUnitChange({ detail: { value: 'kg' } } as CustomEvent);
    expect(component.weightUnit()).toBe('kg');
    expect(component.weightControl.value).toBe(69.9); // 154lbs ~ 69.9kg
  }));

  it('should select body type', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.selectBodyType(2);
    expect(component.selectedBodyTypeId()).toBe(2);
  }));

  it('should select fit preference', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.toggleFitPreference(FitPreference.Fitted);
    expect(component.selectedFitPreferences().has(FitPreference.Fitted)).toBe(true);
  }));

  it('should validate isFormValid with cm units', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.isFormValid).toBe(true);
  }));

  it('should detect hasChanges', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.hasChanges).toBe(false);
    component.heightCmControl.setValue(180);
    expect(component.hasChanges).toBe(true);
  }));

  it('should not submit when no changes', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.onSubmit();
    expect(apiService.put).not.toHaveBeenCalled();
  }));

  it('should submit only changed fields', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.put.and.returnValue(of({ success: true }));
    component.heightCmControl.setValue(180);
    component.onSubmit();
    tick();
    expect(apiService.put).toHaveBeenCalledWith('profile', 'body', { heightCm: 180 });
    expect(toastService.success).toHaveBeenCalledWith('Body profile updated');
    expect(router.navigate).toHaveBeenCalledWith(['/profile']);
  }));

  it('should compute getHeightCm in cm mode', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.getHeightCm()).toBe(170);
  }));

  it('should compute getHeightCm in ft mode', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component.onHeightUnitChange({ detail: { value: 'ft' } } as CustomEvent);
    component.heightFtControl.setValue(5);
    component.heightInControl.setValue(10);
    expect(component.getHeightCm()).toBe(178); // 5'10" ~ 178cm
  }));

  it('should compute getWeightKg in kg mode', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.getWeightKg()).toBe(65);
  }));

  it('should compute getWeightKg in lbs mode', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component['weightUnit'].set('lbs');
    component.weightControl.setValue(154);
    expect(component.getWeightKg()).toBeCloseTo(69.9, 0);
  }));
});
