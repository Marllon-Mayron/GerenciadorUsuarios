import { Injectable } from '@angular/core';

export interface Toast {
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
  id: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toasts: Toast[] = [];
  private nextId = 0;

  show(message: string, type: 'success' | 'error' | 'info' | 'warning' = 'info'): void {
    const id = this.nextId++;
    this.toasts.push({ message, type, id });
    
    setTimeout(() => this.remove(id), 5000);
  }

  remove(id: number): void {
    this.toasts = this.toasts.filter(t => t.id !== id);
  }

  getToasts(): Toast[] {
    return this.toasts;
  }
}