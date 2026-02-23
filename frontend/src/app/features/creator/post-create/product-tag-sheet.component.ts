import { Component, inject, input, output, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import {
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonButtons,
  IonButton,
  IonIcon,
  IonContent,
  IonSearchbar,
  IonList,
  IonItem,
  IonLabel,
  IonBadge,
  IonSpinner,
  IonNote,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { closeOutline, addOutline, checkmarkOutline } from 'ionicons/icons';
import { CommerceService } from '../../../core/services/commerce.service';
import { ProductResponse } from '../../../models';

@Component({
  selector: 'app-product-tag-sheet',
  standalone: true,
  imports: [
    DecimalPipe,
    IonModal,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonButton,
    IonIcon,
    IonContent,
    IonSearchbar,
    IonList,
    IonItem,
    IonLabel,
    IonBadge,
    IonSpinner,
    IonNote,
  ],
  template: `
    <ion-modal
      [isOpen]="isOpen()"
      (didDismiss)="close()"
      [initialBreakpoint]="0.65"
      [breakpoints]="[0, 0.65, 1]"
    >
      <ng-template>
        <ion-header>
          <ion-toolbar>
            <ion-buttons slot="start">
              <ion-button (click)="close()">
                <ion-icon name="close-outline" slot="icon-only"></ion-icon>
              </ion-button>
            </ion-buttons>
            <ion-title>Tag Products</ion-title>
          </ion-toolbar>
          <ion-toolbar>
            <ion-searchbar
              placeholder="Search products..."
              [debounce]="400"
              (ionInput)="onSearch($event)"
            ></ion-searchbar>
          </ion-toolbar>
        </ion-header>

        <ion-content>
          @if (isSearching()) {
            <div class="center-spinner">
              <ion-spinner name="crescent"></ion-spinner>
            </div>
          }

          @if (results().length > 0) {
            <ion-list>
              @for (product of results(); track product.id) {
                <ion-item button (click)="select(product)">
                  @if (product.imageUrls.length > 0) {
                    <img [src]="product.imageUrls[0]" alt="" class="product-thumb" slot="start" loading="lazy" />
                  }
                  <ion-label>
                    <h3>{{ product.name }}</h3>
                    <p>{{ product.retailerName }} &middot; {{ product.currency }}{{ product.price | number:'1.2-2' }}</p>
                  </ion-label>
                  @if (isTagged(product.id)) {
                    <ion-badge color="success" slot="end">Added</ion-badge>
                  } @else {
                    <ion-icon name="add-outline" slot="end" color="primary"></ion-icon>
                  }
                </ion-item>
              }
            </ion-list>
          }

          @if (!isSearching() && results().length === 0 && hasSearched()) {
            <ion-note class="ion-padding ion-text-center" style="display:block;margin-top:24px">
              No products found. Try a different search.
            </ion-note>
          }
        </ion-content>
      </ng-template>
    </ion-modal>
  `,
  styles: [`
    .product-thumb { width: 44px; height: 44px; border-radius: 6px; object-fit: cover; }
    .center-spinner { display: flex; justify-content: center; padding: 24px; }
  `],
})
export class ProductTagSheetComponent {
  private readonly commerce = inject(CommerceService);

  readonly isOpen = input(false);
  readonly taggedProductIds = input<string[]>([]);

  readonly productSelected = output<ProductResponse>();
  readonly dismissed = output<void>();

  readonly results = signal<ProductResponse[]>([]);
  readonly isSearching = signal(false);
  readonly hasSearched = signal(false);

  constructor() {
    addIcons({ closeOutline, addOutline, checkmarkOutline });
  }

  onSearch(event: Event): void {
    const query = (event as CustomEvent).detail.value?.trim();
    if (!query) {
      this.results.set([]);
      return;
    }
    this.isSearching.set(true);
    this.hasSearched.set(true);
    this.commerce.searchProducts(query).subscribe({
      next: (products) => this.results.set(products),
      complete: () => this.isSearching.set(false),
    });
  }

  isTagged(productId: string): boolean {
    return this.taggedProductIds().includes(productId);
  }

  select(product: ProductResponse): void {
    if (!this.isTagged(product.id)) {
      this.productSelected.emit(product);
    }
  }

  close(): void {
    this.dismissed.emit();
  }
}
