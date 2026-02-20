import { Component, inject, signal, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
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
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  cloudUploadOutline,
  trashOutline,
  addOutline,
  closeOutline,
  saveOutline,
  sendOutline,
  pricetagOutline,
} from 'ionicons/icons';
import { CreatorService, ToastService, ApiService } from '../../../core';
import { CommerceService } from '../../../core/services/commerce.service';
import {
  MediaType,
  FitRating,
  PostProductRequest,
  MediaUploadResponse,
  ProductResponse,
} from '../../../models';
import {
  MediaCarouselComponent,
  MediaItem,
} from '../../../shared/components/media-carousel';
import { FitRatingPillsComponent } from './fit-rating-pills.component';
import { ProductTagSheetComponent } from './product-tag-sheet.component';

interface TaggedProduct {
  product: ProductResponse;
  sizeWorn: string;
  fitRating?: FitRating;
  fitNotes?: string;
  stylingNotes?: string;
}

@Component({
  selector: 'app-post-create',
  standalone: true,
  imports: [
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
    MediaCarouselComponent,
    FitRatingPillsComponent,
    ProductTagSheetComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './post-create.page.html',
  styleUrls: ['./post-create.page.scss'],
})
export class PostCreatePage {
  private readonly creatorService = inject(CreatorService);
  private readonly apiService = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  // Media
  readonly mediaType = signal(MediaType.Image);
  readonly uploadedMedia = signal<MediaUploadResponse[]>([]);
  readonly isUploading = signal(false);

  // Details
  readonly title = signal('');
  readonly description = signal('');

  // Products
  readonly taggedProducts = signal<TaggedProduct[]>([]);
  readonly isProductSheetOpen = signal(false);
  readonly showNotes = signal<Record<string, boolean>>({});

  // Submit
  readonly isSubmitting = signal(false);

  readonly mediaItems = computed<MediaItem[]>(() =>
    this.uploadedMedia().map((m) => ({
      url: m.originalUrl,
      type: this.mediaType(),
      thumbnailUrl: m.thumbnailUrl,
    }))
  );

  readonly taggedProductIds = computed(() =>
    this.taggedProducts().map((tp) => tp.product.id)
  );

  readonly hasMedia = computed(() => this.uploadedMedia().length > 0);

  constructor() {
    addIcons({
      cloudUploadOutline,
      trashOutline,
      addOutline,
      closeOutline,
      saveOutline,
      sendOutline,
      pricetagOutline,
    });
  }

  // --- Media ---

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];
    const detectedType = this.detectMediaType(file);

    // If we already have media, reject mismatched types
    if (this.uploadedMedia().length > 0 && detectedType !== this.mediaType()) {
      this.toast.warning(`You can only add ${this.mediaType() === MediaType.Image ? 'images' : 'videos'} to this post`);
      input.value = '';
      return;
    }

    this.mediaType.set(detectedType);
    this.isUploading.set(true);

    const formData = new FormData();
    for (let i = 0; i < input.files.length; i++) {
      const normalized = await this.normalizeImageFile(input.files[i]);
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

  private detectMediaType(file: File): MediaType {
    return file.type.startsWith('video/') ? MediaType.Video : MediaType.Image;
  }

  private async normalizeImageFile(file: File): Promise<File> {
    const heifTypes = ['image/heif', 'image/heic', 'image/heif-sequence', 'image/heic-sequence'];
    const heifExtensions = ['.heif', '.heic'];
    const ext = file.name.toLowerCase().slice(file.name.lastIndexOf('.'));
    const isHeif = heifTypes.includes(file.type.toLowerCase()) || heifExtensions.includes(ext);
    if (!isHeif) return file;

    const bitmap = await createImageBitmap(file);
    const canvas = document.createElement('canvas');
    canvas.width = bitmap.width;
    canvas.height = bitmap.height;
    const ctx = canvas.getContext('2d')!;
    ctx.drawImage(bitmap, 0, 0);
    bitmap.close();

    const blob = await new Promise<Blob>((resolve) =>
      canvas.toBlob((b) => resolve(b!), 'image/jpeg', 0.92)
    );
    const name = file.name.replace(/\.hei[cf]$/i, '.jpg');
    return new File([blob], name, { type: 'image/jpeg' });
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

  // --- Products ---

  addProduct(product: ProductResponse): void {
    const alreadyAdded = this.taggedProducts().some(
      (tp) => tp.product.id === product.id
    );
    if (alreadyAdded) return;

    this.taggedProducts.update((products) => [
      ...products,
      { product, sizeWorn: '', fitRating: undefined, fitNotes: '', stylingNotes: '' },
    ]);
  }

  removeProduct(index: number): void {
    this.taggedProducts.update((products) =>
      products.filter((_, i) => i !== index)
    );
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

  toggleNotes(productId: string): void {
    this.showNotes.update((map) => ({ ...map, [productId]: !map[productId] }));
  }

  isNotesVisible(productId: string): boolean {
    return !!this.showNotes()[productId];
  }

  // --- Submit ---

  saveAsDraft(): void {
    this.submitForm(false);
  }

  submitForReview(): void {
    this.submitForm(true);
  }

  private submitForm(submitAfterCreate: boolean): void {
    const products: PostProductRequest[] = this.taggedProducts().map((tp) => ({
      productId: tp.product.id,
      sizeWorn: tp.sizeWorn,
      fitRating: tp.fitRating,
      fitNotes: tp.fitNotes || undefined,
      stylingNotes: tp.stylingNotes || undefined,
    }));

    this.isSubmitting.set(true);

    this.creatorService
      .createPost({
        title: this.title() || undefined,
        description: this.description() || undefined,
        mediaType: this.mediaType(),
        mediaUrls: this.uploadedMedia().map((m) => m.originalUrl),
        thumbnailUrls: this.uploadedMedia().map((m) => m.thumbnailUrl),
        products,
      })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (submitAfterCreate) {
              this.creatorService.submitPost(r.data.id).subscribe({
                next: (sr) => {
                  if (sr.success) {
                    this.toast.success('Post submitted for review');
                  } else {
                    this.toast.success('Post saved as draft (submit failed)');
                  }
                  this.router.navigate(['/creator/posts']);
                },
                error: () => {
                  this.toast.success('Post saved as draft');
                  this.router.navigate(['/creator/posts']);
                },
                complete: () => this.isSubmitting.set(false),
              });
            } else {
              this.toast.success('Post saved as draft');
              this.isSubmitting.set(false);
              this.router.navigate(['/creator/posts']);
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
