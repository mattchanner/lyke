import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { VerificationPage } from './verification.page';
import { CreatorService, ToastService } from '../../../core';
import { VerificationStatus } from '../../../models';

describe('VerificationPage', () => {
  let component: VerificationPage;
  let fixture: ComponentFixture<VerificationPage>;
  let creatorService: jasmine.SpyObj<CreatorService>;
  let toastService: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', ['getVerificationStatus', 'submitVerification']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    creatorService.getVerificationStatus.and.returnValue(of({ status: VerificationStatus.NotSubmitted } as any));

    await TestBed.configureTestingModule({
      imports: [VerificationPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(VerificationPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load verification status on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getVerificationStatus).toHaveBeenCalled();
    expect(component.verification()).toBeTruthy();
  }));

  it('should add document URL', () => {
    fixture.detectChanges();
    const initial = component.documentUrls().length;
    component.addDocumentUrl();
    expect(component.documentUrls().length).toBe(initial + 1);
  });

  it('should remove document URL', () => {
    fixture.detectChanges();
    component.addDocumentUrl();
    const count = component.documentUrls().length;
    component.removeDocumentUrl(0);
    expect(component.documentUrls().length).toBe(count - 1);
  });

  it('should require at least one URL to submit', () => {
    fixture.detectChanges();
    component['documentUrls'].set(['']);
    component.submitVerification();
    expect(toastService.warning).toHaveBeenCalledWith('Please add at least one document URL');
    expect(creatorService.submitVerification).not.toHaveBeenCalled();
  });

  it('should submit verification with valid URLs', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.submitVerification.and.returnValue(of({ success: true } as any));
    component['documentUrls'].set(['http://example.com/doc.pdf']);
    component.submitVerification();
    tick();
    expect(creatorService.submitVerification).toHaveBeenCalled();
    expect(toastService.success).toHaveBeenCalledWith('Verification submitted successfully');
  }));

  describe('status mappers', () => {
    it('should map status colors', () => {
      fixture.detectChanges();
      expect(component.getStatusColor(VerificationStatus.NotSubmitted)).toBe('medium');
      expect(component.getStatusColor(VerificationStatus.Pending)).toBe('warning');
      expect(component.getStatusColor(VerificationStatus.Approved)).toBe('success');
      expect(component.getStatusColor(VerificationStatus.Rejected)).toBe('danger');
    });

    it('should map status labels', () => {
      fixture.detectChanges();
      expect(component.getStatusLabel(VerificationStatus.NotSubmitted)).toBe('Not Submitted');
      expect(component.getStatusLabel(VerificationStatus.Pending)).toBe('Under Review');
      expect(component.getStatusLabel(VerificationStatus.Approved)).toBe('Verified');
      expect(component.getStatusLabel(VerificationStatus.Rejected)).toBe('Rejected');
    });
  });
});
