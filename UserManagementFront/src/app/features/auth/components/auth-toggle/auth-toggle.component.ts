import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auth-toggle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './auth-toggle.component.html',
  styleUrls: ['./auth-toggle.component.css']
})
export class AuthToggleComponent {
  @Input() isLoginMode: boolean = true;
  @Output() toggleMode = new EventEmitter<void>();
}