import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSpinner,
  IonRefresher,
  IonRefresherContent,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { ApiService } from '../../../core';
import { FeedPostResponse } from '../../../models';
import { PostCardComponent } from '../../../shared/components/post-card/post-card.component';

@Component({
  selector: 'app-saved',
  standalone: true,
  imports: [
    CommonModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSpinner,
    IonRefresher,
    IonRefresherContent,
    PostCardComponent,
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-back-button defaultHref="/feed"></ion-back-button>
        </ion-buttons>
        <ion-title>Saved</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content>
      <ion-refresher slot="fixed" (ionRefresh)="onRefresh($event)">
        <ion-refresher-content></ion-refresher-content>
      </ion-refresher>

      @if (isLoading()) {
        <div class="loading-container">
          <ion-spinner name="crescent"></ion-spinner>
        </div>
      } @else if (posts().length === 0) {
        <div class="empty-state">
          <h3>No saved posts</h3>
          <p>Posts you save will appear here.</p>
        </div>
      } @else {
        <div class="posts-grid">
          @for (post of posts(); track post.id) {
            <app-post-card [post]="post"></app-post-card>
          }
        </div>
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
      padding: 3rem 2rem;

      h3 {
        margin-bottom: 0.5rem;
      }

      p {
        color: var(--ion-color-medium);
      }
    }

    .posts-grid {
      display: grid;
      grid-template-columns: 1fr;
      gap: 1px;
    }
  `],
})
export class SavedPage implements OnInit {
  private readonly api = inject(ApiService);

  readonly posts = signal<FeedPostResponse[]>([]);
  readonly isLoading = signal(false);

  ngOnInit(): void {
    this.loadSaved();
  }

  loadSaved(): void {
    this.isLoading.set(true);

    this.api.get<FeedPostResponse[]>('feed', 'saved').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.posts.set(response.data);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadSaved();
    setTimeout(() => event.target.complete(), 1000);
  }
}
