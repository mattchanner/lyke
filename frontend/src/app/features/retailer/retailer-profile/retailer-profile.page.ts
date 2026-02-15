import { Component, OnInit, signal, inject } from '@angular/core';
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
  IonIcon,
  IonSkeletonText,
  IonText,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { saveOutline } from 'ionicons/icons';
import { RetailerService, ToastService } from '../../../core';
import {
  RetailerProfileResponse,
  UpdateRetailerProfileRequest,
} from '../../../models';

@Component({
  selector: 'app-retailer-profile',
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
    IonIcon,
    IonSkeletonText,
    IonText,
  ],
  templateUrl: './retailer-profile.page.html',
  styleUrls: ['./retailer-profile.page.scss'],
})
export class RetailerProfilePage implements OnInit {
  private readonly retailerService = inject(RetailerService);
  private readonly toast = inject(ToastService);

  readonly profile = signal<RetailerProfileResponse | null>(null);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);

  readonly name = signal('');
  readonly logoUrl = signal('');
  readonly websiteUrl = signal('');
  readonly contactEmail = signal('');
  readonly affiliateBaseUrl = signal('');
  readonly affiliateId = signal('');
  readonly commissionRate = signal('');

  constructor() {
    addIcons({ saveOutline });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.retailerService.getProfile().subscribe((profile) => {
      this.profile.set(profile);
      if (profile) {
        this.name.set(profile.name);
        this.logoUrl.set(profile.logoUrl ?? '');
        this.websiteUrl.set(profile.websiteUrl ?? '');
        this.contactEmail.set(profile.contactEmail ?? '');
        this.affiliateBaseUrl.set(profile.affiliateConfig?.baseUrl ?? '');
        this.affiliateId.set(profile.affiliateConfig?.affiliateId ?? '');
        this.commissionRate.set(
          profile.affiliateConfig?.commissionRate?.toString() ?? ''
        );
      }
      this.isLoading.set(false);
    });
  }

  save(): void {
    this.isSaving.set(true);
    const request: UpdateRetailerProfileRequest = {
      name: this.name().trim() || undefined,
      logoUrl: this.logoUrl().trim() || undefined,
      websiteUrl: this.websiteUrl().trim() || undefined,
      contactEmail: this.contactEmail().trim() || undefined,
    };

    if (
      this.affiliateBaseUrl().trim() ||
      this.affiliateId().trim() ||
      this.commissionRate().trim()
    ) {
      request.affiliateConfig = {
        baseUrl: this.affiliateBaseUrl().trim() || undefined,
        affiliateId: this.affiliateId().trim() || undefined,
        commissionRate: this.commissionRate().trim()
          ? parseFloat(this.commissionRate())
          : undefined,
      };
    }

    this.retailerService.updateProfile(request).subscribe({
      next: (r) => {
        if (r.success) {
          this.toast.success('Profile updated');
          this.profile.set(r.data ?? null);
        } else {
          this.toast.error(r.error?.message ?? 'Update failed');
        }
      },
      error: () => this.toast.error('Update failed'),
      complete: () => this.isSaving.set(false),
    });
  }
}
