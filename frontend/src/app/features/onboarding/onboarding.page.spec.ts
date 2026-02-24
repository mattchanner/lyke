import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { OnboardingPage } from './onboarding.page';
import { ApiService, ToastService, AnalyticsService } from '../../core';
import { FitPreference } from '../../models';

describe('OnboardingPage', () => {
  let component: OnboardingPage;
  let fixture: ComponentFixture<OnboardingPage>;
  let apiService: jasmine.SpyObj<ApiService>;
  let analyticsService: jasmine.SpyObj<AnalyticsService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    apiService = jasmine.createSpyObj('ApiService', ['get', 'post']);
    analyticsService = jasmine.createSpyObj('AnalyticsService', ['track']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');
    (router as any).events = of(null);

    apiService.get.and.returnValue(of({ success: true, data: [{ id: 1, name: 'Athletic' }, { id: 2, name: 'Slim' }] }));

    await TestBed.configureTestingModule({
      imports: [OnboardingPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: ApiService, useValue: apiService },
        { provide: AnalyticsService, useValue: analyticsService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(OnboardingPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should start at step 1', () => {
    fixture.detectChanges();
    expect(component.currentStep()).toBe(1);
  });

  it('should compute progress from step', () => {
    fixture.detectChanges();
    expect(component.progress()).toBeCloseTo(1 / 6, 5); // 1/6 steps
  });

  it('should load body types on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.bodyTypes().length).toBe(2);
  }));

  it('should not proceed from step 1 without height', () => {
    fixture.detectChanges();
    expect(component.canProceed()).toBe(false);
  });

  it('should proceed from step 1 with valid height', () => {
    fixture.detectChanges();
    component.heightCmControl.setValue(170);
    expect(component.canProceed()).toBe(true);
  });

  it('should advance to next step', () => {
    fixture.detectChanges();
    component.heightCmControl.setValue(170);
    component.nextStep();
    expect(component.currentStep()).toBe(2);
  });

  it('should go back to previous step', () => {
    fixture.detectChanges();
    component.heightCmControl.setValue(170);
    component.nextStep();
    component.previousStep();
    expect(component.currentStep()).toBe(1);
  });

  it('should not go back from step 1', () => {
    fixture.detectChanges();
    component.previousStep();
    expect(component.currentStep()).toBe(1);
  });

  it('should validate step 2 (weight)', () => {
    fixture.detectChanges();
    component.heightCmControl.setValue(170);
    component.nextStep(); // step 2
    expect(component.canProceed()).toBe(false);
    component.weightControl.setValue(65);
    expect(component.canProceed()).toBe(true);
  });

  it('should validate step 3 (body type)', () => {
    fixture.detectChanges();
    component.heightCmControl.setValue(170);
    component.nextStep();
    component.weightControl.setValue(65);
    component.nextStep(); // step 3
    expect(component.canProceed()).toBe(false);
    component.selectBodyType(1);
    expect(component.canProceed()).toBe(true);
  });

  it('should select fit preference', () => {
    fixture.detectChanges();
    component.toggleFitPreference(FitPreference.Fitted);
    expect(component.selectedFitPreferences().has(FitPreference.Fitted)).toBe(true);
  });

  it('should require fit preference to submit', () => {
    fixture.detectChanges();
    expect(component.canSubmit()).toBe(false);
    component.toggleFitPreference(FitPreference.Regular);
    expect(component.canSubmit()).toBe(true);
  });

  it('should submit body profile and show success', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    apiService.post.and.returnValue(of({ success: true }));

    component.heightCmControl.setValue(170);
    component.weightControl.setValue(65);
    component.selectBodyType(1);
    component.toggleFitPreference(FitPreference.Regular);

    component.onSubmit();
    tick();

    expect(apiService.post).toHaveBeenCalledWith('profile', 'body', jasmine.objectContaining({
      heightCm: 170,
      weightKg: 65,
      bodyTypeId: 1,
      fitPreferences: [FitPreference.Regular],
    }));
    expect(component.currentStep()).toBe(6);
    expect(analyticsService.track).toHaveBeenCalled();
    expect(toastService.success).toHaveBeenCalledWith('Profile complete!');
  }));

  it('should compute getHeightCm in ft mode', () => {
    fixture.detectChanges();
    component['heightUnit'].set('ft');
    component.heightFtControl.setValue(5);
    component.heightInControl.setValue(10);
    expect(component.getHeightCm()).toBe(178);
  });

  it('should compute getWeightKg in lbs mode', () => {
    fixture.detectChanges();
    component['weightUnit'].set('lbs');
    component.weightControl.setValue(154);
    expect(component.getWeightKg()).toBeCloseTo(69.9, 0);
  });
});
