import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonList,
  IonItem,
  IonLabel,
  IonBadge,
  IonButton,
  IonIcon,
  IonSpinner,
  AlertController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  personOutline,
  banOutline,
  checkmarkCircleOutline,
  alertCircleOutline,
  shieldCheckmarkOutline,
  bodyOutline,
} from 'ionicons/icons';

import { AdminService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import { UserDetailResponse, UserType, VerificationStatus } from '../../../models';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonList,
    IonItem,
    IonLabel,
    IonBadge,
    IonButton,
    IonIcon,
    IonSpinner,
  ],
  templateUrl: './user-detail.page.html',
  styleUrls: ['./user-detail.page.scss'],
})
export class UserDetailPage implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly alertController = inject(AlertController);
  private readonly route = inject(ActivatedRoute);

  readonly user = signal<UserDetailResponse | null>(null);
  readonly isLoading = signal(false);
  readonly isActioning = signal(false);

  readonly UserType = UserType;
  readonly VerificationStatus = VerificationStatus;

  constructor() {
    addIcons({
      personOutline,
      banOutline,
      checkmarkCircleOutline,
      alertCircleOutline,
      shieldCheckmarkOutline,
      bodyOutline,
    });
  }

  ngOnInit(): void {
    this.loadUser();
  }

  loadUser(): void {
    const userId = this.route.snapshot.paramMap.get('id');
    if (!userId) {
      this.toast.error('Invalid user ID');
      return;
    }

    this.isLoading.set(true);
    this.adminService.getUserDetail(userId).subscribe({
      next: (user) => {
        this.user.set(user);
        if (!user) {
          this.toast.error('User not found');
        }
      },
      error: () => this.toast.error('Failed to load user'),
      complete: () => this.isLoading.set(false),
    });
  }

  async onSuspend(): Promise<void> {
    const user = this.user();
    if (!user) return;

    const alert = await this.alertController.create({
      header: 'Suspend User',
      message: `Suspend ${user.email || user.userName || 'this user'}?`,
      inputs: [
        {
          name: 'reason',
          type: 'textarea',
          placeholder: 'Suspension reason (required)...',
        },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Suspend',
          role: 'destructive',
          handler: (data) => {
            if (!data.reason?.trim()) {
              this.toast.error('Suspension reason is required');
              return false;
            }
            this.suspendUser(user.id, data.reason.trim());
            return true;
          },
        },
      ],
    });
    await alert.present();
  }

  async onUnsuspend(): Promise<void> {
    const user = this.user();
    if (!user) return;

    const alert = await this.alertController.create({
      header: 'Unsuspend User',
      message: `Unsuspend ${user.email || user.userName || 'this user'}? They will regain access to their account.`,
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Unsuspend',
          handler: () => {
            this.unsuspendUser(user.id);
          },
        },
      ],
    });
    await alert.present();
  }

  private suspendUser(userId: string, reason: string): void {
    this.isActioning.set(true);
    this.adminService.suspendUser(userId, { reason }).subscribe({
      next: (r) => {
        if (r.success) {
          this.toast.success('User suspended');
          this.loadUser();
        } else {
          this.toast.error(r.error?.message || 'Failed to suspend user');
        }
      },
      error: () => this.toast.error('Failed to suspend user'),
      complete: () => this.isActioning.set(false),
    });
  }

  private unsuspendUser(userId: string): void {
    this.isActioning.set(true);
    this.adminService.unsuspendUser(userId).subscribe({
      next: (r) => {
        if (r.success) {
          this.toast.success('User unsuspended');
          this.loadUser();
        } else {
          this.toast.error(r.error?.message || 'Failed to unsuspend user');
        }
      },
      error: () => this.toast.error('Failed to unsuspend user'),
      complete: () => this.isActioning.set(false),
    });
  }

  getUserTypeColor(userType: UserType): string {
    switch (userType) {
      case UserType.Admin:
        return 'danger';
      case UserType.Creator:
        return 'primary';
      case UserType.Retailer:
        return 'tertiary';
      case UserType.Shopper:
        return 'medium';
      default:
        return 'medium';
    }
  }

  getVerificationColor(status: VerificationStatus): string {
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
      hour: '2-digit',
      minute: '2-digit',
    });
  }
}
