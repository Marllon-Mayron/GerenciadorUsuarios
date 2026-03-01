import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDto } from '../../shared/models/dtos/user-dto.dto';
import { TokenService } from './token.service';
import { ChangeStatusDto } from '../../shared/models/dtos/change-status-dto.dto';
import { UpdateUserDto } from '../../shared/models/dtos/update-user-dto.dto';
import { PaginatedResponse } from '../../shared/models/dtos/paginated-response.dto';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5249/api/users';

  constructor(
    private http: HttpClient,
    private tokenService: TokenService
  ) { }


  // Buscar todos os usuários com paginação
  getAllUsersPaginator(pageNumber: number = 1, pageSize: number = 10): Observable<PaginatedResponse<UserDto>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<UserDto>>(this.apiUrl+'/paginator', { params });
  }

  // Buscar todos os usuários
  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.apiUrl);
  }

  // Buscar usuário por ID
  getUserById(id: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/${id}`);
  }

  // Buscar usuário atual (logado)
  getCurrentUser(): Observable<UserDto> {
    const user = this.tokenService.getUser();
    if (user && user.id) {
      return this.getUserById(user.id);
    }
    throw new Error('Usuário não encontrado');
  }

  // Atualizar usuário
  updateUser(id: string, userData: UpdateUserDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.apiUrl}/${id}`, userData);
  }

  // Atualizar usuário atual
  updateCurrentUser(userData: UpdateUserDto): Observable<UserDto> {
    const user = this.tokenService.getUser();
    if (user && user.id) {
      return this.updateUser(user.id, userData);
    }
    throw new Error('Usuário não encontrado');
  }

  // Mudar status (ativar/desativar) - apenas admin
  changeStatus(id: string, activate: boolean): Observable<UserDto> {
    const dto = { Activate: activate };
    return this.http.patch<UserDto>(`${this.apiUrl}/${id}/status`, dto);
  }

  // Promover a admin - apenas admin
  promoteToAdmin(id: string): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/${id}/promote`, {});
  }

  // Rebaixar para user - apenas admin
  demoteToUser(id: string): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/${id}/demote`, {});
  }

  // Deletar usuário - apenas admin
  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Verificar se é admin
  isAdmin(): boolean {
    const user = this.tokenService.getUser();
    return user?.role === 'Admin';
  }
}
