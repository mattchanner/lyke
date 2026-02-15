import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiService } from './api.service';
import {
  RetailerProfileResponse,
  RegisterRetailerRequest,
  UpdateRetailerProfileRequest,
  RetailerProductResponse,
  RetailerProductsRequest,
  UpdateRetailerProductRequest,
  ImportProductsResponse,
  CampaignResponse,
  CreateCampaignRequest,
  UpdateCampaignRequest,
  CampaignListRequest,
  RetailerAnalyticsResponse,
  RetailerAnalyticsRequest,
  FitInsightsResponse,
  BodyProfileInsightsResponse,
  ApiResponse,
} from '../../models';

@Injectable({
  providedIn: 'root',
})
export class RetailerService {
  private readonly api = inject(ApiService);
  private readonly http = inject(HttpClient);
  private readonly resource = 'retailers';

  register(
    request: RegisterRetailerRequest
  ): Observable<ApiResponse<RetailerProfileResponse>> {
    return this.api.post<RetailerProfileResponse>(
      this.resource,
      'portal/register',
      request
    );
  }

  getProfile(): Observable<RetailerProfileResponse | null> {
    return this.api
      .get<RetailerProfileResponse>(this.resource, 'portal/profile')
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  updateProfile(
    request: UpdateRetailerProfileRequest
  ): Observable<ApiResponse<RetailerProfileResponse>> {
    return this.api.put<RetailerProfileResponse>(
      this.resource,
      'portal/profile',
      request
    );
  }

  importProducts(
    file: File
  ): Observable<ApiResponse<ImportProductsResponse>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.api.uploadFile<ImportProductsResponse>(
      this.resource,
      'portal/products/import',
      formData
    );
  }

  getProducts(
    request?: RetailerProductsRequest
  ): Observable<ApiResponse<RetailerProductResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.category) params['category'] = request.category;
    if (request?.search) params['search'] = request.search;
    if (request?.isActive !== undefined) params['isActive'] = request.isActive;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<RetailerProductResponse[]>(
      this.resource,
      'portal/products',
      params
    );
  }

  updateProduct(
    id: string,
    request: UpdateRetailerProductRequest
  ): Observable<ApiResponse<RetailerProductResponse>> {
    return this.api.put<RetailerProductResponse>(
      this.resource,
      `portal/products/${id}`,
      request
    );
  }

  createCampaign(
    request: CreateCampaignRequest
  ): Observable<ApiResponse<CampaignResponse>> {
    return this.api.post<CampaignResponse>(
      this.resource,
      'portal/campaigns',
      request
    );
  }

  getCampaigns(
    request?: CampaignListRequest
  ): Observable<ApiResponse<CampaignResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.isActive !== undefined) params['isActive'] = request.isActive;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<CampaignResponse[]>(
      this.resource,
      'portal/campaigns',
      params
    );
  }

  updateCampaign(
    id: string,
    request: UpdateCampaignRequest
  ): Observable<ApiResponse<CampaignResponse>> {
    return this.api.put<CampaignResponse>(
      this.resource,
      `portal/campaigns/${id}`,
      request
    );
  }

  getAnalytics(
    request?: RetailerAnalyticsRequest
  ): Observable<RetailerAnalyticsResponse | null> {
    const params: Record<string, unknown> = {};
    if (request?.startDate) params['startDate'] = request.startDate;
    if (request?.endDate) params['endDate'] = request.endDate;
    return this.api
      .get<RetailerAnalyticsResponse>(this.resource, 'portal/analytics', params)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  exportAnalyticsCsv(request?: RetailerAnalyticsRequest): Observable<Blob> {
    let params = new HttpParams();
    if (request?.startDate)
      params = params.set('startDate', request.startDate);
    if (request?.endDate) params = params.set('endDate', request.endDate);
    return this.http.get(
      `${environment.apiBaseUrl}/${this.resource}/${environment.apiVersion}/portal/analytics/export`,
      { params, responseType: 'blob' }
    );
  }

  getFitInsights(): Observable<FitInsightsResponse | null> {
    return this.api
      .get<FitInsightsResponse>(this.resource, 'portal/insights/fit')
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  getBodyProfileInsights(): Observable<BodyProfileInsightsResponse | null> {
    return this.api
      .get<BodyProfileInsightsResponse>(
        this.resource,
        'portal/insights/body-profiles'
      )
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }
}
