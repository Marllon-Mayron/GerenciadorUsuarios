import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from '../auth/pages/login/login.component';
import { RegisterComponent } from '../auth/pages/register/register.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, LoginComponent, RegisterComponent],
  templateUrl: './home.component.html'
})
export class HomeComponent {
  isLoginMode = true;

  setMode(isLogin: boolean): void {
    this.isLoginMode = isLogin;
  }
}