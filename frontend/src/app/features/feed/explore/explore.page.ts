import { Component, inject, signal, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonSearchbar,
  IonBackButton,
  IonButtons,
  IonSpinner,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonItem,
  IonAvatar,
  IonThumbnail,
  IonIcon,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { checkmarkCircle } from 'ionicons/icons';
import { ApiService, AnalyticsService } from '../../../core';
import { CreatorSearchResult, SearchResponse, SearchType } from '../../../models';
import { PostCardComponent } from '../../../shared/components/post-card/post-card.component';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [
    DecimalPipe,
    FormsModule,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonSearchbar,
    IonBackButton,
    IonButtons,
    IonSpinner,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonItem,
    IonAvatar,
    IonThumbnail,
    IonIcon,
    PostCardComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './explore.page.html',
  styleUrls: ['./explore.page.scss'],
})
export class ExplorePage {
  private readonly api = inject(ApiService);
  private readonly analytics = inject(AnalyticsService);
  private readonly router = inject(Router);

  constructor() {
    addIcons({ checkmarkCircle });
  }

  searchQuery = '';
  readonly searchType = signal<SearchType>(SearchType.All);
  readonly isLoading = signal(false);
  readonly results = signal<SearchResponse | null>(null);

  onTypeChange(event: CustomEvent): void {
    this.searchType.set(event.detail.value);
    this.onSearch();
  }

  onSearch(): void {
    if (!this.searchQuery.trim()) {
      this.results.set(null);
      return;
    }

    this.isLoading.set(true);

    this.api
      .get<SearchResponse>('feed', 'search', {
        q: this.searchQuery,
        type: this.searchType(),
        page: 1,
        pageSize: 20,
      })
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.results.set(response.data);
            this.analytics.track('search.execute', {
              query: this.searchQuery,
              type: this.searchType(),
              resultCount: String(response.data.totalResults),
            });
          }
        },
        complete: () => this.isLoading.set(false),
      });
  }

  onCreatorTap(creator: CreatorSearchResult): void {
    this.router.navigate(['/feed'], {
      queryParams: { creatorId: creator.id, creatorName: creator.displayName },
    });
  }
}
