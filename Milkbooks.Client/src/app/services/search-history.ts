import { Injectable, signal } from '@angular/core';

const STORAGE_KEY = 'milkbooks_search_history';
const MAX_ENTRIES = 8;

@Injectable({ providedIn: 'root' })
export class SearchHistoryService {
  private _history = signal<string[]>(this.load());
  readonly history = this._history.asReadonly();

  add(query: string): void {
    const trimmed = query.trim();
    if (!trimmed) return;
    this._history.update(h => {
      const deduped = h.filter(q => q.toLowerCase() !== trimmed.toLowerCase());
      const updated = [trimmed, ...deduped].slice(0, MAX_ENTRIES);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
      return updated;
    });
  }

  remove(query: string): void {
    this._history.update(h => {
      const updated = h.filter(q => q !== query);
      localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
      return updated;
    });
  }

  private load(): string[] {
    try {
      return JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '[]');
    } catch {
      return [];
    }
  }
}
