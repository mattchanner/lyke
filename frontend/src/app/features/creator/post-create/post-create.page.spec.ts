import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { PostCreatePage } from './post-create.page';
import { CreatorService, ToastService, ApiService } from '../../../core';
import { CommerceService } from '../../../core/services/commerce.service';
import { MediaType, FitRating } from '../../../models';

describe('PostCreatePage', () => {
  let component: PostCreatePage;
  let fixture: ComponentFixture<PostCreatePage>;
  let creatorService: jasmine.SpyObj<CreatorService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let apiService: jasmine.SpyObj<ApiService>;
  let router: jasmine.SpyObj<Router>;

  const mockProduct = { id: 'prod-1', name: 'Dress', retailerName: 'Store', price: 50, currency: 'USD', imageUrls: ['img.jpg'], externalSku: 'SKU1', description: null, category: 'dresses', subCategory: null, productUrl: 'http://example.com', isActive: true, postCount: 0, retailerId: 'r1', retailerLogoUrl: null } as any;

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', ['createPost', 'submitPost']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);
    apiService = jasmine.createSpyObj('ApiService', ['get', 'post', 'delete', 'uploadFile']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');

    await TestBed.configureTestingModule({
      imports: [PostCreatePage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: ToastService, useValue: toastService },
        { provide: ApiService, useValue: apiService },
        { provide: Router, useValue: router },
        { provide: CommerceService, useValue: jasmine.createSpyObj('CommerceService', ['searchProducts']) },
        { provide: ActivatedRoute, useValue: { queryParams: of({}), snapshot: { params: {}, queryParams: {} } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PostCreatePage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with empty media', () => {
    expect(component.uploadedMedia().length).toBe(0);
    expect(component.hasMedia()).toBe(false);
  });

  it('should add product to tagged products', () => {
    component.addProduct(mockProduct);
    expect(component.taggedProducts().length).toBe(1);
    expect(component.taggedProducts()[0].product.id).toBe('prod-1');
  });

  it('should not add duplicate product', () => {
    component.addProduct(mockProduct);
    component.addProduct(mockProduct);
    expect(component.taggedProducts().length).toBe(1);
  });

  it('should remove product by index', () => {
    component.addProduct(mockProduct);
    component.removeProduct(0);
    expect(component.taggedProducts().length).toBe(0);
  });

  it('should update product size', () => {
    component.addProduct(mockProduct);
    component.updateProductSize(0, 'M');
    expect(component.taggedProducts()[0].sizeWorn).toBe('M');
  });

  it('should update product fit rating', () => {
    component.addProduct(mockProduct);
    component.updateProductFitRating(0, FitRating.TrueToSize);
    expect(component.taggedProducts()[0].fitRating).toBe(FitRating.TrueToSize);
  });

  it('should update product fit notes', () => {
    component.addProduct(mockProduct);
    component.updateProductFitNotes(0, 'Fits well');
    expect(component.taggedProducts()[0].fitNotes).toBe('Fits well');
  });

  it('should update product styling notes', () => {
    component.addProduct(mockProduct);
    component.updateProductStylingNotes(0, 'Goes with jeans');
    expect(component.taggedProducts()[0].stylingNotes).toBe('Goes with jeans');
  });

  it('should remove media by index', () => {
    component['uploadedMedia'].set([{ originalUrl: 'url1', thumbnailUrl: 'thumb1' } as any, { originalUrl: 'url2', thumbnailUrl: 'thumb2' } as any]);
    component.removeMedia(0);
    expect(component.uploadedMedia().length).toBe(1);
    expect(component.uploadedMedia()[0].originalUrl).toBe('url2');
  });

  it('should save as draft', fakeAsync(() => {
    creatorService.createPost.and.returnValue(of({ success: true, data: { id: 'new-post' } } as any));
    component.saveAsDraft();
    tick();
    expect(creatorService.createPost).toHaveBeenCalled();
    expect(creatorService.submitPost).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/creator/posts']);
    expect(toastService.success).toHaveBeenCalledWith('Post saved as draft');
  }));

  it('should submit for review (create + submit)', fakeAsync(() => {
    creatorService.createPost.and.returnValue(of({ success: true, data: { id: 'new-post' } } as any));
    creatorService.submitPost.and.returnValue(of({ success: true } as any));

    component.submitForReview();
    tick();

    expect(creatorService.createPost).toHaveBeenCalled();
    expect(creatorService.submitPost).toHaveBeenCalledWith('new-post');
    expect(router.navigate).toHaveBeenCalledWith(['/creator/posts']);
    expect(toastService.success).toHaveBeenCalledWith('Post submitted for review');
  }));

  it('should show error on create failure', fakeAsync(() => {
    creatorService.createPost.and.returnValue(of({ success: false, error: { message: 'Failed' } } as any));
    component.saveAsDraft();
    tick();
    expect(toastService.error).toHaveBeenCalled();
  }));

  it('should compute mediaItems from uploaded media', () => {
    component['uploadedMedia'].set([{ originalUrl: 'url1', thumbnailUrl: 'thumb1' } as any]);
    expect(component.mediaItems().length).toBe(1);
    expect(component.mediaItems()[0].url).toBe('url1');
  });
});
