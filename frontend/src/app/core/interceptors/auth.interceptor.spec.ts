import { TestBed, fakeAsync, flushMicrotasks } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { StorageService } from '../services/storage.service';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let storageSpy: jasmine.SpyObj<StorageService>;
  const apiBaseUrl = 'http://localhost:5104/api';

  beforeEach(() => {
    storageSpy = jasmine.createSpyObj('StorageService', ['getAccessToken']);
    storageSpy.getAccessToken.and.returnValue(Promise.resolve('test-token'));

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: StorageService, useValue: storageSpy },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add Authorization header for API URLs', fakeAsync(() => {
    let completed = false;
    httpClient.get(`${apiBaseUrl}/profile/v1/me`).subscribe(() => (completed = true));

    flushMicrotasks();

    const req = httpMock.expectOne(`${apiBaseUrl}/profile/v1/me`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
    req.flush({});
    expect(completed).toBeTrue();
  }));

  it('should skip header for non-API URLs', (done) => {
    httpClient.get('https://external-api.com/data').subscribe(() => done());

    const req = httpMock.expectOne('https://external-api.com/data');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('should skip header for public login endpoint', (done) => {
    httpClient.post(`${apiBaseUrl}/auth/v1/login`, {}).subscribe(() => done());

    const req = httpMock.expectOne(`${apiBaseUrl}/auth/v1/login`);
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('should skip header for public register endpoint', (done) => {
    httpClient.post(`${apiBaseUrl}/auth/v1/register`, {}).subscribe(() => done());

    const req = httpMock.expectOne(`${apiBaseUrl}/auth/v1/register`);
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('should proceed without header when no token stored', fakeAsync(() => {
    storageSpy.getAccessToken.and.returnValue(Promise.resolve(null));

    let completed = false;
    httpClient.get(`${apiBaseUrl}/profile/v1/me`).subscribe(() => (completed = true));

    flushMicrotasks();

    const req = httpMock.expectOne(`${apiBaseUrl}/profile/v1/me`);
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
    expect(completed).toBeTrue();
  }));
});
