import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SkeletonComponent } from '../skeleton';

@Component({
  selector: 'app-skeleton-profile',
  standalone: true,
  imports: [CommonModule, SkeletonComponent],
  templateUrl: './skeleton-profile.component.html',
  styleUrls: ['./skeleton-profile.component.scss'],
})
export class SkeletonProfileComponent {}
