import { TestBed } from '@angular/core/testing';
import { PostEngagementService, EngagementChange } from './post-engagement.service';

describe('PostEngagementService', () => {
  let service: PostEngagementService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PostEngagementService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should emit when notify is called', (done) => {
    const change: EngagementChange = { postId: 'p1', type: 'like', state: true };

    service.engagementChanged.subscribe((emitted) => {
      expect(emitted).toEqual(change);
      done();
    });

    service.notify(change);
  });

  it('should emit like engagement changes', (done) => {
    const change: EngagementChange = { postId: 'p2', type: 'like', state: false };

    service.engagementChanged.subscribe((emitted) => {
      expect(emitted.type).toBe('like');
      expect(emitted.state).toBe(false);
      done();
    });

    service.notify(change);
  });

  it('should emit save engagement changes', (done) => {
    const change: EngagementChange = { postId: 'p3', type: 'save', state: true };

    service.engagementChanged.subscribe((emitted) => {
      expect(emitted.type).toBe('save');
      expect(emitted.state).toBe(true);
      done();
    });

    service.notify(change);
  });
});
