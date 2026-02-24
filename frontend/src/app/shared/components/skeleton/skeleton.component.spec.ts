import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { SkeletonComponent } from './skeleton.component';

describe('SkeletonComponent', () => {
  let component: SkeletonComponent;
  let fixture: ComponentFixture<SkeletonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SkeletonComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();
    fixture = TestBed.createComponent(SkeletonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have default type of text', () => {
    expect(component.type).toBe('text');
  });

  it('should have default width of 100%', () => {
    expect(component.width).toBe('100%');
  });

  it('should have default height of auto', () => {
    expect(component.height).toBe('auto');
  });

  it('should accept custom type input', () => {
    component.type = 'circle';
    expect(component.type).toBe('circle');
  });
});
