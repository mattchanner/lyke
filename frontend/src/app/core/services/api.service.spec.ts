import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpContext, HttpContextToken } from '@angular/common/http';
import { ApiService } from './api.service';
import { ApiResponse } from '../../models';

describe('ApiService', () => {
  let service: ApiService;
  let httpMock: HttpTestingController;
  const baseUrl = 'http://localhost:5104/api';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // URL building
  it('should build URL with resource/v1/endpoint', () => {
    service.get('creators', 'posts').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/creators/v1/posts`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, data: [] });
  });

  // Param serialization
  it('should skip null and undefined params', () => {
    service.get('res', 'ep', { a: 'yes', b: null, c: undefined }).subscribe();

    const req = httpMock.expectOne((r) => r.url === `${baseUrl}/res/v1/ep`);
    expect(req.request.params.get('a')).toBe('yes');
    expect(req.request.params.has('b')).toBeFalse();
    expect(req.request.params.has('c')).toBeFalse();
    req.flush({ success: true });
  });

  it('should stringify number params', () => {
    service.get('res', 'ep', { page: 2, pageSize: 20 }).subscribe();

    const req = httpMock.expectOne((r) => r.url === `${baseUrl}/res/v1/ep`);
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('20');
    req.flush({ success: true });
  });

  it('should append array params with same key', () => {
    service.get('res', 'ep', { ids: ['a', 'b', 'c'] }).subscribe();

    const req = httpMock.expectOne((r) => r.url === `${baseUrl}/res/v1/ep`);
    expect(req.request.params.getAll('ids')).toEqual(['a', 'b', 'c']);
    req.flush({ success: true });
  });

  // HTTP methods
  it('should send GET request', () => {
    service.get('test', 'endpoint').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/test/v1/endpoint`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true });
  });

  it('should send POST request with body', () => {
    const body = { name: 'test' };
    service.post('test', 'endpoint', body).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/test/v1/endpoint`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(body);
    req.flush({ success: true });
  });

  it('should send PUT request with body', () => {
    const body = { name: 'updated' };
    service.put('test', 'endpoint', body).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/test/v1/endpoint`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(body);
    req.flush({ success: true });
  });

  it('should send PATCH request with body', () => {
    const body = { status: 'active' };
    service.patch('test', 'endpoint', body).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/test/v1/endpoint`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual(body);
    req.flush({ success: true });
  });

  it('should send DELETE request', () => {
    service.delete('test', 'endpoint').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/test/v1/endpoint`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true });
  });

  it('should send DELETE request with body', () => {
    const body = { reason: 'test' };
    service.delete('test', 'endpoint', body).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/test/v1/endpoint`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.body).toEqual(body);
    req.flush({ success: true });
  });

  // getData
  it('should unwrap data from successful response', (done) => {
    service.getData<string[]>('test', 'items').subscribe((data) => {
      expect(data).toEqual(['a', 'b']);
      done();
    });

    const req = httpMock.expectOne(`${baseUrl}/test/v1/items`);
    req.flush({ success: true, data: ['a', 'b'] } as ApiResponse<string[]>);
  });

  it('should throw on success: false in getData', (done) => {
    service.getData('test', 'items').subscribe({
      error: (err) => {
        expect(err.message).toBe('Something went wrong');
        done();
      },
    });

    const req = httpMock.expectOne(`${baseUrl}/test/v1/items`);
    req.flush({ success: false, error: { code: 'ERR', message: 'Something went wrong' } });
  });

  // uploadFile
  it('should POST FormData for uploadFile', () => {
    const formData = new FormData();
    formData.append('file', new Blob(['data']), 'test.jpg');

    service.uploadFile('media', 'upload', formData).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/media/v1/upload`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBeTrue();
    req.flush({ success: true });
  });

  // getPaginated
  it('should merge page/pageSize with additional params', () => {
    service.getPaginated('posts', 'list', { page: 2, pageSize: 10 }, { status: 'active' }).subscribe();

    const req = httpMock.expectOne((r) => r.url === `${baseUrl}/posts/v1/list`);
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('status')).toBe('active');
    req.flush({ success: true, data: [] });
  });

  it('should use default page=1 pageSize=20 when not specified', () => {
    service.getPaginated('posts', 'list', {}).subscribe();

    const req = httpMock.expectOne((r) => r.url === `${baseUrl}/posts/v1/list`);
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('20');
    req.flush({ success: true, data: [] });
  });

  // HttpContext forwarding
  it('should forward HttpContext when provided', () => {
    const TEST_TOKEN = new HttpContextToken<boolean>(() => false);
    const context = new HttpContext().set(TEST_TOKEN, true);

    service.get('test', 'ep', undefined, { context }).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/test/v1/ep`);
    expect(req.request.context.get(TEST_TOKEN)).toBeTrue();
    req.flush({ success: true });
  });
});
