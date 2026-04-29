import { Component, inject, signal, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonButton,
  IonIcon,
  IonItem,
  IonInput,
  IonTextarea,
  IonFooter,
  IonSpinner,
  IonNote,
  IonList,
  IonLabel,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  cloudUploadOutline,
  addOutline,
  closeOutline,
  saveOutline,
  sendOutline,
  searchOutline,
} from 'ionicons/icons';
import { PostAuthorService, ToastService, ApiService, HasUnsavedChanges } from '../../core';
import { CommerceService } from '../../core/services/commerce.service';
import {
  MediaType,
  FitRating,
  MediaUploadResponse,
  ProductResponse,
} from '../../models';
import {
  MediaCarouselComponent,
  MediaItem,
} from '../../shared/components/media-carousel';
import { FitRatingPillsComponent } from '../../shared/components/fit-rating-pills';
import { convertToJpeg } from '../../shared/utils/image-convert.util';

const MAX_FILE_SIZE_BYTES = 20 * 1024 * 1024; // 20 MB per file

interface TaggedProduct {
  product: ProductResponse;
  sizeWorn: string;
  fitRating?: FitRating;
  fitNotes?: string;
  stylingNotes?: string;
}

@Component({
  selector: 'app-my-post-create',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonButton,
    IonIcon,
    IonItem,
    IonInput,
    IonTextarea,
    IonFooter,
    IonSpinner,
    IonNote,
    IonList,
    IonLabel,
    MediaCarouselComponent,
    FitRatingPillsComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './my-post-create.page.html',
  styleUrls: ['./my-post-create.page.scss'],
})
export class MyPostCreatePage implements HasUnsavedChanges {
  private readonly postAuthorService = inject(PostAuthorService);
  private readonly apiService = inject(ApiService);
  private readonly commerceService = inject(CommerceService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly FitRating = FitRating;

  // Media
  readonly mediaType = signal(MediaType.Image);
  readonly uploadedMedia = signal<MediaUploadResponse[]>([]);
  readonly isUploading = signal(false);

  // Details
  readonly title = signal('');
  readonly description = signal('');

  // Product search
  readonly productSearchQuery = signal('');
  readonly searchResults = signal<ProductResponse[]>([]);
  readonly isSearching = signal(false);

  // Products
  readonly taggedProducts = signal<TaggedProduct[]>([]);

  // Submit
  readonly isSubmitting = signal(false);

  readonly mediaItems = computed<MediaItem[]>(() =>
    this.uploadedMedia().map((m) => ({
      url: m.originalUrl,
      type: this.mediaType(),
      thumbnailUrl: m.thumbnailUrl,
    }))
  );

  readonly hasMedia = computed(() => this.uploadedMedia().length > 0);

  constructor() {
    addIcons({
      cloudUploadOutline,
      addOutline,
      closeOutline,
      saveOutline,
      sendOutline,
      searchOutline,
    });
  }

  // --- Media ---

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    for (let i = 0; i < input.files.length; i++) {
      const f = input.files[i];
      if (f.size > MAX_FILE_SIZE_BYTES) {
        this.toast.warning(`${f.name || 'File'} is too large. Max 20MB per file.`);
        input.value = '';
        return;
      }
    }

    const file = input.files[0];
    const detectedType = file.type.startsWith('video/') ? MediaType.Video : MediaType.Image;

    if (this.uploadedMedia().length > 0 && detectedType !== this.mediaType()) {
      this.toast.warning(`You can only add ${this.mediaType() === MediaType.Image ? 'images' : 'videos'} to this post`);
      input.value = '';
      return;
    }

    this.mediaType.set(detectedType);
    this.isUploading.set(true);

    const formData = new FormData();
    for (let i = 0; i < input.files.length; i++) {
      const normalized = await convertToJpeg(input.files[i]);
      formData.append('files', normalized);
    }

    this.apiService
      .uploadFile<MediaUploadResponse>('media', 'upload', formData)
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            this.uploadedMedia.update((current) => [...current, r.data!]);
          } else {
            this.toast.error(r.error?.message || 'Upload failed');
          }
        },
        error: () => this.toast.error('Failed to upload media'),
        complete: () => {
          this.isUploading.set(false);
          input.value = '';
        },
      });
  }

  removeMedia(index: number): void {
    this.uploadedMedia.update((media) => media.filter((_, i) => i !== index));
  }

  get fileAccept(): string {
    if (this.uploadedMedia().length === 0) return 'image/*,video/*';
    return this.mediaType() === MediaType.Image ? 'image/*' : 'video/*';
  }

  get fileMultiple(): boolean {
    if (this.uploadedMedia().length === 0) return true;
    return this.mediaType() === MediaType.Image;
  }

  // --- Product Search ---

  searchProducts(): void {
    const query = this.productSearchQuery().trim();
    if (!query) return;

    this.isSearching.set(true);
    this.commerceService.searchProducts(query).subscribe({
      next: (products) => this.searchResults.set(products),
      complete: () => this.isSearching.set(false),
    });
  }

  addProduct(product: ProductResponse): void {
    if (this.taggedProducts().some((tp) => tp.product.id === product.id)) {
      this.toast.warning('Product already added');
      return;
    }
    this.taggedProducts.update((products) => [
      ...products,
      { product, sizeWorn: '', fitRating: undefined, fitNotes: '', stylingNotes: '' },
    ]);
    this.searchResults.set([]);
    this.productSearchQuery.set('');
  }

  removeProduct(index: number): void {
    this.taggedProducts.update((products) => products.filter((_, i) => i !== index));
  }

  updateProductSize(index: number, size: string): void {
    this.taggedProducts.update((products) =>
      products.map((p, i) => (i === index ? { ...p, sizeWorn: size } : p))
    );
  }

  updateProductFitRating(index: number, rating: FitRating): void {
    this.taggedProducts.update((products) =>
      products.map((p, i) => (i === index ? { ...p, fitRating: rating } : p))
    );
  }

  updateProductFitNotes(index: number, notes: string): void {
    this.taggedProducts.update((products) =>
      products.map((p, i) => (i === index ? { ...p, fitNotes: notes } : p))
    );
  }

  updateProductStylingNotes(index: number, notes: string): void {
    this.taggedProducts.update((products) =>
      products.map((p, i) => (i === index ? { ...p, stylingNotes: notes } : p))
    );
  }

  // --- Unsaved-changes guard support ---

  hasUnsavedChanges(): boolean {
    return (
      this.uploadedMedia().length > 0 ||
      this.title().trim().length > 0 ||
      this.description().trim().length > 0 ||
      this.taggedProducts().length > 0
    );
  }

  // --- Submit ---

  saveAsDraft(): void {
    this.submitForm(false);
  }

  submitForReview(): void {
    this.submitForm(true);
  }

  private submitForm(submitAfterCreate: boolean): void {
    this.isSubmitting.set(true);

    this.postAuthorService
      .createPost({
        title: this.title() || undefined,
        description: this.description() || undefined,
        mediaType: this.mediaType(),
        mediaUrls: this.uploadedMedia().map((m) => m.originalUrl),
        thumbnailUrls: this.uploadedMedia().map((m) => m.thumbnailUrl),
        products: this.taggedProducts().map((tp) => ({
          productId: tp.product.id,
          sizeWorn: tp.sizeWorn,
          fitRating: tp.fitRating,
          fitNotes: tp.fitNotes || undefined,
          stylingNotes: tp.stylingNotes || undefined,
        })),
      })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (submitAfterCreate) {
              this.postAuthorService.submitPost(r.data.id).subscribe({
                next: (sr) => {
                  if (sr.success) {
                    this.toast.success('Post submitted for review');
                  } else {
                    this.toast.success('Post saved as draft (submit failed)');
                  }
                  this.router.navigate(['/my-posts']);
                },
                error: () => {
                  this.toast.success('Draft saved. You can finish it any time.');
                  this.router.navigate(['/my-posts']);
                },
                complete: () => this.isSubmitting.set(false),
              });
            } else {
              this.toast.success('Draft saved. You can finish it any time.');
              this.isSubmitting.set(false);
              this.router.navigate(['/my-posts']);
            }
          } else {
            this.toast.error(r.error?.message || 'Failed to create post');
            this.isSubmitting.set(false);
          }
        },
        error: () => {
          this.toast.error('Failed to create post');
          this.isSubmitting.set(false);
        },
      });
  }
}
