import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { SkeletonPostCardComponent } from './skeleton-post-card.component';

describe('SkeletonPostCardComponent', () => {
  let component: SkeletonPostCardComponent;
  let fixture: ComponentFixture<SkeletonPostCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkeletonPostCardComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();
    fixture = TestBed.createComponent(SkeletonPostCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default imageHeight of 300px', () => {
    expect(component.imageHeight).toBe('300px');
  });

  it('should accept custom imageHeight', () => {
    component.imageHeight = '400px';
    expect(component.imageHeight).toBe('400px');
  });
});
