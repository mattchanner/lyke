import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { CommerceService, ClickContext } from './commerce.service';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { ToastService } from './toast.service';

describe('CommerceService', () => {
  let service: CommerceService;
  let apiSpy: jasmine.SpyObj<ApiService>;
  let storageSpy: jasmine.SpyObj<StorageService>;
  let toastSpy: jasmine.SpyObj<ToastService>;

  beforeEach(() => {
    apiSpy = jasmine.createSpyObj('ApiService', ['get', 'post']);
    apiSpy.get.and.returnValue(of({ success: true, data: {} }));
    apiSpy.post.and.returnValue(of({ success: true, data: {} }));

    storageSpy = jasmine.createSpyObj('StorageService', ['get', 'set']);
    storageSpy.get.and.returnValue(Promise.resolve(null));
    storageSpy.set.and.returnValue(Promise.resolve());

    toastSpy = jasmine.createSpyObj('ToastService', ['error', 'success', 'info', 'warning']);

    TestBed.configureTestingModule({
      providers: [
        { provide: ApiService, useValue: apiSpy },
        { provide: StorageService, useValue: storageSpy },
        { provide: ToastService, useValue: toastSpy },
      ],
    });
    service = TestBed.inject(CommerceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize session from storage if exists', async () => {
    // Constructor already called initSession via TestBed.inject
    // Storage.get is called with session key during init
    await new Promise((resolve) => setTimeout(resolve, 50));
    expect(storageSpy.get).toHaveBeenCalledWith('lyke_session_id');
  });

  it('should generate new session when no stored session', async () => {
    // storageSpy.get returns null by default, so a new session ID is generated
    await new Promise((resolve) => setTimeout(resolve, 50));

    expect(storageSpy.set).toHaveBeenCalled();
  });

  it('should call trackClick and return response on success', (done) => {
    const response = { clickId: 'c1', affiliateUrl: 'https://shop.com', retailerName: 'Store', productName: 'Item' };
    apiSpy.post.and.returnValue(of({ success: true, data: response }));

    const context: ClickContext = { source: 'feed', feedPosition: 3 };
    service.trackClick('post1', 'pp1', context).subscribe((r) => {
      expect(r).toEqual(response as any);
      expect(apiSpy.post).toHaveBeenCalledWith(
        'commerce',
        'clicks/track',
        jasmine.objectContaining({
          postId: 'post1',
          postProductId: 'pp1',
          source: 'feed',
          feedPosition: 3,
        })
      );
      done();
    });
  });

  it('should return null from trackClick on failure', (done) => {
    apiSpy.post.and.returnValue(of({ success: false }));

    const context: ClickContext = { source: 'post_detail' };
    service.trackClick('p1', 'pp1', context).subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  it('should call getProduct with correct endpoint', (done) => {
    const product = { id: 'prod1', name: 'Shirt' };
    apiSpy.get.and.returnValue(of({ success: true, data: product }));

    service.getProduct('prod1').subscribe((r) => {
      expect(r).toEqual(product as any);
      expect(apiSpy.get).toHaveBeenCalledWith(
        'commerce',
        'products/prod1',
        undefined,
        jasmine.any(Object)
      );
      done();
    });
  });

  it('should return null from getProduct on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false }));

    service.getProduct('prod1').subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  it('should call searchProducts with params', (done) => {
    const products = [{ id: 'p1' }, { id: 'p2' }];
    apiSpy.get.and.returnValue(of({ success: true, data: products }));

    service.searchProducts('shirt', 'r1', 'Tops', 2, 10).subscribe((r) => {
      expect(r).toEqual(products as any);
      expect(apiSpy.get).toHaveBeenCalledWith(
        'commerce',
        'products/search',
        jasmine.objectContaining({
          query: 'shirt',
          retailerId: 'r1',
          category: 'Tops',
          page: 2,
          pageSize: 10,
        }),
        jasmine.any(Object)
      );
      done();
    });
  });

  it('should return empty array from searchProducts on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false }));

    service.searchProducts('test').subscribe((r) => {
      expect(r).toEqual([]);
      done();
    });
  });

  it('should call getRetailers', (done) => {
    const retailers = [{ id: 'r1', name: 'Store' }];
    apiSpy.get.and.returnValue(of({ success: true, data: retailers }));

    service.getRetailers().subscribe((r) => {
      expect(r).toEqual(retailers as any);
      expect(apiSpy.get).toHaveBeenCalledWith('commerce', 'retailers', undefined, jasmine.any(Object));
      done();
    });
  });

  it('should call getRetailerProducts', (done) => {
    const response = { retailerId: 'r1', retailerName: 'Store', products: [{ id: 'p1' }] };
    apiSpy.get.and.returnValue(of({ success: true, data: response }));

    service.getRetailerProducts('r1', 1, 20).subscribe((r) => {
      expect(r).toEqual([{ id: 'p1' }] as any);
      expect(apiSpy.get).toHaveBeenCalledWith(
        'commerce',
        'retailers/r1/products',
        jasmine.objectContaining({ page: 1, pageSize: 20 }),
        jasmine.any(Object)
      );
      done();
    });
  });

  it('should reset session by generating new ID and storing it', async () => {
    await service.resetSession();
    expect(storageSpy.set).toHaveBeenCalledWith('lyke_session_id', jasmine.any(String));
  });

  it('should return raw search results with searchProductsRaw', (done) => {
    const products = [{ id: 'p1' }];
    apiSpy.get.and.returnValue(of({ success: true, data: products }));

    service.searchProductsRaw('test').subscribe((r) => {
      expect(r.success).toBeTrue();
      done();
    });
  });

  it('should return raw retailer products with getRetailerProductsRaw', (done) => {
    const response = { retailerId: 'r1', retailerName: 'Store', products: [{ id: 'p1' }] };
    apiSpy.get.and.returnValue(of({ success: true, data: response }));

    service.getRetailerProductsRaw('r1').subscribe((r) => {
      expect(r.data).toEqual([{ id: 'p1' }] as any);
      done();
    });
  });
});
