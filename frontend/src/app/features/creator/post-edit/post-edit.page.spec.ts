import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { of } from 'rxjs';
import { PostEditPage } from './post-edit.page';
import { CreatorService, ToastService } from '../../../core';
import { CommerceService } from '../../../core/services/commerce.service';
import { PostStatus, MediaType, FitRating } from '../../../models';

describe('PostEditPage', () => {
  let component: PostEditPage;
  let fixture: ComponentFixture<PostEditPage>;
  let creatorService: jasmine.SpyObj<CreatorService>;
  let commerceService: jasmine.SpyObj<CommerceService>;
  let toastService: jasmine.SpyObj<ToastService>;
  let router: jasmine.SpyObj<Router>;

  const mockPost = {
    id: 'p1', title: 'Edit Post', description: 'Desc', status: PostStatus.Draft,
    mediaType: MediaType.Image, mediaUrls: ['url1'], thumbnailUrls: ['thumb1'],
    products: [{ productId: 'prod-1', productName: 'Dress', retailerName: 'Store', productPrice: 50, productCurrency: 'USD', productImage: 'img.jpg', sizeWorn: 'M', fitRating: FitRating.TrueToSize, fitNotes: '', stylingNotes: '' }],
    engagements: { views: 10, likes: 5, saves: 2, shares: 1 }, createdAt: '2024-01-01', publishedAt: null,
  };

  beforeEach(async () => {
    creatorService = jasmine.createSpyObj('CreatorService', ['getPost', 'updatePost', 'submitPost']);
    commerceService = jasmine.createSpyObj('CommerceService', ['searchProducts']);
    toastService = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);
    router = jasmine.createSpyObj('Router', ['navigate', 'createUrlTree', 'serializeUrl']);
    router.createUrlTree.and.returnValue({} as any);
    router.serializeUrl.and.returnValue('');

    creatorService.getPost.and.returnValue(of(mockPost as any));

    await TestBed.configureTestingModule({
      imports: [PostEditPage],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
      providers: [
        { provide: CreatorService, useValue: creatorService },
        { provide: CommerceService, useValue: commerceService },
        { provide: ToastService, useValue: toastService },
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'p1' } } } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PostEditPage);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should load post from route param', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(creatorService.getPost).toHaveBeenCalledWith('p1');
    expect(component.post()).toBeTruthy();
    expect(component.title()).toBe('Edit Post');
  }));

  it('should populate form fields from loaded post', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.description()).toBe('Desc');
    expect(component.mediaType()).toBe(MediaType.Image);
    expect(component.mediaUrls().length).toBe(1);
    expect(component.taggedProducts().length).toBe(1);
  }));

  it('should compute isEditable true for Draft', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    expect(component.isEditable()).toBe(true);
  }));

  it('should compute isEditable false for Published', fakeAsync(() => {
    creatorService.getPost.and.returnValue(of({ ...mockPost, status: PostStatus.Published } as any));
    fixture.detectChanges();
    tick();
    expect(component.isEditable()).toBe(false);
  }));

  it('should compute isEditable true for Rejected', fakeAsync(() => {
    creatorService.getPost.and.returnValue(of({ ...mockPost, status: PostStatus.Rejected } as any));
    fixture.detectChanges();
    tick();
    expect(component.isEditable()).toBe(true);
  }));

  it('should search products', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    commerceService.searchProducts.and.returnValue(of([{ id: 'prod-2', name: 'Shirt' }] as any));
    component['productSearchQuery'].set('shirt');
    component.searchProducts();
    tick();
    expect(commerceService.searchProducts).toHaveBeenCalledWith('shirt');
    expect(component.searchResults().length).toBe(1);
  }));

  it('should not search with empty query', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    component['productSearchQuery'].set('');
    component.searchProducts();
    expect(commerceService.searchProducts).not.toHaveBeenCalled();
  }));

  it('should deduplicate when adding product', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    const existingCount = component.taggedProducts().length;
    component.addProduct({ id: 'prod-1', name: 'Same', retailerName: 'Store', price: 50, currency: 'USD', imageUrls: [] } as any);
    expect(component.taggedProducts().length).toBe(existingCount);
    expect(toastService.warning).toHaveBeenCalledWith('Product already added');
  }));

  it('should save changes', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.updatePost.and.returnValue(of({ success: true, data: mockPost } as any));

    component.saveChanges();
    tick();

    expect(creatorService.updatePost).toHaveBeenCalled();
    expect(toastService.success).toHaveBeenCalledWith('Post updated');
  }));

  it('should submit for review (save then submit)', fakeAsync(() => {
    fixture.detectChanges();
    tick();
    creatorService.updatePost.and.returnValue(of({ success: true, data: mockPost } as any));
    creatorService.submitPost.and.returnValue(of({ success: true } as any));

    component.submitForReview();
    tick();

    expect(creatorService.updatePost).toHaveBeenCalled();
    expect(creatorService.submitPost).toHaveBeenCalledWith('p1');
    expect(router.navigate).toHaveBeenCalledWith(['/creator/posts']);
  }));

  it('should navigate on post not found', fakeAsync(() => {
    creatorService.getPost.and.returnValue(of(null));
    fixture.detectChanges();
    tick();
    expect(toastService.error).toHaveBeenCalledWith('Post not found');
    expect(router.navigate).toHaveBeenCalledWith(['/creator/posts']);
  }));
});
