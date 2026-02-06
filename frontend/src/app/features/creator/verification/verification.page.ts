import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';
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
  IonBadge,
  IonButton,
  IonItem,
  IonInput,
  IonTextarea,
  IonIcon,
  IonList,
  IonSkeletonText,
  IonSpinner,
} from '@ionic/angular/standalone';
import { FormsModule } from '@angular/forms';
import { addIcons } from 'ionicons';
import {
  shieldCheckmarkOutline,
  timeOutline,
  closeCircleOutline,
  addOutline,
  trashOutline,
  documentTextOutline,
} from 'ionicons/icons';
import { CreatorService, ToastService } from '../../../core';
import { VerificationStatusResponse, VerificationStatus } from '../../../models';

@Component({
  selector: 'app-verification',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
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
    IonBadge,
    IonButton,
    IonItem,
    IonInput,
    IonTextarea,
    IonIcon,
    IonList,
    IonSkeletonText,
    IonSpinner,
  ],
  templateUrl: './verification.page.html',
  styleUrls: ['./verification.page.scss'],
})
export class VerificationPage implements OnInit {
  private readonly creatorService = inject(CreatorService);
  private readonly toast = inject(ToastService);

  readonly VerificationStatus = VerificationStatus;

  readonly verification = signal<VerificationStatusResponse | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly documentUrls = signal<string[]>(['']);
  readonly notes = signal('');

  constructor() {
    addIcons({
      shieldCheckmarkOutline,
      timeOutline,
      closeCircleOutline,
      addOutline,
      trashOutline,
      documentTextOutline,
    });
  }

  ngOnInit(): void {
    this.loadStatus();
  }

  loadStatus(): void {
    this.isLoading.set(true);
    this.creatorService.getVerificationStatus().subscribe((v) => {
      this.verification.set(v);
      this.isLoading.set(false);
    });
  }

  addDocumentUrl(): void {
    this.documentUrls.update((urls) => [...urls, '']);
  }

  removeDocumentUrl(index: number): void {
    this.documentUrls.update((urls) => urls.filter((_, i) => i !== index));
  }

  updateDocumentUrl(index: number, value: string): void {
    this.documentUrls.update((urls) =>
      urls.map((url, i) => (i === index ? value : url))
    );
  }

  submitVerification(): void {
    const urls = this.documentUrls().filter((u) => u.trim().length > 0);
    if (urls.length === 0) {
      this.toast.warning('Please add at least one document URL');
      return;
    }

    this.isSubmitting.set(true);
    this.creatorService
      .submitVerification({
        documentUrls: urls,
        notes: this.notes() || undefined,
      })
      .subscribe({
        next: (r) => {
          if (r.success) {
            this.toast.success('Verification submitted successfully');
            this.loadStatus();
          } else {
            this.toast.error(r.error?.message || 'Submission failed');
          }
        },
        error: () => this.toast.error('Failed to submit verification'),
        complete: () => this.isSubmitting.set(false),
      });
  }

  getStatusColor(status: VerificationStatus): string {
    switch (status) {
      case VerificationStatus.NotSubmitted: return 'medium';
      case VerificationStatus.Pending: return 'warning';
      case VerificationStatus.Approved: return 'success';
      case VerificationStatus.Rejected: return 'danger';
      default: return 'medium';
    }
  }

  getStatusLabel(status: VerificationStatus): string {
    switch (status) {
      case VerificationStatus.NotSubmitted: return 'Not Submitted';
      case VerificationStatus.Pending: return 'Under Review';
      case VerificationStatus.Approved: return 'Verified';
      case VerificationStatus.Rejected: return 'Rejected';
      default: return 'Unknown';
    }
  }
}
