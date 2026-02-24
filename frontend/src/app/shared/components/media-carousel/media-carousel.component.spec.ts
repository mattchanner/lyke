import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { MediaCarouselComponent, MediaItem } from './media-carousel.component';
import { MediaType } from '../../../models';

describe('MediaCarouselComponent', () => {
  let component: MediaCarouselComponent;
  let fixture: ComponentFixture<MediaCarouselComponent>;

  const mockMedia: MediaItem[] = [
    { url: 'http://example.com/img1.jpg', type: MediaType.Image },
    { url: 'http://example.com/img2.jpg', type: MediaType.Image },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MediaCarouselComponent],
      schemas: [CUSTOM_ELEMENTS_SCHEMA],
    }).compileComponents();
    fixture = TestBed.createComponent(MediaCarouselComponent);
    component = fixture.componentInstance;
  });

  it('should create with empty media', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should initialize currentIndex to 0', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    expect(component.currentIndex()).toBe(0);
  });

  it('should initialize mediaLoaded array matching media length', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    expect(component.mediaLoaded().length).toBe(2);
    expect(component.mediaLoaded().every(v => v === false)).toBe(true);
  });

  it('should emit mediaClicked on onMediaClick', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    spyOn(component.mediaClicked, 'emit');
    component.onMediaClick(1);
    expect(component.mediaClicked.emit).toHaveBeenCalledWith(1);
  });

  it('should emit fullscreenRequested on onFullscreen', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    spyOn(component.fullscreenRequested, 'emit');
    component.onFullscreen();
    expect(component.fullscreenRequested.emit).toHaveBeenCalledWith(0);
  });

  it('should toggle isMuted on toggleMute', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    expect(component.isMuted()).toBe(true);
    component.toggleMute();
    expect(component.isMuted()).toBe(false);
    component.toggleMute();
    expect(component.isMuted()).toBe(true);
  });

  it('should update currentIndex on onSlideChange', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    const mockEvent = { target: { swiper: { activeIndex: 1 } } } as unknown as Event;
    component.onSlideChange(mockEvent);
    expect(component.currentIndex()).toBe(1);
  });

  it('should update mediaLoaded on onMediaLoaded', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    component.onMediaLoaded(0);
    expect(component.mediaLoaded()[0]).toBe(true);
    expect(component.mediaLoaded()[1]).toBe(false);
  });

  it('should toggle isPlaying on togglePlay', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    const mockVideo = { paused: true, play: jasmine.createSpy('play'), pause: jasmine.createSpy('pause') } as unknown as HTMLVideoElement;
    component.togglePlay(mockVideo);
    expect(mockVideo.play).toHaveBeenCalled();
    expect(component.isPlaying()).toBe(true);
  });

  it('should pause playing video on togglePlay', () => {
    component.media = mockMedia;
    fixture.detectChanges();
    const mockVideo = { paused: false, play: jasmine.createSpy('play'), pause: jasmine.createSpy('pause') } as unknown as HTMLVideoElement;
    component.togglePlay(mockVideo);
    expect(mockVideo.pause).toHaveBeenCalled();
    expect(component.isPlaying()).toBe(false);
  });
});
