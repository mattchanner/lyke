import {
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  computed,
  CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonIcon, IonSpinner } from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  playCircleOutline,
  pauseCircleOutline,
  volumeMuteOutline,
  volumeHighOutline,
  expandOutline,
} from 'ionicons/icons';
import { register } from 'swiper/element/bundle';
import { MediaType } from '../../../models';

// Register Swiper custom elements
register();

export interface MediaItem {
  url: string;
  type: MediaType;
  thumbnailUrl?: string;
}

@Component({
  selector: 'app-media-carousel',
  standalone: true,
  imports: [CommonModule, IonIcon, IonSpinner],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <div class="carousel-container" [class.single]="media.length === 1">
      @if (media.length === 1) {
        <!-- Single media item - no swiper needed -->
        <div class="media-item">
          @if (media[0].type === MediaType.Video) {
            <div class="video-container">
              <video
                #videoPlayer
                [src]="media[0].url"
                [poster]="media[0].thumbnailUrl"
                playsinline
                loop
                [muted]="isMuted()"
                (click)="togglePlay(videoPlayer)"
                (loadeddata)="onMediaLoaded(0)"
              ></video>
              <div class="video-controls">
                <ion-icon
                  [name]="isPlaying() ? 'pause-circle-outline' : 'play-circle-outline'"
                  (click)="togglePlay(videoPlayer); $event.stopPropagation()"
                ></ion-icon>
                <ion-icon
                  [name]="isMuted() ? 'volume-mute-outline' : 'volume-high-outline'"
                  (click)="toggleMute(); $event.stopPropagation()"
                ></ion-icon>
              </div>
              @if (!mediaLoaded()[0]) {
                <div class="loading-overlay">
                  <ion-spinner name="crescent"></ion-spinner>
                </div>
              }
            </div>
          } @else {
            <img
              [src]="media[0].url"
              [alt]="alt"
              loading="lazy"
              (load)="onMediaLoaded(0)"
              (click)="onMediaClick(0)"
            />
            @if (!mediaLoaded()[0]) {
              <div class="loading-overlay">
                <ion-spinner name="crescent"></ion-spinner>
              </div>
            }
          }
        </div>
      } @else {
        <!-- Multiple media items - use swiper -->
        <swiper-container
          [attr.slides-per-view]="1"
          [attr.pagination]="true"
          [attr.pagination-clickable]="true"
          (swiperslidechange)="onSlideChange($event)"
        >
          @for (item of media; track item.url; let i = $index) {
            <swiper-slide>
              <div class="media-item">
                @if (item.type === MediaType.Video) {
                  <div class="video-container">
                    <video
                      #videoPlayer
                      [src]="item.url"
                      [poster]="item.thumbnailUrl"
                      playsinline
                      loop
                      [muted]="isMuted()"
                      (click)="togglePlay(videoPlayer)"
                      (loadeddata)="onMediaLoaded(i)"
                    ></video>
                    <div class="video-controls">
                      <ion-icon
                        [name]="isPlaying() && currentIndex() === i ? 'pause-circle-outline' : 'play-circle-outline'"
                        (click)="togglePlay(videoPlayer); $event.stopPropagation()"
                      ></ion-icon>
                      <ion-icon
                        [name]="isMuted() ? 'volume-mute-outline' : 'volume-high-outline'"
                        (click)="toggleMute(); $event.stopPropagation()"
                      ></ion-icon>
                    </div>
                    @if (!mediaLoaded()[i]) {
                      <div class="loading-overlay">
                        <ion-spinner name="crescent"></ion-spinner>
                      </div>
                    }
                  </div>
                } @else {
                  <img
                    [src]="item.url"
                    [alt]="alt + ' ' + (i + 1)"
                    loading="lazy"
                    (load)="onMediaLoaded(i)"
                    (click)="onMediaClick(i)"
                  />
                  @if (!mediaLoaded()[i]) {
                    <div class="loading-overlay">
                      <ion-spinner name="crescent"></ion-spinner>
                    </div>
                  }
                }
              </div>
            </swiper-slide>
          }
        </swiper-container>

        <!-- Custom pagination indicator -->
        <div class="pagination-dots">
          @for (item of media; track item.url; let i = $index) {
            <span
              class="dot"
              [class.active]="currentIndex() === i"
            ></span>
          }
        </div>

        <!-- Slide counter -->
        <div class="slide-counter">
          {{ currentIndex() + 1 }} / {{ media.length }}
        </div>
      }

      <!-- Fullscreen button -->
      @if (showFullscreenButton) {
        <button class="fullscreen-btn" (click)="onFullscreen()">
          <ion-icon name="expand-outline"></ion-icon>
        </button>
      }
    </div>
  `,
  styles: [`
    .carousel-container {
      position: relative;
      width: 100%;
      aspect-ratio: 1;
      background: var(--ion-color-light);
      overflow: hidden;
    }

    .carousel-container.single .media-item {
      height: 100%;
    }

    swiper-container {
      width: 100%;
      height: 100%;
    }

    swiper-slide {
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .media-item {
      width: 100%;
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      position: relative;

      img {
        width: 100%;
        height: 100%;
        object-fit: cover;
        cursor: pointer;
      }
    }

    .video-container {
      width: 100%;
      height: 100%;
      position: relative;

      video {
        width: 100%;
        height: 100%;
        object-fit: cover;
        cursor: pointer;
      }
    }

    .video-controls {
      position: absolute;
      bottom: 1rem;
      left: 1rem;
      display: flex;
      gap: 0.75rem;

      ion-icon {
        font-size: 2rem;
        color: white;
        cursor: pointer;
        filter: drop-shadow(0 2px 4px rgba(0, 0, 0, 0.5));
        transition: transform 0.2s;

        &:hover {
          transform: scale(1.1);
        }
      }
    }

    .loading-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--ion-color-light);

      ion-spinner {
        --color: var(--ion-color-medium);
      }
    }

    .pagination-dots {
      position: absolute;
      bottom: 0.75rem;
      left: 50%;
      transform: translateX(-50%);
      display: flex;
      gap: 0.5rem;
      z-index: 10;

      .dot {
        width: 6px;
        height: 6px;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.5);
        transition: all 0.3s;

        &.active {
          background: white;
          width: 18px;
          border-radius: 3px;
        }
      }
    }

    .slide-counter {
      position: absolute;
      top: 0.75rem;
      right: 0.75rem;
      background: rgba(0, 0, 0, 0.6);
      color: white;
      padding: 0.25rem 0.5rem;
      border-radius: 4px;
      font-size: 0.75rem;
      z-index: 10;
    }

    .fullscreen-btn {
      position: absolute;
      top: 0.75rem;
      left: 0.75rem;
      background: rgba(0, 0, 0, 0.6);
      border: none;
      border-radius: 50%;
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
      z-index: 10;

      ion-icon {
        color: white;
        font-size: 1.25rem;
      }
    }

    /* Hide default swiper pagination - we use custom */
    swiper-container::part(pagination) {
      display: none;
    }
  `],
})
export class MediaCarouselComponent {
  @Input() media: MediaItem[] = [];
  @Input() alt = 'Media';
  @Input() showFullscreenButton = false;
  @Input() autoplay = false;

  @Output() mediaClicked = new EventEmitter<number>();
  @Output() fullscreenRequested = new EventEmitter<number>();

  readonly MediaType = MediaType;

  readonly currentIndex = signal(0);
  readonly isPlaying = signal(false);
  readonly isMuted = signal(true);
  readonly mediaLoaded = signal<boolean[]>([]);

  constructor() {
    addIcons({
      playCircleOutline,
      pauseCircleOutline,
      volumeMuteOutline,
      volumeHighOutline,
      expandOutline,
    });
  }

  ngOnInit(): void {
    // Initialize loaded state for all media items
    this.mediaLoaded.set(new Array(this.media.length).fill(false));
  }

  onSlideChange(event: CustomEvent): void {
    const swiper = event.target as any;
    this.currentIndex.set(swiper.swiper?.activeIndex ?? 0);
    this.isPlaying.set(false);
  }

  onMediaLoaded(index: number): void {
    this.mediaLoaded.update((loaded) => {
      const newLoaded = [...loaded];
      newLoaded[index] = true;
      return newLoaded;
    });
  }

  onMediaClick(index: number): void {
    this.mediaClicked.emit(index);
  }

  togglePlay(video: HTMLVideoElement): void {
    if (video.paused) {
      video.play();
      this.isPlaying.set(true);
    } else {
      video.pause();
      this.isPlaying.set(false);
    }
  }

  toggleMute(): void {
    this.isMuted.update((muted) => !muted);
  }

  onFullscreen(): void {
    this.fullscreenRequested.emit(this.currentIndex());
  }
}
