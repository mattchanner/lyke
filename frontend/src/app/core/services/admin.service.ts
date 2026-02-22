import { Injectable, inject } from '@angular/core';
import { Observable, map, catchError, of } from 'rxjs';
import { ApiService } from './api.service';
import {
  ApiResponse,
  PlatformStatsResponse,
  PendingPostResponse,
  ModeratePostRequest,
  PostModerationResponse,
  UserListRequest,
  UserListResponse,
  UserDetailResponse,
  SuspendUserRequest,
  UserSuspensionResponse,
  PendingVerificationResponse,
  ReviewVerificationRequest,
  ContentReportResponse,
  ContentReportQueryRequest,
  ReviewContentReportRequest,
  ModerationQueueItemResponse,
  BulkModeratePostsRequest,
  BulkSuspendUsersRequest,
  BulkActionResult,
  AdminAnalyticsRequest,
  AdminAnalyticsResponse,
  PostStatus,
  VerificationStatus,
} from '../../models';

export interface PendingPostsRequest {
  status?: PostStatus;
  page?: number;
  pageSize?: number;
}

export interface PendingVerificationsRequest {
  status?: VerificationStatus;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  private readonly api = inject(ApiService);
  private readonly resource = 'admin';

  // Platform stats
  getPlatformStats(): Observable<PlatformStatsResponse | null> {
    return this.api.get<PlatformStatsResponse>(this.resource, 'stats').pipe(
      map((r) => (r.success ? r.data ?? null : null)),
      catchError(() => of(null))
    );
  }

  // Platform analytics
  getPlatformAnalytics(
    request?: AdminAnalyticsRequest
  ): Observable<AdminAnalyticsResponse | null> {
    const params: Record<string, unknown> = {};
    if (request?.startDate) params['startDate'] = request.startDate;
    if (request?.endDate) params['endDate'] = request.endDate;
    return this.api
      .get<AdminAnalyticsResponse>(this.resource, 'analytics', params)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  // Post moderation
  getPendingPosts(
    request?: PendingPostsRequest
  ): Observable<ApiResponse<PendingPostResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.status !== undefined) params['status'] = request.status;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<PendingPostResponse[]>(this.resource, 'posts', params);
  }

  getPostForModeration(postId: string): Observable<PendingPostResponse | null> {
    return this.api
      .get<PendingPostResponse>(this.resource, `posts/${postId}`)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  moderatePost(
    postId: string,
    request: ModeratePostRequest
  ): Observable<ApiResponse<PostModerationResponse>> {
    return this.api.post<PostModerationResponse>(
      this.resource,
      `posts/${postId}/moderate`,
      request
    );
  }

  // User management
  getUsers(request?: UserListRequest): Observable<ApiResponse<UserListResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.userType !== undefined) params['userType'] = request.userType;
    if (request?.isActive !== undefined) params['isActive'] = request.isActive;
    if (request?.search) params['search'] = request.search;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<UserListResponse[]>(this.resource, 'users', params);
  }

  getUserDetail(userId: string): Observable<UserDetailResponse | null> {
    return this.api
      .get<UserDetailResponse>(this.resource, `users/${userId}`)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  suspendUser(
    userId: string,
    request: SuspendUserRequest
  ): Observable<ApiResponse<UserSuspensionResponse>> {
    return this.api.post<UserSuspensionResponse>(
      this.resource,
      `users/${userId}/suspend`,
      request
    );
  }

  unsuspendUser(userId: string): Observable<ApiResponse<UserSuspensionResponse>> {
    return this.api.post<UserSuspensionResponse>(
      this.resource,
      `users/${userId}/unsuspend`
    );
  }

  // Verification management
  getPendingVerifications(
    request?: PendingVerificationsRequest
  ): Observable<ApiResponse<PendingVerificationResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.status !== undefined) params['status'] = request.status;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<PendingVerificationResponse[]>(
      this.resource,
      'verifications',
      params
    );
  }

  getVerificationDetail(
    creatorId: string
  ): Observable<PendingVerificationResponse | null> {
    return this.api
      .get<PendingVerificationResponse>(this.resource, `verifications/${creatorId}`)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  reviewVerification(
    creatorId: string,
    request: ReviewVerificationRequest
  ): Observable<ApiResponse<void>> {
    return this.api.post<void>(
      this.resource,
      `verifications/${creatorId}/review`,
      request
    );
  }

  // Content reports
  getContentReports(
    request?: ContentReportQueryRequest
  ): Observable<ApiResponse<ContentReportResponse[]>> {
    const params: Record<string, unknown> = {};
    if (request?.status !== undefined) params['status'] = request.status;
    if (request?.reason !== undefined) params['reason'] = request.reason;
    if (request?.postId) params['postId'] = request.postId;
    if (request?.from) params['from'] = request.from;
    if (request?.to) params['to'] = request.to;
    if (request?.page) params['page'] = request.page;
    if (request?.pageSize) params['pageSize'] = request.pageSize;
    return this.api.get<ContentReportResponse[]>(this.resource, 'reports', params);
  }

  getContentReport(reportId: string): Observable<ContentReportResponse | null> {
    return this.api
      .get<ContentReportResponse>(this.resource, `reports/${reportId}`)
      .pipe(
        map((r) => (r.success ? r.data ?? null : null)),
        catchError(() => of(null))
      );
  }

  reviewContentReport(
    reportId: string,
    request: ReviewContentReportRequest
  ): Observable<ApiResponse<ContentReportResponse>> {
    return this.api.post<ContentReportResponse>(
      this.resource,
      `reports/${reportId}/review`,
      request
    );
  }

  // Moderation queue
  getModerationQueue(
    page = 1,
    pageSize = 20
  ): Observable<ApiResponse<ModerationQueueItemResponse[]>> {
    return this.api.get<ModerationQueueItemResponse[]>(
      this.resource,
      'moderation-queue',
      { page, pageSize }
    );
  }

  // Bulk actions
  bulkModeratePosts(
    request: BulkModeratePostsRequest
  ): Observable<ApiResponse<BulkActionResult>> {
    return this.api.post<BulkActionResult>(
      this.resource,
      'posts/bulk-moderate',
      request
    );
  }

  bulkSuspendUsers(
    request: BulkSuspendUsersRequest
  ): Observable<ApiResponse<BulkActionResult>> {
    return this.api.post<BulkActionResult>(
      this.resource,
      'users/bulk-suspend',
      request
    );
  }
}
