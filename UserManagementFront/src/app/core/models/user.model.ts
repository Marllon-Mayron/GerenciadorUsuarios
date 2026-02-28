import { UserStatus } from '../models/enums/user-status.enum';
import { UserRole } from '../models/enums/user-role.enum';

export interface User {
  id: string;
  name: string;
  email: string;
  passwordHash?: string;
  status: UserStatus;
  role: UserRole;
  createdAt: Date;
  updatedAt: Date;
}