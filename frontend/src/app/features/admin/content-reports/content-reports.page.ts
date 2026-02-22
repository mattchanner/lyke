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
  IonChip,
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
  flagOutline,
  eyeOutline,
} from 'ionicons/icons';

import { AdminService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import {
  ContentReportResponse,
  ReportStatus,
  ReportReason,
  PostStatus,
} from '../../../models';
import { SkeletonListComponent } from '../../../shared/components/skeleton-list';

type StatusTab = ReportStatus | 'all';

@Component({
  selector: 'app-content-reports',
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
    IonChip,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonRefresher,
    IonRefresherContent,
    IonNote,
    IonCard,
    IonCardContent,
    SkeletonListComponent,
  ],
  templateUrl: './content-reports.page.html',
  styleUrls: ['./content-reports.page.scss'],
})
export class ContentReportsPage implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly alertController = inject(AlertController);

  readonly reports = signal<ContentReportResponse[]>([]);
  readonly isLoading = signal(false);
  readonly isActioning = signal(false);
  readonly activeStatusTab = signal<StatusTab>('all');
  readonly activeReasonFilter = signal<ReportReason | undefined>(undefined);
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);
  readonly expandedId = signal<string | null>(null);

  readonly ReportStatus = ReportStatus;
  readonly ReportReason = ReportReason;

  readonly reasonOptions: { value: ReportReason; label: string }[] = [
    { value: ReportReason.InappropriateContent, label: 'Inappropriate' },
    { value: ReportReason.Spam, label: 'Spam' },
    { value: ReportReason.MisleadingProductTag, label: 'Misleading Tag' },
    { value: ReportReason.Copyright, label: 'Copyright' },
    { value: ReportReason.HateSpeech, label: 'Hate Speech' },
    { value: ReportReason.Other, label: 'Other' },
  ];

  constructor() {
    addIcons({
      checkmarkOutline,
      closeOutline,
      chevronDownOutline,
      chevronUpOutline,
      flagOutline,
      eyeOutline,
    });
  }

  ngOnInit(): void {
    this.loadReports(true);
  }

  loadReports(refresh = false, event?: CustomEvent): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }
    this.isLoading.set(true);

    const tab = this.activeStatusTab();
    const status = tab === 'all' ? undefined : (tab as ReportStatus);

    this.adminService
      .getContentReports({
        status,
        reason: this.activeReasonFilter(),
        page: this.currentPage(),
        pageSize: 20,
      })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (refresh) {
              this.reports.set(r.data);
            } else {
              this.reports.update((rpts) => [...rpts, ...r.data!]);
            }
            this.hasMore.set(r.meta?.hasNextPage ?? false);
          }
        },
        error: () => {
          this.toast.error('Failed to load reports');
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
    this.activeStatusTab.set(event.detail.value as StatusTab);
    this.expandedId.set(null);
    this.loadReports(true);
  }

  onReasonFilterChange(reason: ReportReason | undefined): void {
    this.activeReasonFilter.set(
      this.activeReasonFilter() === reason ? undefined : reason
    );
    this.expandedId.set(null);
    this.loadReports(true);
  }

  onRefresh(event: CustomEvent): void {
    this.loadReports(true, event);
  }

  loadMore(event: CustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadReports(false, event);
  }

  toggleExpand(id: string): void {
    this.expandedId.update((current) => (current === id ? null : id));
  }

  async onDismiss(report: ContentReportResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Dismiss Report',
      message: `Dismiss report for "${report.postTitle || 'Untitled'}"?`,
      inputs: [
        {
          name: 'notes',
          type: 'textarea',
          placeholder: 'Review notes (optional)...',
        },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Dismiss',
          handler: (data) => {
            this.reviewReport(report.id, ReportStatus.Dismissed, data.notes?.trim());
          },
        },
      ],
    });
    await alert.present();
  }

  async onTakeAction(report: ContentReportResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Take Action',
      message: `Take action on report for "${report.postTitle || 'Untitled'}"`,
      inputs: [
        {
          name: 'notes',
          type: 'textarea',
          placeholder: 'Review notes (optional)...',
        },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'No Post Change',
          handler: (data) => {
            this.reviewReport(report.id, ReportStatus.ActionTaken, data.notes?.trim());
          },
        },
        {
          text: 'Reject Post',
          role: 'destructive',
          handler: (data) => {
            this.reviewReport(
              report.id,
              ReportStatus.ActionTaken,
              data.notes?.trim(),
              PostStatus.Rejected
            );
          },
        },
        {
          text: 'Remove Post',
          role: 'destructive',
          handler: (data) => {
            this.reviewReport(
              report.id,
              ReportStatus.ActionTaken,
              data.notes?.trim(),
              PostStatus.Removed
            );
          },
        },
      ],
    });
    await alert.present();
  }

  private reviewReport(
    reportId: string,
    newStatus: ReportStatus,
    reviewNotes?: string,
    postAction?: PostStatus
  ): void {
    this.isActioning.set(true);
    this.adminService
      .reviewContentReport(reportId, { newStatus, reviewNotes, postAction })
      .subscribe({
        next: (r) => {
          if (r.success) {
            this.toast.success('Report reviewed');
            this.reports.update((rpts) => rpts.filter((rpt) => rpt.id !== reportId));
            this.expandedId.set(null);
          } else {
            this.toast.error(r.error?.message || 'Failed to review report');
          }
        },
        error: () => this.toast.error('Failed to review report'),
        complete: () => this.isActioning.set(false),
      });
  }

  getStatusColor(status: ReportStatus): string {
    switch (status) {
      case ReportStatus.Pending:
        return 'warning';
      case ReportStatus.UnderReview:
        return 'primary';
      case ReportStatus.Dismissed:
        return 'medium';
      case ReportStatus.ActionTaken:
        return 'success';
      default:
        return 'medium';
    }
  }

  getReasonColor(reason: ReportReason): string {
    switch (reason) {
      case ReportReason.HateSpeech:
        return 'danger';
      case ReportReason.InappropriateContent:
        return 'warning';
      case ReportReason.Spam:
        return 'tertiary';
      case ReportReason.Copyright:
        return 'primary';
      case ReportReason.MisleadingProductTag:
        return 'secondary';
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
}
