// core/services/token.service.ts
import { Injectable } from '@angular/core';
import { UserDto } from '../../shared/models/dtos/user-dto.dto';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'user_data';

  constructor() { }

  saveToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  saveUser(user: UserDto): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  getUser(): UserDto | null {
    const user = localStorage.getItem(this.USER_KEY);
    if (user) {
      try {
        return JSON.parse(user);
      } catch {
        return null;
      }
    }
    return null;
  }

  clearStorage(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  // Método útil para verificar se o token existe
  hasToken(): boolean {
    return !!this.getToken();
  }

  // Método para atualizar usuário (útil quando o perfil é editado)
  updateUser(user: Partial<UserDto>): void {
    const currentUser = this.getUser();
    if (currentUser) {
      const updatedUser = { ...currentUser, ...user };
      this.saveUser(updatedUser);
    }
  }
}
