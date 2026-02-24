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
  selector: 'app-liked',
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
  templateUrl: './liked.page.html',
  styleUrls: ['./liked.page.scss'],
})
export class LikedPage implements OnInit {
  private readonly api = inject(ApiService);

  readonly posts = signal<FeedPostResponse[]>([]);
  readonly isLoading = signal(false);

  ngOnInit(): void {
    this.loadLiked();
  }

  loadLiked(): void {
    this.isLoading.set(true);

    this.api.get<FeedPostResponse[]>('posts', 'liked').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.posts.set(response.data);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadLiked();
    setTimeout(() => event.target.complete(), 1000);
  }
}
