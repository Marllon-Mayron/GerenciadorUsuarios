import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html'
})
export class RegisterComponent implements OnInit {
  @Output() registrationSuccess = new EventEmitter<void>();
  @Output() switchToLoginMode = new EventEmitter<void>();
  
  registerForm!: FormGroup;
  isLoading = false;
  message = '';
  messageType: 'success' | 'error' = 'error';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.showErrors();
      return;
    }

    if (this.registerForm.value.password !== this.registerForm.value.confirmPassword) {
      this.message = 'As senhas não coincidem';
      this.messageType = 'error';
      return;
    }

    this.isLoading = true;
    this.message = '';
    
    const { confirmPassword, ...userData } = this.registerForm.value;

    this.authService.register(userData).subscribe({
      next: () => {
        this.isLoading = false;
        this.message = 'Cadastro realizado com sucesso!';
        this.messageType = 'success';
        
        setTimeout(() => {
          this.registrationSuccess.emit();
        }, 2000);
      },
      error: (error) => {
        this.isLoading = false;
        this.message = error.error?.message || 'Erro ao fazer cadastro';
        this.messageType = 'error';
      }
    });
  }

  switchToLogin(): void {
    this.switchToLoginMode.emit();
  }

  private showErrors(): void {
    const controls = this.registerForm.controls;
    let message = '';
    
    if (controls['name'].errors?.['required']) message = 'Nome é obrigatório';
    else if (controls['name'].errors?.['minlength']) message = 'Nome deve ter 3+ caracteres';
    else if (controls['email'].errors?.['required']) message = 'E-mail é obrigatório';
    else if (controls['email'].errors?.['email']) message = 'E-mail inválido';
    else if (controls['password'].errors?.['required']) message = 'Senha é obrigatória';
    else if (controls['password'].errors?.['minlength']) message = 'Senha deve ter 6+ caracteres';
    else if (controls['confirmPassword'].errors?.['required']) message = 'Confirme sua senha';
    
    if (message) {
      this.message = message;
      this.messageType = 'error';
    }
  }
}