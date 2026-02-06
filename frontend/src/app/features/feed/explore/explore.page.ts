import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
} from '@ionic/angular/standalone';
import { ApiService } from '../../../core';
import { SearchResponse, SearchType } from '../../../models';
import { PostCardComponent } from '../../../shared/components/post-card/post-card.component';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
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
    PostCardComponent,
  ],
  templateUrl: './explore.page.html',
  styleUrls: ['./explore.page.scss'],
})
export class ExplorePage {
  private readonly api = inject(ApiService);

  searchQuery = '';
  readonly searchType = signal<SearchType>(SearchType.All);
  readonly isLoading = signal(false);
  readonly results = signal<SearchResponse | null>(null);

  onTypeChange(event: CustomEvent): void {
    this.searchType.set(parseInt(event.detail.value));
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
        query: this.searchQuery,
        type: this.searchType(),
        page: 1,
        pageSize: 20,
      })
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.results.set(response.data);
          }
        },
        complete: () => this.isLoading.set(false),
      });
  }
}
