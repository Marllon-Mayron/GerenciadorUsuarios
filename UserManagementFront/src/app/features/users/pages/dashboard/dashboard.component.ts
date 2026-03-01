import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserTableComponent } from '../../components/user-table.component';
import { UserService } from '../../../../core/services/user.service';
import { UserDto } from '../../../../shared/models/dtos/user-dto.dto';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, UserTableComponent],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  users: UserDto[] = [];
  isLoading = true;

  constructor(
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  private loadUsers(): void {
    this.isLoading = true;

    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar usuários:', error);
        this.isLoading = false;

        if (error.status === 403) {
          this.router.navigate(['/user-info']);
        }
      }
    });
  }

  onUserUpdated(updatedUser: UserDto): void {
    const index = this.users.findIndex(u => u.id === updatedUser.id);
    if (index !== -1) {
      this.users[index] = updatedUser;
    }
  }

  onUserDeleted(deletedUserId: string): void {
    this.users = this.users.filter(u => u.id !== deletedUserId);
  }

  goBack(): void {
    this.router.navigate(['/user-info']);
  }
}
