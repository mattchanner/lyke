import { Injectable, inject } from '@angular/core';
import { Observable, map, catchError, of } from 'rxjs';
import { ApiService } from './api.service';
import {
  CreatorProfileResponse,
  UpdateCreatorProfileRequest,
  CreatorPostResponse,
  CreatorPostsRequest,
  CreatePostRequest,
  UpdatePostRequest,
  CreatorAnalyticsRequest,
  CreatorAnalyticsResponse,
  EarningsSummaryResponse,
  EarningDetailResponse,
  EarningsHistoryRequest,
  VerificationStatusResponse,
  SubmitVerificationRequest,
  ApiResponse,
} from '../../models';

@Injectable({
  providedIn: 'root',
})
export class CreatorService {
  private readonly api = inject(ApiService);
  private readonly resource = 'creators';

  getProfile(): Observable<CreatorProfileResponse | null> {
    return this.api.get<CreatorProfileResponse>(this.resource, 'profile').pipe(
      map((r) => (r.success ? r.data ?? null : null)),
      catchError(() => of(null))
    );
  }

  updateProfile(
    request: UpdateCreatorProfileRequest
  ): Observable<ApiResponse<CreatorProfileResponse>> {
    return this.api.put<CreatorProfileResponse>(
      this.resource,
      'profile',
      request
    );
  }

  getPosts(
    request?: CreatorPostsRequest
  ): Observable<ApiResponse<CreatorPostResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.status !== undefined) params['status'] = request.status;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<CreatorPostResponse[]>(this.resource, 'posts', params);
  }

  getPost(id: string): Observable<CreatorPostResponse | null> {
    return this.api
      .get<CreatorPostResponse>(this.resource, `posts/${id}`)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  createPost(
    request: CreatePostRequest
  ): Observable<ApiResponse<CreatorPostResponse>> {
    return this.api.post<CreatorPostResponse>(
      this.resource,
      'posts',
      request
    );
  }

  updatePost(
    id: string,
    request: UpdatePostRequest
  ): Observable<ApiResponse<CreatorPostResponse>> {
    return this.api.put<CreatorPostResponse>(
      this.resource,
      `posts/${id}`,
      request
    );
  }

  deletePost(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>(this.resource, `posts/${id}`);
  }

  submitPost(id: string): Observable<ApiResponse<CreatorPostResponse>> {
    return this.api.post<CreatorPostResponse>(
      this.resource,
      `posts/${id}/submit`
    );
  }

  getAnalytics(
    request?: CreatorAnalyticsRequest
  ): Observable<CreatorAnalyticsResponse | null> {
    const params: Record<string, unknown> = {};
    if (request?.startDate) params['startDate'] = request.startDate;
    if (request?.endDate) params['endDate'] = request.endDate;
    return this.api
      .get<CreatorAnalyticsResponse>(this.resource, 'analytics', params)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  getEarnings(): Observable<EarningsSummaryResponse | null> {
    return this.api
      .get<EarningsSummaryResponse>(this.resource, 'earnings')
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  getEarningsHistory(
    request?: EarningsHistoryRequest
  ): Observable<ApiResponse<EarningDetailResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.status !== undefined) params['status'] = request.status;
    if (request?.startDate) params['startDate'] = request.startDate;
    if (request?.endDate) params['endDate'] = request.endDate;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<EarningDetailResponse[]>(
      this.resource,
      'earnings/history',
      params
    );
  }

  getVerificationStatus(): Observable<VerificationStatusResponse | null> {
    return this.api
      .get<VerificationStatusResponse>(this.resource, 'verification')
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  submitVerification(
    request: SubmitVerificationRequest
  ): Observable<ApiResponse<VerificationStatusResponse>> {
    return this.api.post<VerificationStatusResponse>(
      this.resource,
      'verification',
      request
    );
  }
}
