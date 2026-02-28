import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginDto } from '../../shared/models/dtos/login-dto.dto';
import { LoginResponseDto } from '../../shared/models/dtos/login-response-dto.dto';
import { CreateUserDto } from '../../shared/models/dtos/create-user-dto.dto';
import { UserDto } from '../../shared/models/dtos/user-dto.dto';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5249/api';

  constructor(
    private http: HttpClient,
    private tokenService: TokenService
  ) {}

  login(credentials: LoginDto): Observable<LoginResponseDto> {
    return this.http.post<LoginResponseDto>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap(response => {
          this.tokenService.saveToken(response.token);
          this.tokenService.saveUser(response.user);
        })
      );
  }

  register(userData: CreateUserDto): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.apiUrl}/users`, userData);
  }

  logout(): void {
    this.tokenService.clearStorage();
  }

  isAuthenticated(): boolean {
    return !!this.tokenService.getToken();
  }
}