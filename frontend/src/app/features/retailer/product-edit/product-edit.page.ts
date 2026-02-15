import { Component, OnInit, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButtons,
  IonBackButton,
  IonList,
  IonItem,
  IonInput,
  IonTextarea,
  IonToggle,
  IonButton,
  IonSpinner,
  IonIcon,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { saveOutline } from 'ionicons/icons';
import { RetailerService, ToastService } from '../../../core';
import {
  RetailerProductResponse,
  UpdateRetailerProductRequest,
} from '../../../models';

@Component({
  selector: 'app-product-edit',
  standalone: true,
  imports: [
    FormsModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonList,
    IonItem,
    IonInput,
    IonTextarea,
    IonToggle,
    IonButton,
    IonSpinner,
    IonIcon,
  ],
  templateUrl: './product-edit.page.html',
  styleUrls: ['./product-edit.page.scss'],
})
export class ProductEditPage implements OnInit {
  private readonly retailerService = inject(RetailerService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly product = signal<RetailerProductResponse | null>(null);
  readonly isSaving = signal(false);

  readonly name = signal('');
  readonly description = signal('');
  readonly category = signal('');
  readonly subCategory = signal('');
  readonly productUrl = signal('');
  readonly price = signal('');
  readonly currency = signal('');
  readonly isActive = signal(true);

  constructor() {
    addIcons({ saveOutline });
  }

  ngOnInit(): void {
    const state = this.router.getCurrentNavigation()?.extras.state;
    const p = (state as { product?: RetailerProductResponse })?.product ??
      history.state?.product;
    if (p) {
      this.product.set(p);
      this.name.set(p.name);
      this.description.set(p.description ?? '');
      this.category.set(p.category);
      this.subCategory.set(p.subCategory ?? '');
      this.productUrl.set(p.productUrl);
      this.price.set(String(p.price));
      this.currency.set(p.currency);
      this.isActive.set(p.isActive);
    } else {
      this.toast.error('No product data available');
      this.router.navigate(['/retailer/products']);
    }
  }

  save(): void {
    const p = this.product();
    if (!p) return;

    this.isSaving.set(true);
    const request: UpdateRetailerProductRequest = {
      name: this.name().trim() || undefined,
      description: this.description().trim() || undefined,
      category: this.category().trim() || undefined,
      subCategory: this.subCategory().trim() || undefined,
      productUrl: this.productUrl().trim() || undefined,
      price: this.price().trim() ? parseFloat(this.price()) : undefined,
      currency: this.currency().trim() || undefined,
      isActive: this.isActive(),
    };

    this.retailerService.updateProduct(p.id, request).subscribe({
      next: (r) => {
        if (r.success) {
          this.toast.success('Product updated');
          this.router.navigate(['/retailer/products']);
        } else {
          this.toast.error(r.error?.message ?? 'Update failed');
        }
      },
      error: () => this.toast.error('Update failed'),
      complete: () => this.isSaving.set(false),
    });
  }
}
