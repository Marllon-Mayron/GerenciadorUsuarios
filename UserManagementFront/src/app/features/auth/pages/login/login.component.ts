import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  @Output() switchToRegisterMode = new EventEmitter<void>();
  
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.showErrors();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.router.navigate(['/user-info']);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.status === 401 
          ? 'E-mail ou senha incorretos' 
          : 'Erro ao fazer login';
      }
    });
  }

  switchToRegister(): void {
    this.switchToRegisterMode.emit();
  }

  private showErrors(): void {
    const controls = this.loginForm.controls;
    let message = '';
    
    if (controls['email'].errors?.['required']) message = 'E-mail é obrigatório';
    else if (controls['email'].errors?.['email']) message = 'E-mail inválido';
    else if (controls['password'].errors?.['required']) message = 'Senha é obrigatória';
    else if (controls['password'].errors?.['minlength']) message = 'Senha deve ter 6+ caracteres';
    
    this.errorMessage = message;
  }
}