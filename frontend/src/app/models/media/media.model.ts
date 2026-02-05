export interface MediaUploadResponse {
  mediaId: string;
  originalUrl: string;
  standardUrl: string | null;
  thumbnailUrl: string;
  contentType: string;
  sizeBytes: number;
  width: number;
  height: number;
  durationSeconds: number | null;
  isVideo: boolean;
}

export interface BulkMediaUploadResponse {
  succeeded: MediaUploadResponse[];
  failed: MediaUploadError[];
}

export interface MediaUploadError {
  fileName: string;
  errorCode: string;
  errorMessage: string;
}

export interface MediaDeleteRequest {
  mediaIds: string[];
}
