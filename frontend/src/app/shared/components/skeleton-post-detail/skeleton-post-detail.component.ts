import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SkeletonComponent } from '../skeleton';

@Component({
  selector: 'app-skeleton-post-detail',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  templateUrl: './skeleton-post-detail.component.html',
  styleUrls: ['./skeleton-post-detail.component.scss'],
})
export class SkeletonPostDetailComponent {}
