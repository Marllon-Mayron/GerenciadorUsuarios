import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../../core/services/auth.service';
import { ToastService } from '../../../../shared/services/toast.service';
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

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService
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
      this.toastService.error('As senhas não coincidem');
      return;
    }

    this.isLoading = true;

    const { confirmPassword, ...userData } = this.registerForm.value;

    this.authService.register(userData).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.success('Cadastro realizado com sucesso!');

        setTimeout(() => {
          this.registrationSuccess.emit();
        }, 2000);
      },
      error: (error) => {
        this.isLoading = false;
        const errorMessage = error.error?.message || 'Erro ao fazer cadastro';
        this.toastService.error(errorMessage);
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
      this.toastService.error(message);
    }
  }
}
