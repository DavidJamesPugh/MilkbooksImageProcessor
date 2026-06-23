import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ImageService, SseCompleteEvent } from '../../services/image';
import { ToastService } from '../../services/toast';
import { friendlyHttpError } from '../../utils/http-errors';
import { ImageResponseItem } from '../../models/image-result';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule, DecimalPipe],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class HomeComponent {
  private imageService = inject(ImageService);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  query = '';
  images = signal<ImageResponseItem[]>([]);
  loading = signal(false);
  progress = signal(0);
  progressLabel = signal('');

  downloadImages(): void {
    if (!this.query.trim()) {
      this.toast.show('Please enter a search query.', 'info');
      return;
    }
    if (this.loading()) return;

    this.loading.set(true);
    this.progress.set(0);
    this.progressLabel.set('Fetching from Unsplash...');
    this.images.set([]);

    this.imageService.streamImages(this.query.trim())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: (msg) => {
        if (msg.type === 'progress') {
          this.progress.set(Math.round((msg.current / msg.total) * 100));
          this.progressLabel.set(`Processing ${msg.current} of ${msg.total} images...`);
          if (msg.image) {
            this.images.update(imgs => [...imgs, msg.image!]);
          }
        } else if (msg.type === 'complete') {
          this.handleComplete(msg);
          this.progressLabel.set(`Processed ${msg.successCount} images.`);
          setTimeout(() => { this.progress.set(0); this.loading.set(false); }, 600);
        } else if (msg.type === 'error') {
          this.toast.show(msg.error, 'error');
          setTimeout(() => { this.progress.set(0); this.loading.set(false); }, 600);
          ;
        }
      },
      error: (err) => {
        this.toast.show(friendlyHttpError(err), 'error');
        this.loading.set(false);
      },
      complete: () => {
        this.loading.set(false);
      }
    });
  }

  private handleComplete(res: SseCompleteEvent): void {
    this.progress.set(100);

    if (res.successCount === 0) {
      this.toast.show('No images found for that query.', 'info');
    } else if (res.isPartialResult) {
      this.toast.show(
        `${res.successCount} of ${res.successCount + res.failureCount} images loaded — ${res.failureCount} failed.`,
        'error'
      );
    } else {
      this.toast.show(`${res.successCount} image${res.successCount === 1 ? '' : 's'} loaded.`, 'success');
    }
  }
}
