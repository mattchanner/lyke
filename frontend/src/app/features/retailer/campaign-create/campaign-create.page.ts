import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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
  IonDatetime,
  IonButton,
  IonSpinner,
  IonIcon,
  IonLabel,
  IonChip,
  IonSkeletonText,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { saveOutline, closeCircleOutline } from 'ionicons/icons';
import { RetailerService, ToastService } from '../../../core';
import {
  CampaignResponse,
  CreateCampaignRequest,
  UpdateCampaignRequest,
} from '../../../models';

const BODY_TYPES = [
  'Hourglass',
  'Pear',
  'Apple',
  'Rectangle',
  'Inverted Triangle',
  'Athletic',
];

@Component({
  selector: 'app-campaign-create',
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
    IonDatetime,
    IonButton,
    IonSpinner,
    IonIcon,
    IonLabel,
    IonChip,
    IonSkeletonText,
  ],
  templateUrl: './campaign-create.page.html',
  styleUrls: ['./campaign-create.page.scss'],
})
export class CampaignCreatePage implements OnInit {
  private readonly retailerService = inject(RetailerService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly campaignId = signal<string | null>(null);
  readonly isEditMode = computed(() => this.campaignId() !== null);
  readonly existingCampaign = signal<CampaignResponse | null>(null);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);

  readonly budgetAmount = signal('');
  readonly startDate = signal('');
  readonly endDate = signal('');
  readonly selectedBodyTypes = signal<string[]>([]);
  readonly bodyTypeOptions = BODY_TYPES;

  constructor() {
    addIcons({ saveOutline, closeCircleOutline });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.campaignId.set(id);
      this.loadCampaign(id);
    }
  }

  private loadCampaign(id: string): void {
    this.isLoading.set(true);
    this.retailerService.getCampaigns({ page: 1, pageSize: 100 }).subscribe({
      next: (r) => {
        if (r.success && r.data) {
          const campaign = r.data.find((c) => c.id === id);
          if (campaign) {
            this.existingCampaign.set(campaign);
            this.budgetAmount.set(String(campaign.budgetAmount));
            this.startDate.set(campaign.startDate);
            this.endDate.set(campaign.endDate);
            this.selectedBodyTypes.set(campaign.targetBodyTypes ?? []);
          }
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  toggleBodyType(type: string): void {
    this.selectedBodyTypes.update((types) =>
      types.includes(type)
        ? types.filter((t) => t !== type)
        : [...types, type]
    );
  }

  submit(): void {
    if (!this.budgetAmount().trim() || !this.startDate() || !this.endDate()) {
      this.toast.error('Budget, start date, and end date are required');
      return;
    }

    this.isSaving.set(true);

    if (this.isEditMode()) {
      const request: UpdateCampaignRequest = {
        budgetAmount: parseFloat(this.budgetAmount()),
        startDate: this.startDate(),
        endDate: this.endDate(),
        targetBodyTypes: this.selectedBodyTypes().length
          ? this.selectedBodyTypes()
          : undefined,
      };
      this.retailerService
        .updateCampaign(this.campaignId()!, request)
        .subscribe({
          next: (r) => {
            if (r.success) {
              this.toast.success('Campaign updated');
              this.router.navigate(['/retailer/campaigns']);
            } else {
              this.toast.error(r.error?.message ?? 'Update failed');
            }
          },
          error: () => this.toast.error('Update failed'),
          complete: () => this.isSaving.set(false),
        });
    } else {
      const request: CreateCampaignRequest = {
        budgetAmount: parseFloat(this.budgetAmount()),
        startDate: this.startDate(),
        endDate: this.endDate(),
        targetBodyTypes: this.selectedBodyTypes().length
          ? this.selectedBodyTypes()
          : undefined,
      };
      this.retailerService.createCampaign(request).subscribe({
        next: (r) => {
          if (r.success) {
            this.toast.success('Campaign created');
            this.router.navigate(['/retailer/campaigns']);
          } else {
            this.toast.error(r.error?.message ?? 'Creation failed');
          }
        },
        error: () => this.toast.error('Creation failed'),
        complete: () => this.isSaving.set(false),
      });
    }
  }
}
