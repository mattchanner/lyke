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
  templateUrl: './saved.page.html',
  styleUrls: ['./saved.page.scss'],
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

    this.api.get<FeedPostResponse[]>('posts', 'saved').subscribe({
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
