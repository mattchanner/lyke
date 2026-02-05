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
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-back-button defaultHref="/feed"></ion-back-button>
        </ion-buttons>
        <ion-title>Explore</ion-title>
      </ion-toolbar>
      <ion-toolbar>
        <ion-searchbar
          [(ngModel)]="searchQuery"
          (ionInput)="onSearch()"
          placeholder="Search outfits, products, creators..."
          debounce="300"
        ></ion-searchbar>
      </ion-toolbar>
      <ion-toolbar>
        <ion-segment [value]="searchType()" (ionChange)="onTypeChange($event)">
          <ion-segment-button value="0">
            <ion-label>All</ion-label>
          </ion-segment-button>
          <ion-segment-button value="1">
            <ion-label>Posts</ion-label>
          </ion-segment-button>
          <ion-segment-button value="2">
            <ion-label>Products</ion-label>
          </ion-segment-button>
          <ion-segment-button value="3">
            <ion-label>Creators</ion-label>
          </ion-segment-button>
        </ion-segment>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      @if (isLoading()) {
        <div class="loading-container">
          <ion-spinner name="crescent"></ion-spinner>
        </div>
      } @else if (!searchQuery) {
        <div class="empty-state">
          <p>Start typing to search...</p>
        </div>
      } @else if (results()?.posts?.length === 0 && results()?.products?.length === 0 && results()?.creators?.length === 0) {
        <div class="empty-state">
          <p>No results found for "{{ searchQuery }}"</p>
        </div>
      } @else {
        @if (results()?.posts?.length) {
          <h3>Posts</h3>
          <div class="results-grid">
            @for (post of results()!.posts; track post.id) {
              <app-post-card [post]="post"></app-post-card>
            }
          </div>
        }
      }
    </ion-content>
  `,
  styles: [`
    .loading-container {
      display: flex;
      justify-content: center;
      padding: 2rem;
    }

    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--ion-color-medium);
    }

    .results-grid {
      display: grid;
      grid-template-columns: 1fr;
      gap: 1px;
    }

    h3 {
      margin: 1rem 0 0.5rem;
    }
  `],
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
