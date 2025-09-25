/**
 * Generic API response interface
 */
export interface ApiResponse<T = any> {
  data: T;
  success: boolean;
  message?: string;
  errors?: ValidationError[];
}

/**
 * Paginated API response interface
 */
export interface PaginatedResponse<T = any> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

/**
 * API error interface
 */
export interface ApiError {
  message: string;
  code: string;
  details?: any;
  timestamp: Date;
}

/**
 * Validation error interface
 */
export interface ValidationError {
  field: string;
  message: string;
  code: string;
}

/**
 * Loading state interface
 */
export interface LoadingState {
  isLoading: boolean;
  error?: string;
}