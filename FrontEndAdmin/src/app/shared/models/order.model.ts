import { BaseEntity } from './base.model';

/**
 * Order status enum
 */
export enum OrderStatus {
  APROVADO = 'Aprovado',
  PENDENTE = 'Pendente',
  CANCELADO = 'Cancelado'
}

/**
 * Order entity interface
 */
export interface Order {
  id: string;
  customer: string;
  date: string;
  value: number;
  status: OrderStatus | string;
}

/**
 * Order form interface for reactive forms
 */
export interface OrderForm {
  id?: string;
  customer: string;
  date: string;
  value: number;
  status: OrderStatus | string;
}