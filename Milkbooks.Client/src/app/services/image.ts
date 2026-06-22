import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ImageService {

  constructor(private http: HttpClient) { }

  getImages(query: string): Observable<any> {
    return this.http.get(`/api/images/${query}`);
  }
}
