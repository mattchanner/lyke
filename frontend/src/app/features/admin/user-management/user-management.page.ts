import { Component, OnInit, inject, signal, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSearchbar,
  IonChip,
  IonLabel,
  IonList,
  IonItem,
  IonAvatar,
  IonBadge,
  IonIcon,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonRefresher,
  IonRefresherContent,
  IonNote,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  personOutline,
  alertCircleOutline,
  chevronForwardOutline,
} from 'ionicons/icons';

import { AdminService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import { UserListResponse, UserType } from '../../../models';
import { SkeletonListComponent } from '../../../shared/components/skeleton-list';

type FilterType = UserType | 'all' | 'suspended';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSearchbar,
    IonChip,
    IonLabel,
    IonList,
    IonItem,
    IonAvatar,
    IonBadge,
    IonIcon,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonRefresher,
    IonRefresherContent,
    IonNote,
    SkeletonListComponent,
  ],
  templateUrl: './user-management.page.html',
  styleUrls: ['./user-management.page.scss'],
})
export class UserManagementPage implements OnInit, OnDestroy {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroy$ = new Subject<void>();
  private readonly searchSubject = new Subject<string>();

  readonly users = signal<UserListResponse[]>([]);
  readonly isLoading = signal(false);
  readonly searchQuery = signal('');
  readonly filterType = signal<FilterType>('all');
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);

  readonly UserType = UserType;
  readonly filterOptions: { value: FilterType; label: string }[] = [
    { value: 'all', label: 'All' },
    { value: UserType.Shopper, label: 'Shoppers' },
    { value: UserType.Creator, label: 'Creators' },
    { value: UserType.Retailer, label: 'Retailers' },
    { value: UserType.Admin, label: 'Admins' },
    { value: 'suspended', label: 'Suspended' },
  ];

  constructor() {
    addIcons({
      personOutline,
      alertCircleOutline,
      chevronForwardOutline,
    });
  }

  ngOnInit(): void {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((query) => {
        this.searchQuery.set(query);
        this.loadUsers(true);
      });

    this.loadUsers(true);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(refresh = false, event?: CustomEvent): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }
    this.isLoading.set(true);

    const filter = this.filterType();
    const userType = filter !== 'all' && filter !== 'suspended' ? filter : undefined;
    const isActive = filter === 'suspended' ? false : undefined;

    this.adminService
      .getUsers({
        userType,
        isActive,
        search: this.searchQuery() || undefined,
        page: this.currentPage(),
        pageSize: 20,
      })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (refresh) {
              this.users.set(r.data);
            } else {
              this.users.update((u) => [...u, ...r.data!]);
            }
            this.hasMore.set(r.meta?.hasNextPage ?? false);
          }
        },
        error: () => {
          this.toast.error('Failed to load users');
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
        complete: () => {
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
      });
  }

  onSearchInput(event: CustomEvent): void {
    this.searchSubject.next(event.detail.value || '');
  }

  onFilterChange(filter: FilterType): void {
    this.filterType.set(filter);
    this.loadUsers(true);
  }

  onRefresh(event: CustomEvent): void {
    this.loadUsers(true, event);
  }

  loadMore(event: CustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadUsers(false, event);
  }

  navigateToUser(user: UserListResponse): void {
    this.router.navigate(['/admin/users', user.id]);
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

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}
