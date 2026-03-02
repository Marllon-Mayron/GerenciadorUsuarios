import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.css']
})
export class ToastComponent {
  constructor(public toastService: ToastService) {}

  getToastClass(type: string): string {
    const baseClass = 'flex items-center justify-between p-4 rounded-lg shadow-lg min-w-[300px]';
    switch (type) {
      case 'success':
        return `${baseClass} bg-green-50 text-green-800 border-l-4 border-green-500`;
      case 'error':
        return `${baseClass} bg-red-50 text-red-800 border-l-4 border-red-500`;
      case 'warning':
        return `${baseClass} bg-yellow-50 text-yellow-800 border-l-4 border-yellow-500`;
      default:
        return `${baseClass} bg-blue-50 text-blue-800 border-l-4 border-blue-500`;
    }
  }
}
