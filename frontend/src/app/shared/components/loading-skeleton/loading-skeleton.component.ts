import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="skeleton"
      [class.circle]="type === 'circle'"
      [class.text]="type === 'text'"
      [class.image]="type === 'image'"
      [class.button]="type === 'button'"
      [style.width]="width"
      [style.height]="height"
      [style.border-radius]="borderRadius"
    ></div>
  `,
  styles: [`
    .skeleton {
      background: linear-gradient(
        90deg,
        var(--ion-color-light) 25%,
        var(--ion-color-light-shade) 50%,
        var(--ion-color-light) 75%
      );
      background-size: 200% 100%;
      animation: shimmer 1.5s infinite;
    }

    .text {
      height: 1em;
      border-radius: 4px;
    }

    .circle {
      border-radius: 50%;
    }

    .image {
      border-radius: 8px;
    }

    .button {
      border-radius: 8px;
      height: 44px;
    }

    @keyframes shimmer {
      0% {
        background-position: 200% 0;
      }
      100% {
        background-position: -200% 0;
      }
    }
  `],
})
export class SkeletonComponent {
  @Input() type: 'text' | 'circle' | 'image' | 'button' | 'custom' = 'text';
  @Input() width = '100%';
  @Input() height = 'auto';
  @Input() borderRadius = '';
}

@Component({
  selector: 'app-skeleton-post-card',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  template: `
    <div class="skeleton-post-card">
      <!-- Image skeleton -->
      <app-skeleton type="image" width="100%" [height]="imageHeight"></app-skeleton>

      <!-- Content skeleton -->
      <div class="content">
        <!-- Creator row -->
        <div class="creator-row">
          <app-skeleton type="circle" width="36px" height="36px"></app-skeleton>
          <div class="creator-info">
            <app-skeleton type="text" width="120px" height="14px"></app-skeleton>
            <app-skeleton type="text" width="80px" height="12px"></app-skeleton>
          </div>
        </div>

        <!-- Title -->
        <app-skeleton type="text" width="90%" height="16px"></app-skeleton>

        <!-- Products -->
        <div class="products">
          <app-skeleton type="custom" width="80px" height="24px" borderRadius="12px"></app-skeleton>
          <app-skeleton type="custom" width="100px" height="24px" borderRadius="12px"></app-skeleton>
        </div>

        <!-- Engagement row -->
        <div class="engagement-row">
          <div class="stats">
            <app-skeleton type="text" width="40px" height="12px"></app-skeleton>
            <app-skeleton type="text" width="40px" height="12px"></app-skeleton>
          </div>
          <div class="actions">
            <app-skeleton type="circle" width="24px" height="24px"></app-skeleton>
            <app-skeleton type="circle" width="24px" height="24px"></app-skeleton>
            <app-skeleton type="circle" width="24px" height="24px"></app-skeleton>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .skeleton-post-card {
      background: var(--ion-background-color);
    }

    .content {
      padding: 0.75rem;
    }

    .creator-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.75rem;
    }

    .creator-info {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }

    .products {
      display: flex;
      gap: 0.5rem;
      margin: 0.75rem 0;
    }

    .engagement-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding-top: 0.5rem;
      border-top: 1px solid var(--ion-color-light);
    }

    .stats {
      display: flex;
      gap: 1rem;
    }

    .actions {
      display: flex;
      gap: 0.5rem;
    }
  `],
})
export class SkeletonPostCardComponent {
  @Input() imageHeight = '300px';
}

@Component({
  selector: 'app-skeleton-post-detail',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  template: `
    <div class="skeleton-post-detail">
      <!-- Media skeleton -->
      <app-skeleton type="image" width="100%" height="400px" borderRadius="0"></app-skeleton>

      <!-- Content skeleton -->
      <div class="content">
        <!-- Creator row -->
        <div class="creator-row">
          <app-skeleton type="circle" width="48px" height="48px"></app-skeleton>
          <div class="creator-info">
            <app-skeleton type="text" width="140px" height="16px"></app-skeleton>
            <app-skeleton type="text" width="200px" height="14px"></app-skeleton>
          </div>
        </div>

        <!-- Engagement buttons -->
        <div class="engagement-buttons">
          <app-skeleton type="custom" width="80px" height="36px" borderRadius="8px"></app-skeleton>
          <app-skeleton type="custom" width="80px" height="36px" borderRadius="8px"></app-skeleton>
        </div>

        <!-- Title and description -->
        <div class="post-text">
          <app-skeleton type="text" width="80%" height="20px"></app-skeleton>
          <app-skeleton type="text" width="100%" height="14px"></app-skeleton>
          <app-skeleton type="text" width="60%" height="14px"></app-skeleton>
        </div>

        <!-- Products header -->
        <app-skeleton type="text" width="180px" height="18px"></app-skeleton>

        <!-- Product cards -->
        @for (i of [1, 2]; track i) {
          <div class="product-card">
            <app-skeleton type="image" width="80px" height="80px"></app-skeleton>
            <div class="product-info">
              <app-skeleton type="text" width="140px" height="16px"></app-skeleton>
              <app-skeleton type="text" width="100px" height="14px"></app-skeleton>
              <app-skeleton type="text" width="60px" height="14px"></app-skeleton>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .skeleton-post-detail {
      background: var(--ion-background-color);
    }

    .content {
      padding: 1rem;
    }

    .creator-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 1rem;
    }

    .creator-info {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .engagement-buttons {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 1rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid var(--ion-color-light);
    }

    .post-text {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }

    .product-card {
      display: flex;
      gap: 1rem;
      padding: 1rem;
      margin-top: 1rem;
      border: 1px solid var(--ion-color-light);
      border-radius: 8px;
    }

    .product-info {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
  `],
})
export class SkeletonPostDetailComponent {}

@Component({
  selector: 'app-skeleton-profile',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  template: `
    <div class="skeleton-profile">
      <!-- Avatar and info -->
      <div class="header">
        <app-skeleton type="circle" width="100px" height="100px"></app-skeleton>
        <app-skeleton type="text" width="180px" height="20px"></app-skeleton>
        <app-skeleton type="text" width="140px" height="14px"></app-skeleton>
      </div>

      <!-- List items -->
      <div class="list">
        @for (i of [1, 2, 3, 4]; track i) {
          <div class="list-item">
            <app-skeleton type="circle" width="24px" height="24px"></app-skeleton>
            <app-skeleton type="text" width="120px" height="16px"></app-skeleton>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .skeleton-profile {
      padding: 2rem 1rem;
    }

    .header {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 2rem;
    }

    .list {
      display: flex;
      flex-direction: column;
    }

    .list-item {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      border-bottom: 1px solid var(--ion-color-light);
    }
  `],
})
export class SkeletonProfileComponent {}

@Component({
  selector: 'app-skeleton-list',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  template: `
    <div class="skeleton-list">
      @for (i of items; track i) {
        <div class="list-item">
          @if (showAvatar) {
            <app-skeleton type="circle" [width]="avatarSize" [height]="avatarSize"></app-skeleton>
          }
          <div class="item-content">
            <app-skeleton type="text" width="70%" height="16px"></app-skeleton>
            @if (showSubtitle) {
              <app-skeleton type="text" width="50%" height="14px"></app-skeleton>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .skeleton-list {
      display: flex;
      flex-direction: column;
    }

    .list-item {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding: 1rem;
      border-bottom: 1px solid var(--ion-color-light);
    }

    .item-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
  `],
})
export class SkeletonListComponent {
  @Input() count = 5;
  @Input() showAvatar = true;
  @Input() showSubtitle = true;
  @Input() avatarSize = '40px';

  get items(): number[] {
    return Array.from({ length: this.count }, (_, i) => i);
  }
}
