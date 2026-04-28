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
  templateUrl: './media-carousel.component.html',
  styleUrls: ['./media-carousel.component.scss'],
})
export class MediaCarouselComponent {
  @Input() media: MediaItem[] = [];
  @Input() alt = 'Media';
  @Input() showFullscreenButton = false;
  @Input() autoplay = false;
  /** Use thumbnail URLs for image src — suitable for card/list views */
  @Input() compact = false;

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

  onSlideChange(event: Event): void {
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
