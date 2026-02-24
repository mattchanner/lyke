import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { ShopTheLookComponent } from './shop-the-look.component';
import { CommerceService } from '../../../core/services/commerce.service';
import { PostProductDetailResponse, FitRating } from '../../../models';

describe('ShopTheLookComponent', () => {
  let component: ShopTheLookComponent;
  let fixture: ComponentFixture<ShopTheLookComponent>;

  const mockProducts: PostProductDetailResponse[] = [
    {
      id: 'pp-1',
      productId: 'prod-1',
      productName: 'Test Product',
      productDescription: null,
      productImageUrls: [],
      productUrl: 'http://example.com',
      price: 49.99,
      currency: 'USD',
      retailerName: 'Retailer',
      sizeWorn: 'M',
      fitRating: FitRating.TrueToSize,
      fitNotes: null,
      stylingNotes: null,
      fitTags: [],
    },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShopTheLookComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        provideHttpClient(),
        { provide: CommerceService, useValue: jasmine.createSpyObj('CommerceService', ['trackClick'], { isTracking: () => false }) },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ShopTheLookComponent);
    component = fixture.componentInstance;
    component.products = mockProducts;
    component.postId = 'post-1';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should accept products input', () => {
    expect(component.products.length).toBe(1);
  });

  it('should accept postId input', () => {
    expect(component.postId).toBe('post-1');
  });

  it('should reflect products count', () => {
    const multipleProducts: PostProductDetailResponse[] = [
      { ...mockProducts[0], id: 'pp-1', productId: 'prod-1' },
      { ...mockProducts[0], id: 'pp-2', productId: 'prod-2' },
      { ...mockProducts[0], id: 'pp-3', productId: 'prod-3' },
    ];
    component.products = multipleProducts;
    expect(component.products.length).toBe(3);
  });

  it('should handle empty products array', () => {
    component.products = [];
    fixture.detectChanges();
    expect(component.products.length).toBe(0);
  });

  it('should update products when input changes', () => {
    const updated: PostProductDetailResponse[] = [
      { ...mockProducts[0], id: 'pp-new', productId: 'prod-new', productName: 'Updated Product' },
    ];
    component.products = updated;
    fixture.detectChanges();
    expect(component.products[0].productName).toBe('Updated Product');
  });
});
