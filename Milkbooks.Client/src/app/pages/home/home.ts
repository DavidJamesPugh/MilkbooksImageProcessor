import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ImageService } from '../../services/image';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './home.html'
})
export class HomeComponent {

  query = '';
  images: any[] = [];
  statusMessage = '';

  constructor(private imageService: ImageService) { }

  downloadImages() {
    if (!this.query) {
      this.statusMessage = 'Please enter a search query.';
      return;
    }

    this.statusMessage = 'Loading...';

    this.imageService.getImages(this.query)
      .subscribe({
        next: (res) => {
          this.images = res.images;
          this.statusMessage = this.images.length
            ? ''
            : 'No images found for the given query.';
        },
        error: (err) => {
          this.statusMessage = err.error || 'Error occurred';
        }
      });
  }
}
