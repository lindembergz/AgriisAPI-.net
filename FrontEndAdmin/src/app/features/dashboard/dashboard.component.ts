import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MockDataService } from '../../core/services/mock-data.service';
import { Order } from '../../shared/models/order.model';

import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, TagModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent {
  private mockDataService = inject(MockDataService);

  summary$ = this.mockDataService.getDashboardSummary();
  latestOrders$: Observable<Order[]> = this.mockDataService.getLatestOrders();

  getStatusSeverity(status: string): "success" | "warning" | "danger" | undefined {
    switch (status) {
      case 'Aprovado':
        return 'success';
      case 'Pendente':
        return 'warning';
      case 'Cancelado':
        return 'danger';
      default:
        return undefined;
    }
  }
}
