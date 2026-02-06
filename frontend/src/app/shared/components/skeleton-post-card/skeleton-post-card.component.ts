import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SkeletonComponent } from '../skeleton';

@Component({
  selector: 'app-skeleton-post-card',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  templateUrl: './skeleton-post-card.component.html',
  styleUrls: ['./skeleton-post-card.component.scss'],
})
export class SkeletonPostCardComponent {
  @Input() imageHeight = '300px';
}
