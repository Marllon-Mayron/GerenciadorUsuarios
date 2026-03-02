import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { UserTableComponent } from './components/user-table/user-table.component';
import { UserDto } from '../../../../shared/models/dtos/user-dto.dto';
import { UserStatisticsDto } from '../../../../shared/models/dtos/user-statistics.dto';
import { UserChartsComponent } from './components/user-charts/user-charts.component';
import { PaginatorComponent } from '../../../../shared/components/paginator/paginator.component';
import { UserService } from '../../../../core/services/user.service';
import { PaginatedResponse } from '../../../../shared/models/dtos/paginated-response.dto';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    UserTableComponent,
    PaginatorComponent,
    UserChartsComponent,
  ],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  users: UserDto[] = [];

  // Propriedades para os gráficos
  statistics: UserStatisticsDto | null = null;
  chartsLoading = true;
  chartsError = '';

  // Propriedades de paginação
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 1;
  hasPreviousPage = false;
  hasNextPage = false;

  // Estados
  isLoading = true;
  errorMessage = '';

  constructor(
    public userService: UserService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUsers();
    this.loadStatistics();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.userService.getAllUsersPaginator(this.currentPage, this.pageSize).subscribe({
      next: (response: PaginatedResponse<UserDto>) => {
        this.users = response.items;
        this.totalItems = response.totalItems;
        this.totalPages = response.totalPages;
        this.hasPreviousPage = response.hasPreviousPage;
        this.hasNextPage = response.hasNextPage;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar usuários:', error);
        this.errorMessage = 'Erro ao carregar usuários. Tente novamente.';
        this.isLoading = false;

        if (error.status === 403) {
          this.router.navigate(['/user-info']);
        }
      }
    });
  }

  loadStatistics(): void {
    if (!this.userService.isAdmin()) {
      this.chartsLoading = false;
      return;
    }

    this.chartsLoading = true;
    this.chartsError = '';

    this.userService.getUserStatistics().subscribe({
      next: (response) => {
        if (response.success) {
          this.statistics = response.data;
        }
        this.chartsLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar estatísticas:', error);
        this.chartsError = 'Erro ao carregar estatísticas';
        this.chartsLoading = false;
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadUsers();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadUsers();
  }

  onUserUpdated(updatedUser: UserDto): void {
    const index = this.users.findIndex(u => u.id === updatedUser.id);
    if (index !== -1) {
      this.users[index] = updatedUser;
    }
    this.loadStatistics();
  }

  onUserDeleted(deletedUserId: string): void {
    this.users = this.users.filter(u => u.id !== deletedUserId);
    this.totalItems--;

    this.cdr.detectChanges();

    if (this.users.length === 0 && this.currentPage > 1) {
      this.currentPage--;
      this.loadUsers();
    } else {
      this.totalPages = Math.ceil(this.totalItems / this.pageSize);
      this.hasPreviousPage = this.currentPage > 1;
      this.hasNextPage = this.currentPage < this.totalPages;
    }

    this.loadStatistics();
  }

  goBack(): void {
    this.router.navigate(['/user-info']);
  }

  retryLoading(): void {
    this.loadUsers();
    this.loadStatistics();
  }
}
