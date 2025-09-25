import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { Order } from '../../shared/models/order.model';

@Injectable({
  providedIn: 'root',
})
export class MockDataService {
  getDashboardSummary() {
    return of({
      totalSales: 9567.50,
      salesTarget: 15000,
      totalClients: 120,
      newOrders: 15,
    });
  }

  getLatestOrders() {
    const orders: Order[] = [
      { id: '#8547', customer: 'João Silva', date: '2024-09-16', value: 1250.00, status: 'Aprovado' },
      { id: '#8548', customer: 'Maria Oliveira', date: '2024-09-16', value: 890.50, status: 'Pendente' },
      { id: '#8549', customer: 'Carlos Pereira', date: '2024-09-15', value: 2300.00, status: 'Aprovado' },
      { id: '#8550', customer: 'Ana Costa', date: '2024-09-15', value: 450.75, status: 'Cancelado' },
      { id: '#8551', customer: 'Pedro Martins', date: '2024-09-14', value: 3200.00, status: 'Aprovado' },
    ];
    return of(orders);
  }
}
