import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../../../core/services/user.service';
import { AuthService } from '../../../../core/services/auth.service';
import { TokenService } from '../../../../core/services/token.service';
import { UpdateUserDto } from '../../../../shared/models/dtos/update-user-dto.dto';
import { UserDto } from '../../../../shared/models/dtos/user-dto.dto';

@Component({
  selector: 'app-user-info',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-info.component.html'
})
export class UserInfoComponent implements OnInit {
  user: UserDto | null = null;
  isLoading = true;
  isEditing = false;
  isSaving = false;
  isAdmin = false;
  editMessage = '';
  editForm!: FormGroup;

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private tokenService: TokenService,
    private router: Router,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.loadUserData();
    this.initForm();
    this.isAdmin = this.userService.isAdmin();
  }

  private initForm(): void {
    this.editForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  private loadUserData(): void {
    this.isLoading = true;
    
    try {
      this.userService.getCurrentUser().subscribe({
        next: (user) => {
          this.user = user;
          this.isLoading = false;
          
          // Atualizar form com dados atuais
          this.editForm.patchValue({
            name: user.name,
            email: user.email
          });
        },
        error: (error) => {
          console.error('Erro ao carregar usuário:', error);
          this.isLoading = false;
          
          // Se não conseguir carregar, tenta pegar do token
          this.user = this.tokenService.getUser();
        }
      });
    } catch (error) {
      console.error('Erro ao carregar usuário:', error);
      this.isLoading = false;
      this.user = this.tokenService.getUser();
    }
  }

  getUserInitials(): string {
    if (!this.user?.name) return 'U';
    
    const names = this.user.name.split(' ');
    if (names.length >= 2) {
      return (names[0][0] + names[1][0]).toUpperCase();
    }
    return names[0][0].toUpperCase();
  }

  toggleEditMode(): void {
    this.isEditing = !this.isEditing;
    this.editMessage = '';
    
    if (this.isEditing && this.user) {
      this.editForm.patchValue({
        name: this.user.name,
        email: this.user.email
      });
    }
  }

  updateProfile(): void {
    if (this.editForm.invalid || !this.user) return;

    this.isSaving = true;
    this.editMessage = '';

    const updateData: UpdateUserDto = {
      name: this.editForm.value.name,
      email: this.editForm.value.email
    };

    this.userService.updateCurrentUser(updateData).subscribe({
      next: (updatedUser) => {
        this.isSaving = false;
        this.user = updatedUser;
        this.isEditing = false;
        
        // Atualizar dados no token service
        this.tokenService.saveUser(updatedUser);
      },
      error: (error) => {
        this.isSaving = false;
        this.editMessage = error.error?.message || 'Erro ao atualizar perfil';
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  viewAllUsers(): void {
    // Navegar para a lista de usuários (será implementado depois)
    console.log('Navegar para lista de usuários');
  }
}