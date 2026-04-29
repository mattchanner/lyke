import { Component, ElementRef, inject, input, output } from '@angular/core';
import { FitRating } from '../../../models';

interface RatingOption {
  value: FitRating;
  label: string;
  color: 'danger' | 'warning' | 'success';
}

// WAI-ARIA radiogroup pattern: roving tabindex, arrow-key navigation, Home/End to jump.
// Renders five fit-rating options as colored pills. Selected pill takes the colored fill;
// unselected pills are neutral outlines. Used by both the my-posts and creator post-create
// surfaces.
@Component({
  selector: 'app-fit-rating-pills',
  standalone: true,
  template: `
    <div
      class="pills"
      role="radiogroup"
      [attr.aria-label]="ariaLabel()"
    >
      @for (opt of options; track opt.value) {
        <button
          type="button"
          role="radio"
          class="pill"
          [class.selected]="selected() === opt.value"
          [attr.aria-checked]="selected() === opt.value"
          [attr.data-color]="opt.color"
          [tabindex]="tabIndexFor(opt.value)"
          (click)="select(opt.value)"
          (keydown)="onKeydown($event)"
        >
          {{ opt.label }}
        </button>
      }
    </div>
  `,
  styles: [`
    .pills {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }

    .pill {
      background: transparent;
      border: 1px solid rgba(var(--ion-text-color-rgb), 0.2);
      border-radius: 20px;
      padding: 5px 12px;
      font-size: 0.78rem;
      font-weight: 500;
      letter-spacing: 0.01em;
      color: var(--ion-color-medium);
      cursor: pointer;
      transition: background 0.2s ease, border-color 0.2s ease, color 0.2s ease;

      &:focus-visible {
        outline: 2px solid var(--ion-color-primary);
        outline-offset: 2px;
      }

      &.selected {
        border-color: transparent;
      }

      &.selected[data-color="danger"] {
        background: var(--ion-color-danger);
        color: var(--ion-color-danger-contrast);
      }

      &.selected[data-color="warning"] {
        background: var(--ion-color-warning);
        color: var(--ion-color-warning-contrast);
      }

      &.selected[data-color="success"] {
        background: var(--ion-color-success);
        color: var(--ion-color-success-contrast);
      }
    }
  `],
})
export class FitRatingPillsComponent {
  private readonly host: ElementRef<HTMLElement> = inject(ElementRef);

  readonly selected = input<FitRating>();
  readonly ariaLabel = input<string>('Fit rating');
  readonly ratingChanged = output<FitRating>();

  readonly options: RatingOption[] = [
    { value: FitRating.TooSmall, label: 'Too Small', color: 'danger' },
    { value: FitRating.SlightlySmall, label: 'Slightly Small', color: 'warning' },
    { value: FitRating.TrueToSize, label: 'True to Size', color: 'success' },
    { value: FitRating.SlightlyLarge, label: 'Slightly Large', color: 'warning' },
    { value: FitRating.TooLarge, label: 'Too Large', color: 'danger' },
  ];

  select(value: FitRating): void {
    this.ratingChanged.emit(value);
  }

  tabIndexFor(value: FitRating): number {
    const sel = this.selected();
    if (sel === undefined) {
      return value === this.options[0].value ? 0 : -1;
    }
    return sel === value ? 0 : -1;
  }

  onKeydown(event: KeyboardEvent): void {
    const navKeys = ['ArrowLeft', 'ArrowRight', 'ArrowUp', 'ArrowDown', 'Home', 'End'];
    if (!navKeys.includes(event.key)) return;
    event.preventDefault();

    const currentValue = this.selected() ?? this.options[0].value;
    const currentIndex = this.options.findIndex((o) => o.value === currentValue);
    const last = this.options.length - 1;
    let nextIndex = currentIndex;

    switch (event.key) {
      case 'ArrowLeft':
      case 'ArrowUp':
        nextIndex = currentIndex > 0 ? currentIndex - 1 : last;
        break;
      case 'ArrowRight':
      case 'ArrowDown':
        nextIndex = currentIndex < last ? currentIndex + 1 : 0;
        break;
      case 'Home':
        nextIndex = 0;
        break;
      case 'End':
        nextIndex = last;
        break;
    }

    this.ratingChanged.emit(this.options[nextIndex].value);

    // After the parent updates `selected`, move focus to the new active pill.
    queueMicrotask(() => {
      const buttons = this.host.nativeElement.querySelectorAll<HTMLButtonElement>('button.pill');
      buttons[nextIndex]?.focus();
    });
  }
}
