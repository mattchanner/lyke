import { Component, inject, signal, computed, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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
  IonBadge,
  IonList,
  IonNote,
  IonSpinner,
  IonSkeletonText,
  AlertController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  trashOutline,
  addOutline,
  searchOutline,
  saveOutline,
  sendOutline,
  closeOutline,
  lockClosedOutline,
} from 'ionicons/icons';
import { PostAuthorService, ToastService } from '../../core';
import { CommerceService } from '../../core/services/commerce.service';
import {
  AuthorPostResponse,
  PostStatus,
  MediaType,
  FitRating,
  ProductResponse,
} from '../../models';
import {
  MediaCarouselComponent,
  MediaItem,
} from '../../shared/components/media-carousel';

interface TaggedProduct {
  product: { id: string; name: string; retailerName: string; price: number; currency: string; imageUrls: string[] };
  sizeWorn: string;
  fitRating?: FitRating;
  fitNotes?: string;
  stylingNotes?: string;
}

@Component({
  selector: 'app-my-post-edit',
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
    IonBadge,
    IonList,
    IonNote,
    IonSpinner,
    IonSkeletonText,
    MediaCarouselComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './my-post-edit.page.html',
  styleUrls: ['./my-post-edit.page.scss'],
})
export class MyPostEditPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly postAuthorService = inject(PostAuthorService);
  private readonly commerceService = inject(CommerceService);
  private readonly toast = inject(ToastService);
  private readonly alertCtrl = inject(AlertController);

  readonly FitRating = FitRating;
  readonly PostStatus = PostStatus;
  readonly MediaType = MediaType;

  readonly post = signal<AuthorPostResponse | null>(null);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);

  // Editable fields
  readonly title = signal('');
  readonly description = signal('');
  readonly mediaType = signal(MediaType.Image);
  readonly mediaUrls = signal<string[]>([]);
  readonly thumbnailUrls = signal<string[]>([]);
  readonly taggedProducts = signal<TaggedProduct[]>([]);

  // Product search
  readonly productSearchQuery = signal('');
  readonly searchResults = signal<ProductResponse[]>([]);
  readonly isSearching = signal(false);

  readonly mediaItems = computed<MediaItem[]>(() =>
    this.mediaUrls().map((url, i) => ({
      url,
      type: this.mediaType(),
      thumbnailUrl: this.thumbnailUrls()?.[i],
    }))
  );

  readonly isEditable = computed(() => {
    const p = this.post();
    return p?.status === PostStatus.Draft || p?.status === PostStatus.Rejected;
  });

  constructor() {
    addIcons({
      trashOutline,
      addOutline,
      searchOutline,
      saveOutline,
      sendOutline,
      closeOutline,
      lockClosedOutline,
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadPost(id);
    }
  }

  private loadPost(id: string): void {
    this.isLoading.set(true);
    this.postAuthorService.getPost(id).subscribe({
      next: (r) => {
        if (r.success && r.data) {
          const post = r.data;
          this.post.set(post);
          this.title.set(post.title || '');
          this.description.set(post.description || '');
          this.mediaType.set(post.mediaType);
          this.mediaUrls.set([...post.mediaUrls]);
          this.thumbnailUrls.set([...post.thumbnailUrls]);
          this.taggedProducts.set(
            post.products.map((pp) => ({
              product: {
                id: pp.productId,
                name: pp.productName,
                retailerName: pp.retailerName,
                price: pp.productPrice,
                currency: pp.productCurrency,
                imageUrls: pp.productImage ? [pp.productImage] : [],
              },
              sizeWorn: pp.sizeWorn,
              fitRating: pp.fitRating ?? undefined,
              fitNotes: pp.fitNotes || '',
              stylingNotes: pp.stylingNotes || '',
            }))
          );
        } else {
          this.toast.error('Post not found');
          this.router.navigate(['/my-posts']);
        }
      },
      error: () => {
        this.toast.error('Failed to load post');
        this.router.navigate(['/my-posts']);
      },
      complete: () => this.isLoading.set(false),
    });
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
      {
        product: {
          id: product.id,
          name: product.name,
          retailerName: product.retailerName,
          price: product.price,
          currency: product.currency,
          imageUrls: product.imageUrls,
        },
        sizeWorn: '',
        fitRating: undefined,
        fitNotes: '',
        stylingNotes: '',
      },
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

  // --- Save / Submit ---

  getStatusColor(status: PostStatus): string {
    switch (status) {
      case PostStatus.Draft: return 'medium';
      case PostStatus.PendingReview: return 'warning';
      case PostStatus.Published: return 'success';
      case PostStatus.Rejected: return 'danger';
      default: return 'medium';
    }
  }

  getStatusLabel(status: PostStatus): string {
    switch (status) {
      case PostStatus.Draft: return 'Draft';
      case PostStatus.PendingReview: return 'Pending Review';
      case PostStatus.Published: return 'Published';
      case PostStatus.Rejected: return 'Rejected';
      default: return 'Unknown';
    }
  }

  saveChanges(): void {
    const post = this.post();
    if (!post) return;

    this.isSaving.set(true);

    this.postAuthorService
      .updatePost(post.id, {
        title: this.title() || undefined,
        description: this.description() || undefined,
        mediaType: this.mediaType(),
        mediaUrls: this.mediaUrls(),
        thumbnailUrls: this.thumbnailUrls(),
        products: this.taggedProducts()
          .filter((tp) => tp.sizeWorn.trim().length > 0)
          .map((tp) => ({
            productId: tp.product.id,
            sizeWorn: tp.sizeWorn,
            fitRating: tp.fitRating,
            fitNotes: tp.fitNotes || undefined,
            stylingNotes: tp.stylingNotes || undefined,
          })),
      })
      .subscribe({
        next: (r) => {
          if (r.success) {
            this.toast.success('Post updated');
            if (r.data) this.post.set(r.data);
          } else {
            this.toast.error(r.error?.message || 'Failed to save');
          }
        },
        error: () => this.toast.error('Failed to save changes'),
        complete: () => this.isSaving.set(false),
      });
  }

  submitForReview(): void {
    const post = this.post();
    if (!post) return;

    this.isSaving.set(true);

    this.postAuthorService
      .updatePost(post.id, {
        title: this.title() || undefined,
        description: this.description() || undefined,
        mediaType: this.mediaType(),
        mediaUrls: this.mediaUrls(),
        thumbnailUrls: this.thumbnailUrls(),
        products: this.taggedProducts()
          .filter((tp) => tp.sizeWorn.trim().length > 0)
          .map((tp) => ({
            productId: tp.product.id,
            sizeWorn: tp.sizeWorn,
            fitRating: tp.fitRating,
            fitNotes: tp.fitNotes || undefined,
            stylingNotes: tp.stylingNotes || undefined,
          })),
      })
      .subscribe({
        next: (r) => {
          if (r.success) {
            this.postAuthorService.submitPost(post.id).subscribe({
              next: (sr) => {
                if (sr.success) {
                  this.toast.success('Post submitted for review');
                  this.router.navigate(['/my-posts']);
                } else {
                  this.toast.error(sr.error?.message || 'Submit failed');
                }
              },
              error: () => this.toast.error('Failed to submit'),
              complete: () => this.isSaving.set(false),
            });
          } else {
            this.toast.error(r.error?.message || 'Failed to save');
            this.isSaving.set(false);
          }
        },
        error: () => {
          this.toast.error('Failed to save changes');
          this.isSaving.set(false);
        },
      });
  }

  async deletePost(): Promise<void> {
    const post = this.post();
    if (!post) return;

    const alert = await this.alertCtrl.create({
      header: 'Delete Post',
      message: 'Are you sure you want to delete this post? This cannot be undone.',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Delete',
          role: 'destructive',
          handler: () => {
            this.postAuthorService.deletePost(post.id).subscribe({
              next: () => {
                this.toast.success('Post deleted');
                this.router.navigate(['/my-posts']);
              },
              error: () => this.toast.error('Failed to delete post'),
            });
          },
        },
      ],
    });
    await alert.present();
  }
}
