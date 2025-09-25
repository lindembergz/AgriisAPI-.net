import { Usuario } from './user.model';

/**
 * Login form interface
 */
export interface LoginForm {
  username: string;
  password: string;
  rememberMe: boolean;
}

/**
 * Authentication response interface
 */
export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: Usuario;
  expiresIn: number;
}

/**
 * Token refresh request interface
 */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/**
 * Authentication state interface
 */
export interface AuthState {
  isAuthenticated: boolean;
  user?: Usuario;
  token?: string;
  refreshToken?: string;
}