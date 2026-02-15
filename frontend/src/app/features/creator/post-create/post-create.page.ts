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
  IonLabel,
  IonInput,
  IonTextarea,
  IonSelect,
  IonSelectOption,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonChip,
  IonBadge,
  IonList,
  IonNote,
  IonProgressBar,
  IonSpinner,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  cloudUploadOutline,
  imageOutline,
  videocamOutline,
  trashOutline,
  addOutline,
  searchOutline,
  arrowForwardOutline,
  arrowBackOutline,
  checkmarkOutline,
  saveOutline,
  sendOutline,
  closeOutline,
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
    IonLabel,
    IonInput,
    IonTextarea,
    IonSelect,
    IonSelectOption,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonChip,
    IonBadge,
    IonList,
    IonNote,
    IonProgressBar,
    IonSpinner,
    MediaCarouselComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './post-create.page.html',
  styleUrls: ['./post-create.page.scss'],
})
export class PostCreatePage {
  private readonly creatorService = inject(CreatorService);
  private readonly commerceService = inject(CommerceService);
  private readonly apiService = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly FitRating = FitRating;
  readonly MediaType = MediaType;

  // Wizard state
  readonly currentStep = signal(1);
  readonly totalSteps = 4;

  // Step 1: Media
  readonly mediaType = signal(MediaType.Image);
  readonly uploadedMedia = signal<MediaUploadResponse[]>([]);
  readonly isUploading = signal(false);
  readonly uploadProgress = signal(0);

  // Step 2: Details
  readonly title = signal('');
  readonly description = signal('');

  // Step 3: Products
  readonly productSearchQuery = signal('');
  readonly searchResults = signal<ProductResponse[]>([]);
  readonly isSearching = signal(false);
  readonly taggedProducts = signal<TaggedProduct[]>([]);

  // Step 4: Submit
  readonly isSubmitting = signal(false);

  readonly reviewMediaItems = computed<MediaItem[]>(() =>
    this.uploadedMedia().map((m) => ({
      url: m.originalUrl,
      type: this.mediaType(),
      thumbnailUrl: m.thumbnailUrl,
    }))
  );

  readonly canProceed = computed(() => {
    switch (this.currentStep()) {
      case 1: return this.uploadedMedia().length > 0;
      case 2: return true; // Title and description are optional
      case 3: return true; // Products are optional but recommended
      case 4: return true;
      default: return false;
    }
  });

  readonly stepTitle = computed(() => {
    switch (this.currentStep()) {
      case 1: return 'Upload Media';
      case 2: return 'Post Details';
      case 3: return 'Tag Products';
      case 4: return 'Review & Submit';
      default: return 'Create Post';
    }
  });

  constructor() {
    addIcons({
      cloudUploadOutline,
      imageOutline,
      videocamOutline,
      trashOutline,
      addOutline,
      searchOutline,
      arrowForwardOutline,
      arrowBackOutline,
      checkmarkOutline,
      saveOutline,
      sendOutline,
      closeOutline,
    });
  }

  nextStep(): void {
    if (this.currentStep() < this.totalSteps) {
      this.currentStep.update((s) => s + 1);
    }
  }

  prevStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update((s) => s - 1);
    }
  }

  // Step 1: Media upload
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const formData = new FormData();
    for (let i = 0; i < input.files.length; i++) {
      formData.append('files', input.files[i]);
    }

    this.isUploading.set(true);
    this.uploadProgress.set(0);

    this.apiService
      .uploadFile<MediaUploadResponse>('media', 'upload', formData)
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            this.uploadedMedia.update((current) => [...current, r.data!]);
            this.toast.success('Media uploaded successfully');
          } else {
            this.toast.error(r.error?.message || 'Upload failed');
          }
        },
        error: () => this.toast.error('Failed to upload media'),
        complete: () => {
          this.isUploading.set(false);
          this.uploadProgress.set(0);
          input.value = '';
        },
      });
  }

  removeMedia(index: number): void {
    this.uploadedMedia.update((media) => media.filter((_, i) => i !== index));
  }

  // Step 3: Product search & tagging
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
    const alreadyAdded = this.taggedProducts().some(
      (tp) => tp.product.id === product.id
    );
    if (alreadyAdded) {
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

  getFitRatingLabel(rating: FitRating): string {
    switch (rating) {
      case FitRating.TooSmall: return 'Too Small';
      case FitRating.SlightlySmall: return 'Slightly Small';
      case FitRating.TrueToSize: return 'True to Size';
      case FitRating.SlightlyLarge: return 'Slightly Large';
      case FitRating.TooLarge: return 'Too Large';
      default: return 'Unknown';
    }
  }

  // Step 4: Save / Submit
  saveAsDraft(): void {
    this.submitForm(false);
  }

  submitForReview(): void {
    this.submitForm(true);
  }

  private submitForm(submitAfterCreate: boolean): void {
    const products: PostProductRequest[] = this.taggedProducts()
      .filter((tp) => tp.sizeWorn.trim().length > 0)
      .map((tp) => ({
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
