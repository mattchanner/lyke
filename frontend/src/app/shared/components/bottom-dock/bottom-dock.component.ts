import { Component, computed, effect, inject } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs/operators';
import { IonIcon } from '@ionic/angular/standalone';
import { AuthService } from '../../../core';

@Component({
  selector: 'app-bottom-dock',
  standalone: true,
  templateUrl: './bottom-dock.component.html',
  styleUrls: ['./bottom-dock.component.scss'],
  imports: [RouterLink, RouterLinkActive, IonIcon],
})
export class BottomDockComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((e): e is NavigationEnd => e instanceof NavigationEnd),
      map((e) => e.urlAfterRedirects),
    ),
    { initialValue: this.router.url },
  );

  readonly shouldShow = computed(() => {
    if (!this.auth.isAuthenticated()) return false;
    const url = this.currentUrl();
    if (url.startsWith('/auth')) return false;
    if (url.startsWith('/onboarding')) return false;
    if (url.startsWith('/style-quiz')) return false;
    return true;
  });

  readonly createPostPath = computed(() =>
    this.auth.isCreator() ? '/creator/posts/create' : '/my-posts/create',
  );

  constructor() {
    effect(() => {
      document.body.classList.toggle('has-bottom-dock', this.shouldShow());
    });
  }
}
