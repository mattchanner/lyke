import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SkeletonComponent } from '../skeleton';

@Component({
  selector: 'app-skeleton-list',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  templateUrl: './skeleton-list.component.html',
  styleUrls: ['./skeleton-list.component.scss'],
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
