import { Component, OnInit, inject, signal } from '@angular/core';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonList,
  IonItem,
  IonBadge,
  IonButton,
  IonIcon,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonRefresher,
  IonRefresherContent,
  IonNote,
  IonCard,
  IonCardContent,
  AlertController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  checkmarkOutline,
  closeOutline,
  chevronDownOutline,
  chevronUpOutline,
  personOutline,
  linkOutline,
  documentOutline,
  imagesOutline,
} from 'ionicons/icons';

import { AdminService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import { PendingVerificationResponse, VerificationStatus } from '../../../models';
import { SkeletonListComponent } from '../../../shared/components/skeleton-list';

@Component({
  selector: 'app-verification-review',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonList,
    IonItem,
    IonBadge,
    IonButton,
    IonIcon,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonRefresher,
    IonRefresherContent,
    IonNote,
    IonCard,
    IonCardContent,
    SkeletonListComponent,
  ],
  templateUrl: './verification-review.page.html',
  styleUrls: ['./verification-review.page.scss'],
})
export class VerificationReviewPage implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly alertController = inject(AlertController);

  readonly verifications = signal<PendingVerificationResponse[]>([]);
  readonly isLoading = signal(false);
  readonly isActioning = signal(false);
  readonly activeTab = signal<VerificationStatus>(VerificationStatus.Pending);
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);
  readonly expandedId = signal<string | null>(null);

  readonly VerificationStatus = VerificationStatus;

  constructor() {
    addIcons({
      checkmarkOutline,
      closeOutline,
      chevronDownOutline,
      chevronUpOutline,
      personOutline,
      linkOutline,
      documentOutline,
      imagesOutline,
    });
  }

  ngOnInit(): void {
    this.loadVerifications(true);
  }

  loadVerifications(refresh = false, event?: CustomEvent): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }
    this.isLoading.set(true);

    this.adminService
      .getPendingVerifications({
        status: this.activeTab(),
        page: this.currentPage(),
        pageSize: 20,
      })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (refresh) {
              this.verifications.set(r.data);
            } else {
              this.verifications.update((v) => [...v, ...r.data!]);
            }
            this.hasMore.set(r.meta?.hasNextPage ?? false);
          }
        },
        error: () => {
          this.toast.error('Failed to load verifications');
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
        complete: () => {
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
      });
  }

  onTabChange(event: CustomEvent): void {
    this.activeTab.set(event.detail.value as VerificationStatus);
    this.expandedId.set(null);
    this.loadVerifications(true);
  }

  onRefresh(event: CustomEvent): void {
    this.loadVerifications(true, event);
  }

  loadMore(event: CustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadVerifications(false, event);
  }

  toggleExpand(creatorId: string): void {
    this.expandedId.update((id) => (id === creatorId ? null : creatorId));
  }

  async onApprove(verification: PendingVerificationResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Approve Verification',
      message: `Approve ${verification.displayName}'s verification request?`,
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Approve',
          handler: () => {
            this.reviewVerification(verification.creatorId, true);
          },
        },
      ],
    });
    await alert.present();
  }

  async onReject(verification: PendingVerificationResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Reject Verification',
      message: `Reject ${verification.displayName}'s verification request?`,
      inputs: [
        {
          name: 'reason',
          type: 'textarea',
          placeholder: 'Rejection reason (required)...',
        },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Reject',
          role: 'destructive',
          handler: (data) => {
            if (!data.reason?.trim()) {
              this.toast.error('Rejection reason is required');
              return false;
            }
            this.reviewVerification(verification.creatorId, false, data.reason.trim());
            return true;
          },
        },
      ],
    });
    await alert.present();
  }

  private reviewVerification(
    creatorId: string,
    approve: boolean,
    rejectionReason?: string
  ): void {
    this.isActioning.set(true);
    this.adminService
      .reviewVerification(creatorId, { approve, rejectionReason })
      .subscribe({
        next: (r) => {
          if (r.success) {
            this.toast.success(approve ? 'Verification approved' : 'Verification rejected');
            this.verifications.update((v) => v.filter((item) => item.creatorId !== creatorId));
            this.expandedId.set(null);
          } else {
            this.toast.error(r.error?.message || 'Failed to review verification');
          }
        },
        error: () => this.toast.error('Failed to review verification'),
        complete: () => this.isActioning.set(false),
      });
  }

  getStatusColor(status: VerificationStatus): string {
    switch (status) {
      case VerificationStatus.Approved:
        return 'success';
      case VerificationStatus.Pending:
        return 'warning';
      case VerificationStatus.Rejected:
        return 'danger';
      case VerificationStatus.NotSubmitted:
        return 'medium';
      default:
        return 'medium';
    }
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }

  getSocialLinkKeys(links: Record<string, string> | null): string[] {
    return links ? Object.keys(links) : [];
  }
}
