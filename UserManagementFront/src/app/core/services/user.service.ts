import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDto } from '../../shared/models/dtos/user-dto.dto';
import { TokenService } from './token.service';
import { UpdateUserDto } from '../../shared/models/dtos/update-user-dto.dto';
import { PaginatedResponse } from '../../shared/models/dtos/paginated-response.dto';
import { StatisticsResponse } from '../../shared/models/dtos/user-statistics.dto';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = 'http://localhost:5249/api/users';

  constructor(
    private http: HttpClient,
    private tokenService: TokenService
  ) { }

  private getHeaders(): HttpHeaders {
    const token = this.tokenService.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  // Buscar todos os usuários com paginação
  getAllUsersPaginator(pageNumber: number = 1, pageSize: number = 10): Observable<PaginatedResponse<UserDto>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<UserDto>>(this.apiUrl + '/paginator', {
      params,
      headers: this.getHeaders()
    });
  }

  // Buscar todos os usuários
  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.apiUrl, {
      headers: this.getHeaders()
    });
  }

  // Buscar usuário por ID
  getUserById(id: string): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
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
    return this.http.put<UserDto>(`${this.apiUrl}/${id}`, userData, {
      headers: this.getHeaders()
    });
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
    return this.http.patch<UserDto>(`${this.apiUrl}/${id}/status`, dto, {
      headers: this.getHeaders()
    });
  }

  // Promover a admin - apenas admin
  promoteToAdmin(id: string): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/${id}/promote`, {}, {
      headers: this.getHeaders()
    });
  }

  // Rebaixar para user - apenas admin
  demoteToUser(id: string): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/${id}/demote`, {}, {
      headers: this.getHeaders()
    });
  }

  // Deletar usuário - apenas admin
  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  // Verificar se é admin
  isAdmin(): boolean {
    const user = this.tokenService.getUser();
    return user?.role === 'Admin';
  }

  // Estatisticas para os graficos do dashboard
  getUserStatistics(): Observable<StatisticsResponse> {
    return this.http.get<StatisticsResponse>(`${this.apiUrl}/statistics`, {
      headers: this.getHeaders()
    });
  }
}
