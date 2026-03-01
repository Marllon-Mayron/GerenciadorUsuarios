import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserDto } from '../../../shared/models/dtos/user-dto.dto';
import { UpdateUserDto } from '../../../shared/models/dtos/update-user-dto.dto';
import { UserService } from '../../../core/services/user.service';

@Component({
  selector: 'app-user-table',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-table.component.html'
})
export class UserTableComponent {
  @Input() users: UserDto[] = [];
  @Output() userUpdated = new EventEmitter<UserDto>();
  @Output() userDeleted = new EventEmitter<string>();

  // Modal de edição
  showEditModal = false;
  selectedUser: UserDto | null = null;
  editForm!: FormGroup;
  isSaving = false;
  editMessage = '';

  // Modal de exclusão
  showDeleteModal = false;
  isDeleting = false;
  deleteMessage = '';

  constructor(
    private userService: UserService,
    private fb: FormBuilder
  ) {
    this.initForm();
  }

  private initForm(): void {
    this.editForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]]
    });
  }

  // Métodos para edição
  openEditModal(user: UserDto): void {
    this.selectedUser = user;
    this.editForm.patchValue({
      name: user.name,
      email: user.email
    });
    this.showEditModal = true;
    this.editMessage = '';
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.selectedUser = null;
    this.editMessage = '';
  }

  updateUser(): void {
    if (this.editForm.invalid || !this.selectedUser) return;

    this.isSaving = true;
    this.editMessage = '';

    const updateData: UpdateUserDto = {
      name: this.editForm.value.name,
      email: this.editForm.value.email
    };

    this.userService.updateUser(this.selectedUser.id, updateData).subscribe({
      next: (updatedUser) => {
        this.isSaving = false;
        this.closeEditModal();
        this.userUpdated.emit(updatedUser);
      },
      error: (error) => {
        this.isSaving = false;
        this.editMessage = error.error?.message || 'Erro ao atualizar usuário';
      }
    });
  }

  // Métodos para exclusão
  openDeleteModal(user: UserDto): void {
    this.selectedUser = user;
    this.showDeleteModal = true;
    this.deleteMessage = '';
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedUser = null;
    this.deleteMessage = '';
  }

  deleteUser(): void {
    if (!this.selectedUser) return;

    this.isDeleting = true;
    this.deleteMessage = '';

    this.userService.deleteUser(this.selectedUser.id).subscribe({
      next: () => {
        this.isDeleting = false;
        this.closeDeleteModal();
        this.userDeleted.emit(this.selectedUser!.id);
      },
      error: (error) => {
        this.isDeleting = false;
        this.deleteMessage = error.error?.message || 'Erro ao excluir usuário';
      }
    });
  }

  // Utilitário para iniciais
  getUserInitials(name: string): string {
    if (!name) return 'U';
    const names = name.split(' ');
    if (names.length >= 2) {
      return (names[0][0] + names[1][0]).toUpperCase();
    }
    return names[0][0].toUpperCase();
  }
}
