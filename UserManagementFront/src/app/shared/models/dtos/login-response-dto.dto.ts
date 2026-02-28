import { UserDto } from './user-dto.dto';

export interface LoginResponseDto {
  token: string;
  user: UserDto;
}