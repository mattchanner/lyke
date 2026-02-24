import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { SkeletonListComponent } from './skeleton-list.component';

describe('SkeletonListComponent', () => {
  let component: SkeletonListComponent;
  let fixture: ComponentFixture<SkeletonListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkeletonListComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();
    fixture = TestBed.createComponent(SkeletonListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default count of 5', () => {
    expect(component.count).toBe(5);
  });

  it('should generate items array matching count', () => {
    expect(component.items.length).toBe(5);
  });

  it('should update items when count changes', () => {
    component.count = 3;
    expect(component.items.length).toBe(3);
  });

  it('should have default showAvatar true', () => {
    expect(component.showAvatar).toBe(true);
  });

  it('should have default showSubtitle true', () => {
    expect(component.showSubtitle).toBe(true);
  });
});
