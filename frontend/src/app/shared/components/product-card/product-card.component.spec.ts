import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA, signal } from '@angular/core';
import { ProductCardComponent } from './product-card.component';
import { CommerceService } from '../../../core/services/commerce.service';
import { AnalyticsService } from '../../../core';
import { PostProductDetailResponse, FitRating } from '../../../models';

describe('ProductCardComponent', () => {
  let component: ProductCardComponent;
  let fixture: ComponentFixture<ProductCardComponent>;
  let commerceService: jasmine.SpyObj<CommerceService>;
  let analyticsService: jasmine.SpyObj<AnalyticsService>;

  const mockProduct: PostProductDetailResponse = {
    id: 'pp-1',
    productId: 'prod-1',
    productName: 'Test Dress',
    productDescription: 'A nice dress',
    productImageUrls: ['http://example.com/img.jpg'],
    productUrl: 'http://example.com/product',
    price: 59.99,
    currency: 'USD',
    retailerName: 'Test Retailer',
    sizeWorn: 'M',
    fitRating: FitRating.TrueToSize,
    fitNotes: null,
    stylingNotes: null,
    fitTags: [],
  };

  beforeEach(async () => {
    commerceService = jasmine.createSpyObj('CommerceService', ['trackAndShop'], { isTracking: signal(false) });
    analyticsService = jasmine.createSpyObj('AnalyticsService', ['track']);

    await TestBed.configureTestingModule({
      imports: [ProductCardComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CommerceService, useValue: commerceService },
        { provide: AnalyticsService, useValue: analyticsService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ProductCardComponent);
    component = fixture.componentInstance;
    component.product = mockProduct;
    component.postId = 'post-1';
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('getFitRatingLabel', () => {
    it('should return "Too Small" for TooSmall', () => {
      expect(component.getFitRatingLabel(FitRating.TooSmall)).toBe('Too Small');
    });

    it('should return "Slightly Small" for SlightlySmall', () => {
      expect(component.getFitRatingLabel(FitRating.SlightlySmall)).toBe('Slightly Small');
    });

    it('should return "True to Size" for TrueToSize', () => {
      expect(component.getFitRatingLabel(FitRating.TrueToSize)).toBe('True to Size');
    });

    it('should return "Slightly Large" for SlightlyLarge', () => {
      expect(component.getFitRatingLabel(FitRating.SlightlyLarge)).toBe('Slightly Large');
    });

    it('should return "Too Large" for TooLarge', () => {
      expect(component.getFitRatingLabel(FitRating.TooLarge)).toBe('Too Large');
    });
  });

  describe('getFitRatingColor', () => {
    it('should return success for TrueToSize', () => {
      expect(component.getFitRatingColor(FitRating.TrueToSize)).toBe('success');
    });

    it('should return warning for SlightlySmall', () => {
      expect(component.getFitRatingColor(FitRating.SlightlySmall)).toBe('warning');
    });

    it('should return warning for SlightlyLarge', () => {
      expect(component.getFitRatingColor(FitRating.SlightlyLarge)).toBe('warning');
    });

    it('should return danger for TooSmall', () => {
      expect(component.getFitRatingColor(FitRating.TooSmall)).toBe('danger');
    });

    it('should return danger for TooLarge', () => {
      expect(component.getFitRatingColor(FitRating.TooLarge)).toBe('danger');
    });
  });

  it('should track analytics and call trackAndShop on shopProduct', () => {
    component.shopProduct();
    expect(analyticsService.track).toHaveBeenCalledWith(
      'product.click',
      { postId: 'post-1', productId: 'pp-1', source: 'post_detail' },
      'pp-1',
      'Product'
    );
    expect(commerceService.trackAndShop).toHaveBeenCalledWith('post-1', 'pp-1', { source: 'post_detail' });
  });
});
