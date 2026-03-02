// src/app/features/dashboard/components/user-charts/user-charts.component.ts
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgxChartsModule } from '@swimlane/ngx-charts';
import { UserStatisticsDto } from '../../../../../../shared/models/dtos/user-statistics.dto';

@Component({
  selector: 'app-user-charts',
  standalone: true,
  imports: [CommonModule, NgxChartsModule],
  templateUrl: './user-charts.component.html'
})
export class UserChartsComponent implements OnChanges {
  @Input() statistics: UserStatisticsDto | null = null;
  @Input() isLoading = false;
  @Input() errorMessage = '';

  // Dados para o gráfico de Status
  statusChartData: any[] = [];
  roleChartData: any[] = [];

  colorScheme: any = {
    domain: ['#10B981', '#4B5563']
  };

  roleColorScheme: any = {
    domain: ['#3B82F6', '#4B5563']
  };


  showLegend = true;
  showLabels = true;
  doughnut = false;
  gradient = false;

  tooltipText = (tooltip: any): string => {
    try {
      if (!tooltip || !tooltip.data) return '';
      const name = tooltip.data.name || '';
      const value = tooltip.data.value || 0;
      return `${name}: ${value}`;
    } catch (e) {
      return '';
    }
  };

  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['statistics'] && this.statistics) {
      this.prepareChartData();
    }
  }

  private prepareChartData(): void {
    if (!this.statistics) return;

    this.statusChartData = [
      {
        name: 'Ativos',
        value: this.statistics.statusStats.active
      },
      {
        name: 'Inativos',
        value: this.statistics.statusStats.inactive
      }
    ];

    this.roleChartData = [
      {
        name: 'Administradores',
        value: this.statistics.roleStats.admin
      },
      {
        name: 'Usuários',
        value: this.statistics.roleStats.user
      }
    ];

  }
}
