import { Component, signal, inject } from '@angular/core';
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
  IonButton,
  IonSpinner,
  IonText,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { storefrontOutline } from 'ionicons/icons';
import { RetailerService, ToastService } from '../../../core';
import { RegisterRetailerRequest } from '../../../models';

@Component({
  selector: 'app-retailer-register',
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
    IonButton,
    IonSpinner,
    IonText,
  ],
  templateUrl: './register.page.html',
  styleUrls: ['./register.page.scss'],
})
export class RetailerRegisterPage {
  private readonly retailerService = inject(RetailerService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly name = signal('');
  readonly logoUrl = signal('');
  readonly websiteUrl = signal('');
  readonly contactEmail = signal('');
  readonly isSaving = signal(false);

  constructor() {
    addIcons({ storefrontOutline });
  }

  submit(): void {
    if (!this.name().trim()) {
      this.toast.error('Business name is required');
      return;
    }

    this.isSaving.set(true);
    const request: RegisterRetailerRequest = {
      name: this.name().trim(),
    };
    if (this.logoUrl().trim()) request.logoUrl = this.logoUrl().trim();
    if (this.websiteUrl().trim()) request.websiteUrl = this.websiteUrl().trim();
    if (this.contactEmail().trim())
      request.contactEmail = this.contactEmail().trim();

    this.retailerService.register(request).subscribe({
      next: (r) => {
        if (r.success) {
          this.toast.success('Retailer account created!');
          this.router.navigate(['/retailer']);
        } else {
          this.toast.error(r.error?.message ?? 'Registration failed');
        }
      },
      error: () => this.toast.error('Registration failed'),
      complete: () => this.isSaving.set(false),
    });
  }
}
