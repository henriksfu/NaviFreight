export interface UserResponse {
  userId: number;
  email: string;
  displayName: string;
  role: string;
  isActive: boolean;
  createdUtc: string;
}

export interface CreateUserRequest {
  email: string;
  displayName: string;
  role: string;
  password: string;
}

export interface UpdateUserRequest {
  displayName: string;
  role: string;
  newPassword?: string;
}
