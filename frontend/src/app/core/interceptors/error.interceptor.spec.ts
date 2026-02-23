import { TestBed, fakeAsync, tick, flushMicrotasks } from '@angular/core/testing';
import { HttpClient, HttpContext, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { errorInterceptor, SUPPRESS_ERROR_TOAST } from './error.interceptor';
import { ToastService } from '../services/toast.service';
import { StorageService } from '../services/storage.service';
import { AppInsightsService } from '../services/app-insights.service';

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let toastSpy: jasmine.SpyObj<ToastService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let storageSpy: jasmine.SpyObj<StorageService>;
  let appInsightsSpy: jasmine.SpyObj<AppInsightsService>;

  beforeEach(() => {
    toastSpy = jasmine.createSpyObj('ToastService', ['error', 'success', 'warning', 'info']);
    routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    routerSpy.navigate.and.returnValue(Promise.resolve(true));
    storageSpy = jasmine.createSpyObj('StorageService', ['clearAuthData']);
    storageSpy.clearAuthData.and.returnValue(Promise.resolve());
    appInsightsSpy = jasmine.createSpyObj('AppInsightsService', ['trackException']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: ToastService, useValue: toastSpy },
        { provide: Router, useValue: routerSpy },
        { provide: StorageService, useValue: storageSpy },
        { provide: AppInsightsService, useValue: appInsightsSpy },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should pass through successful responses', (done) => {
    httpClient.get('/api/test').subscribe((data) => {
      expect(data).toEqual({ ok: true });
      done();
    });

    httpMock.expectOne('/api/test').flush({ ok: true });
  });

  it('should handle status 0 (network error)', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toContain('Unable to connect');
        expect(toastSpy.error).toHaveBeenCalledWith(jasmine.stringContaining('Unable to connect'));
        done();
      },
    });

    httpMock.expectOne('/api/test').error(new ProgressEvent('error'), { status: 0 });
  });

  it('should handle status 400 with server message', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toBe('Email is required');
        expect(toastSpy.error).toHaveBeenCalledWith('Email is required');
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      { error: { message: 'Email is required' } },
      { status: 400, statusText: 'Bad Request' }
    );
  });

  it('should handle status 401 by clearing auth and redirecting, no toast', fakeAsync(() => {
    let errorResult: any;
    httpClient.get('/api/test').subscribe({
      error: (err) => (errorResult = err),
    });

    httpMock.expectOne('/api/test').flush(
      { error: { message: 'Unauthorized' } },
      { status: 401, statusText: 'Unauthorized' }
    );

    flushMicrotasks();
    tick(100);

    expect(errorResult.status).toBe(401);
    expect(toastSpy.error).not.toHaveBeenCalled();
    expect(storageSpy.clearAuthData).toHaveBeenCalled();
  }));

  it('should handle status 403', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toContain('permission');
        expect(toastSpy.error).toHaveBeenCalled();
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      {},
      { status: 403, statusText: 'Forbidden' }
    );
  });

  it('should handle status 404 with server message', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toBe('Post not found');
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      { error: { message: 'Post not found' } },
      { status: 404, statusText: 'Not Found' }
    );
  });

  it('should handle status 409', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toBe('Email already exists');
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      { error: { message: 'Email already exists' } },
      { status: 409, statusText: 'Conflict' }
    );
  });

  it('should handle status 422', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toBe('Invalid data format');
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      { error: { message: 'Invalid data format' } },
      { status: 422, statusText: 'Unprocessable Entity' }
    );
  });

  it('should handle status 429', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toContain('Too many requests');
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      {},
      { status: 429, statusText: 'Too Many Requests' }
    );
  });

  it('should handle status 500', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toContain('Server error');
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      {},
      { status: 500, statusText: 'Internal Server Error' }
    );
  });

  it('should suppress toast when SUPPRESS_ERROR_TOAST is set', (done) => {
    const context = new HttpContext().set(SUPPRESS_ERROR_TOAST, true);
    httpClient.get('/api/test', { context }).subscribe({
      error: () => {
        expect(toastSpy.error).not.toHaveBeenCalled();
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      {},
      { status: 500, statusText: 'Internal Server Error' }
    );
  });

  it('should track exception to AppInsights on all errors', (done) => {
    httpClient.get('/api/test').subscribe({
      error: () => {
        expect(appInsightsSpy.trackException).toHaveBeenCalledWith(
          jasmine.any(Error),
          jasmine.objectContaining({ url: '/api/test', method: 'GET', status: '500' })
        );
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      {},
      { status: 500, statusText: 'Internal Server Error' }
    );
  });

  it('should handle default status codes', (done) => {
    httpClient.get('/api/test').subscribe({
      error: (err) => {
        expect(err.message).toContain('418');
        done();
      },
    });

    httpMock.expectOne('/api/test').flush(
      {},
      { status: 418, statusText: "I'm a teapot" }
    );
  });
});
