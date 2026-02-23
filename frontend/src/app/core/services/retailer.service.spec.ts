import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { RetailerService } from './retailer.service';
import { ApiService } from './api.service';

describe('RetailerService', () => {
  let service: RetailerService;
  let apiSpy: jasmine.SpyObj<ApiService>;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    apiSpy = jasmine.createSpyObj('ApiService', ['get', 'post', 'put', 'uploadFile']);
    apiSpy.get.and.returnValue(of({ success: true, data: {} }));
    apiSpy.post.and.returnValue(of({ success: true, data: {} }));
    apiSpy.put.and.returnValue(of({ success: true, data: {} }));
    apiSpy.uploadFile.and.returnValue(of({ success: true, data: {} }));

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [{ provide: ApiService, useValue: apiSpy }],
    });
    service = TestBed.inject(RetailerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should call register', () => {
    const req = { companyName: 'Store' } as any;
    service.register(req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('retailers', 'portal/register', req);
  });

  it('should call getProfile and return data', (done) => {
    const profile = { id: 'r1', companyName: 'Store' };
    apiSpy.get.and.returnValue(of({ success: true, data: profile } as any));

    service.getProfile().subscribe((r) => {
      expect(r).toEqual(profile as any);
      expect(apiSpy.get).toHaveBeenCalledWith('retailers', 'portal/profile');
      done();
    });
  });

  it('should return null from getProfile on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));
    service.getProfile().subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  it('should call updateProfile', () => {
    const req = { companyName: 'New Name' } as any;
    service.updateProfile(req).subscribe();
    expect(apiSpy.put).toHaveBeenCalledWith('retailers', 'portal/profile', req);
  });

  it('should call importProducts with FormData', () => {
    const file = new File(['csv data'], 'products.csv', { type: 'text/csv' });
    service.importProducts(file).subscribe();
    expect(apiSpy.uploadFile).toHaveBeenCalledWith('retailers', 'portal/products/import', jasmine.any(FormData));
  });

  it('should call getProducts with filter params', () => {
    service.getProducts({ category: 'Tops', search: 'shirt', page: 1, pageSize: 20 } as any).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('retailers', 'portal/products', jasmine.objectContaining({
      category: 'Tops',
      search: 'shirt',
      page: 1,
      pageSize: 20,
    }));
  });

  it('should call updateProduct', () => {
    const req = { name: 'Updated Product' } as any;
    service.updateProduct('prod1', req).subscribe();
    expect(apiSpy.put).toHaveBeenCalledWith('retailers', 'portal/products/prod1', req);
  });

  it('should call createCampaign', () => {
    const req = { name: 'Campaign' } as any;
    service.createCampaign(req).subscribe();
    expect(apiSpy.post).toHaveBeenCalledWith('retailers', 'portal/campaigns', req);
  });

  it('should call getCampaigns with params', () => {
    service.getCampaigns({ isActive: true, page: 1, pageSize: 10 } as any).subscribe();
    expect(apiSpy.get).toHaveBeenCalledWith('retailers', 'portal/campaigns', jasmine.objectContaining({
      isActive: true,
      page: 1,
      pageSize: 10,
    }));
  });

  it('should call updateCampaign', () => {
    const req = { name: 'Updated' } as any;
    service.updateCampaign('c1', req).subscribe();
    expect(apiSpy.put).toHaveBeenCalledWith('retailers', 'portal/campaigns/c1', req);
  });

  it('should call getAnalytics with date params', (done) => {
    const analytics = { totalClicks: 50 };
    apiSpy.get.and.returnValue(of({ success: true, data: analytics } as any));

    service.getAnalytics({ startDate: '2026-01-01', endDate: '2026-01-31' }).subscribe((r) => {
      expect(r).toEqual(analytics as any);
      done();
    });
  });

  it('should return null from getAnalytics on failure', (done) => {
    apiSpy.get.and.returnValue(of({ success: false } as any));
    service.getAnalytics().subscribe((r) => {
      expect(r).toBeNull();
      done();
    });
  });

  it('should call exportAnalyticsCsv with direct HttpClient GET', () => {
    service.exportAnalyticsCsv({ startDate: '2026-01-01', endDate: '2026-01-31' }).subscribe();

    const req = httpMock.expectOne((r) =>
      r.url.includes('retailers/v1/portal/analytics/export')
    );
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    expect(req.request.params.get('startDate')).toBe('2026-01-01');
    req.flush(new Blob(['csv']));
  });

  it('should call getFitInsights', (done) => {
    const insights = { averageFitRating: 3.5 };
    apiSpy.get.and.returnValue(of({ success: true, data: insights } as any));

    service.getFitInsights().subscribe((r) => {
      expect(r).toEqual(insights as any);
      expect(apiSpy.get).toHaveBeenCalledWith('retailers', 'portal/insights/fit');
      done();
    });
  });

  it('should call getBodyProfileInsights', (done) => {
    const insights = { profiles: [] };
    apiSpy.get.and.returnValue(of({ success: true, data: insights } as any));

    service.getBodyProfileInsights().subscribe((r) => {
      expect(r).toEqual(insights as any);
      expect(apiSpy.get).toHaveBeenCalledWith('retailers', 'portal/insights/body-profiles');
      done();
    });
  });
});
