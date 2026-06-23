import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  private _nextId = 0;

  show(message: string, type: ToastType = 'info', duration = 4000): void {
    const id = ++this._nextId;
    this._toasts.update(t => {

      const capped = t.length >= 5 ? t.slice(1) : t;
      return [...capped, { id, message, type }]
    });
    setTimeout(() => this.dismiss(id), duration);
  }

  dismiss(id: number): void {
    this._toasts.update(t => t.filter(x => x.id !== id));
  }
}
