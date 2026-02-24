import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { EarningsPage } from './earnings.page';
import { CreatorService, ToastService } from '../../../core';
import { EarningType, EarningStatus } from '../../../models';

describe('EarningsPage', () => {
  let component: EarningsPage;
  let fixture: ComponentFixture<EarningsPage>;
  let creatorService: jasmine.SpyObj<CreatorService>;
  let toastService: jasmine.SpyObj<ToastService>;

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', ['getEarnings', 'getEarningsHistory']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    creatorService.getEarnings.and.returnValue(of({ totalEarnings: 100, pendingEarnings: 20, paidEarnings: 80 } as any));
    creatorService.getEarningsHistory.and.returnValue(of({
      success: true,
      data: [{ id: 'e1', type: EarningType.Affiliate, amount: 10, status: EarningStatus.Pending, createdAt: '2024-01-01' }],
      meta: { hasNextPage: false },
    } as any));

    await TestBed.configureTestingModule({
      imports: [EarningsPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: ToastService, useValue: toastService },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EarningsPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load summary and history on init', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getEarnings).toHaveBeenCalled();
    expect(creatorService.getEarningsHistory).toHaveBeenCalled();
    expect(component.summary()).toBeTruthy();
    expect(component.earnings().length).toBe(1);
  }));

  it('should filter by tab', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.getEarningsHistory.calls.reset();
    component.onTabChange('pending' as any);
    tick();
    expect(component.activeTab()).toBe('pending');
    expect(creatorService.getEarningsHistory).toHaveBeenCalled();
  }));

  it('should not reload on same tab', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.getEarningsHistory.calls.reset();
    component.onTabChange('all' as any);
    expect(creatorService.getEarningsHistory).not.toHaveBeenCalled();
  }));

  it('should paginate on loadMore', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    const event = { target: { complete: jasmine.createSpy() } } as any;
    component.loadMore(event);
    tick(1000);
    expect(component.currentPage()).toBe(2);
  }));

  describe('enum mappers', () => {
    it('should map earning type labels', () => {
      expect(component.getEarningTypeLabel(EarningType.Affiliate)).toBe('Affiliate');
      expect(component.getEarningTypeLabel(EarningType.Sponsored)).toBe('Sponsored');
    });

    it('should map earning type colors', () => {
      expect(component.getEarningTypeColor(EarningType.Affiliate)).toBe('primary');
      expect(component.getEarningTypeColor(EarningType.Sponsored)).toBe('tertiary');
    });

    it('should map status labels', () => {
      expect(component.getStatusLabel(EarningStatus.Pending)).toBe('Pending');
      expect(component.getStatusLabel(EarningStatus.Confirmed)).toBe('Confirmed');
      expect(component.getStatusLabel(EarningStatus.Paid)).toBe('Paid');
    });

    it('should map status colors', () => {
      expect(component.getStatusColor(EarningStatus.Pending)).toBe('warning');
      expect(component.getStatusColor(EarningStatus.Confirmed)).toBe('success');
      expect(component.getStatusColor(EarningStatus.Paid)).toBe('primary');
    });
  });
});
