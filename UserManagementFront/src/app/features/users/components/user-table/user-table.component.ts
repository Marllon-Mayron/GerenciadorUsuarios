import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserDto } from '../../../../shared/models/dtos/user-dto.dto';
import { UpdateUserDto } from '../../../../shared/models/dtos/update-user-dto.dto';
import { UserService } from '../../../../core/services/user.service';

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
  hasChanges = false;
  isChangingStatus = false;
  isChangingRole = false;

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
      email: ['', [Validators.required, Validators.email]],
      status: [true],
      role: ['User']
    });

    // Monitora mudanças no formulário
    this.editForm.valueChanges.subscribe(() => {
      this.checkForChanges();
    });
  }

  // Verifica se houve alterações em relação ao usuário original
  private checkForChanges(): void {
    if (!this.selectedUser) {
      this.hasChanges = false;
      return;
    }

    const currentValues = this.editForm.value;
    const originalStatus = this.selectedUser.status === 'Ativo';

    this.hasChanges =
      currentValues.name !== this.selectedUser.name ||
      currentValues.email !== this.selectedUser.email ||
      currentValues.status !== originalStatus ||
      currentValues.role !== this.selectedUser.role;
  }

  // Métodos para edição
  openEditModal(user: UserDto): void {
    this.selectedUser = user;
    this.editForm.patchValue({
      name: user.name,
      email: user.email,
      status: user.status === 'Ativo',
      role: user.role
    });
    this.showEditModal = true;
    this.editMessage = '';
    this.hasChanges = false;
    this.isChangingStatus = false;
    this.isChangingRole = false;
  }

  closeEditModal(): void {
    this.showEditModal = false;
    this.selectedUser = null;
    this.editMessage = '';
    this.hasChanges = false;
    this.isChangingStatus = false;
    this.isChangingRole = false;
  }

  updateUser(): void {
    if (this.editForm.invalid || !this.selectedUser || !this.hasChanges) return;

    this.isSaving = true;
    this.editMessage = '';

    const currentValues = this.editForm.value;
    const originalStatus = this.selectedUser.status === 'Ativo';
    const statusChanged = currentValues.status !== originalStatus;
    const roleChanged = currentValues.role !== this.selectedUser.role;
    const basicInfoChanged =
      currentValues.name !== this.selectedUser.name ||
      currentValues.email !== this.selectedUser.email;

    // Array para armazenar as operações
    const operations: Promise<any>[] = [];

    if (basicInfoChanged) {
      const updateData: UpdateUserDto = {
        name: currentValues.name,
        email: currentValues.email
      };

      operations.push(
        this.userService.updateUser(this.selectedUser.id, updateData).toPromise()
      );
    }

    if (statusChanged) {
      this.isChangingStatus = true;
      operations.push(
        this.userService.changeStatus(this.selectedUser.id, currentValues.status).toPromise()
      );
    }

    if (roleChanged) {
      this.isChangingRole = true;
      const roleOperation = currentValues.role === 'Admin'
        ? this.userService.promoteToAdmin(this.selectedUser.id).toPromise()
        : this.userService.demoteToUser(this.selectedUser.id).toPromise();

      operations.push(roleOperation);
    }

    // Executar todas as operações em paralelo
    Promise.all(operations)
      .then((results) => {
        // Pega o último resultado nãonulo (ou o único)
        const lastResult = results.find(r => r !== null && r !== undefined);
        this.isSaving = false;
        this.isChangingStatus = false;
        this.isChangingRole = false;
        this.closeEditModal();
        if (lastResult) {
          this.userUpdated.emit(lastResult as UserDto);
        } else {
          // Se não houve resultado (só atualizações de role que não retornam UserDto?), recarrega
          this.userService.getUserById(this.selectedUser!.id).subscribe(user => {
            this.userUpdated.emit(user);
          });
        }
      })
      .catch((error) => {
        this.isSaving = false;
        this.isChangingStatus = false;
        this.isChangingRole = false;
        this.editMessage = error.error?.message || 'Erro ao atualizar usuário';
      });
  }

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
  if (!this.selectedUser) {
    return;
  }

  // Guarda o ID antes de qualquer operação
  const userIdToDelete = this.selectedUser.id;
  const userName = this.selectedUser.name;

  console.log('🗑️ Iniciando exclusão do usuário:', {
    id: userIdToDelete,
    name: userName
  });

  this.isDeleting = true;
  this.deleteMessage = '';

  this.userService.deleteUser(userIdToDelete).subscribe({
    next: () => {

      this.userDeleted.emit(userIdToDelete);

      this.isDeleting = false;
      this.deleteMessage = '';

      this.showDeleteModal = false;
      this.selectedUser = null;
    },
    error: (error) => {
      this.isDeleting = false;
      this.deleteMessage = error.error?.message || 'Erro ao excluir usuário';
    }
  });
}

  getUserInitials(name: string): string {
    if (!name) return 'U';
    const names = name.split(' ');
    if (names.length >= 2) {
      return (names[0][0] + names[1][0]).toUpperCase();
    }
    return names[0][0].toUpperCase();
  }
}
