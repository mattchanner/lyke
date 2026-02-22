import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, PaginatedRequest } from '../../models';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;
  private readonly version = environment.apiVersion;

  private buildUrl(resource: string, endpoint: string): string {
    return `${this.baseUrl}/${resource}/${this.version}/${endpoint}`;
  }

  private buildParams(params?: Record<string, unknown>): HttpParams {
    let httpParams = new HttpParams();

    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          if (Array.isArray(value)) {
            value.forEach((item) => {
              httpParams = httpParams.append(key, String(item));
            });
          } else {
            httpParams = httpParams.set(key, String(value));
          }
        }
      });
    }

    return httpParams;
  }

  get<T>(
    resource: string,
    endpoint: string,
    params?: Record<string, unknown>,
    options?: { context?: HttpContext }
  ): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(this.buildUrl(resource, endpoint), {
      params: this.buildParams(params),
      context: options?.context,
    });
  }

  post<T>(
    resource: string,
    endpoint: string,
    body?: unknown,
    options?: { context?: HttpContext }
  ): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(
      this.buildUrl(resource, endpoint),
      body,
      { context: options?.context }
    );
  }

  put<T>(
    resource: string,
    endpoint: string,
    body?: unknown
  ): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(
      this.buildUrl(resource, endpoint),
      body
    );
  }

  patch<T>(
    resource: string,
    endpoint: string,
    body?: unknown
  ): Observable<ApiResponse<T>> {
    return this.http.patch<ApiResponse<T>>(
      this.buildUrl(resource, endpoint),
      body
    );
  }

  delete<T>(resource: string, endpoint: string, body?: unknown): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(this.buildUrl(resource, endpoint), { body });
  }

  // Convenience method to extract data from successful responses
  getData<T>(
    resource: string,
    endpoint: string,
    params?: Record<string, unknown>
  ): Observable<T> {
    return this.get<T>(resource, endpoint, params).pipe(
      map((response) => {
        if (!response.success || !response.data) {
          throw new Error(response.error?.message || 'Request failed');
        }
        return response.data;
      })
    );
  }

  // File upload helper
  uploadFile<T>(
    resource: string,
    endpoint: string,
    formData: FormData
  ): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(
      this.buildUrl(resource, endpoint),
      formData
    );
  }

  // Paginated request helper
  getPaginated<T>(
    resource: string,
    endpoint: string,
    pagination: PaginatedRequest,
    additionalParams?: Record<string, unknown>
  ): Observable<ApiResponse<T[]>> {
    const params = {
      ...additionalParams,
      page: pagination.page ?? 1,
      pageSize: pagination.pageSize ?? 20,
    };
    return this.get<T[]>(resource, endpoint, params);
  }
}
