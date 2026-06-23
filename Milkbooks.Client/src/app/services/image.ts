import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ImageResponseItem } from '../models/image-result';

export type SseProgressEvent = { type: 'progress'; current: number; total: number; image?: ImageResponseItem };
export type SseCompleteEvent = { type: 'complete'; successCount: number; failureCount: number; isPartialResult: boolean };
export type SseErrorEvent    = { type: 'error'; error: string };
export type SseMessage = SseProgressEvent | SseCompleteEvent | SseErrorEvent;

@Injectable({ providedIn: 'root' })
export class ImageService {

  streamImages(query: string): Observable<SseMessage> {
    return new Observable(subscriber => {
      const controller = new AbortController();

      fetch(`/api/images?query=${encodeURIComponent(query)}`, {
        signal: controller.signal
      }).then(async response => {
        if (!response.ok) {
          subscriber.error(new HttpErrorResponse({ status: response.status }));
          return;
        }

        const reader = response.body!.getReader();
        const decoder = new TextDecoder();
        let buffer = '';

        try {
          while (true) {
            const { done, value } = await reader.read();
            if (done) break;

            buffer += decoder.decode(value, { stream: true });
            const blocks = buffer.split('\n\n');
            buffer = blocks.pop() ?? '';

            for (const block of blocks) {
              const line = block.split('\n').find(l => l.startsWith('data: '));
              if (line) subscriber.next(JSON.parse(line.slice(6)) as SseMessage);
            }
          }
          subscriber.complete();
        } catch (err) {
          if (!(err instanceof DOMException && err.name === 'AbortError')) {
            subscriber.error(err);
          }
        }
      }).catch(err => subscriber.error(err));

      return () => controller.abort();
    });
  }
}
