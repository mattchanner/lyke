import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  ApiResponse,
  AuthorPostResponse,
  CreatePostRequest,
  UpdatePostRequest,
} from '../../models';

@Injectable({ providedIn: 'root' })
export class PostAuthorService {
  private readonly api = inject(ApiService);

  getPosts(params?: {
    status?: string;
    page?: number;
    pageSize?: number;
  }): Observable<ApiResponse<AuthorPostResponse[]>> {
    return this.api.get<AuthorPostResponse[]>('posts', 'my', params);
  }

  getPost(id: string): Observable<ApiResponse<AuthorPostResponse>> {
    return this.api.get<AuthorPostResponse>('posts', `my/${id}`);
  }

  createPost(request: CreatePostRequest): Observable<ApiResponse<AuthorPostResponse>> {
    return this.api.post<AuthorPostResponse>('posts', 'my', request);
  }

  updatePost(id: string, request: UpdatePostRequest): Observable<ApiResponse<AuthorPostResponse>> {
    return this.api.put<AuthorPostResponse>('posts', `my/${id}`, request);
  }

  deletePost(id: string): Observable<ApiResponse<void>> {
    return this.api.delete<void>('posts', `my/${id}`, {});
  }

  submitPost(id: string): Observable<ApiResponse<AuthorPostResponse>> {
    return this.api.post<AuthorPostResponse>('posts', `my/${id}/submit`, {});
  }
}
