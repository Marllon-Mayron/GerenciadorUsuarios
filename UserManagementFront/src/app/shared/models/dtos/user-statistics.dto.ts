export interface UserStatisticsDto {
  statusStats: StatusStatisticsDto;
  roleStats: RoleStatisticsDto;
}

export interface StatusStatisticsDto {
  active: number;
  inactive: number;
  activePercentage: number;
  inactivePercentage: number;
}

export interface RoleStatisticsDto {
  admin: number;
  user: number;
  adminPercentage: number;
  userPercentage: number;
}

export interface StatisticsResponse {
  success: boolean;
  data: UserStatisticsDto;
  message: string;
}
