import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DecimalPipe, PercentPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { environment } from '../../../environments/environment';

interface UsageByFeature {
  featureType: string;
  requests: number;
  tokens: number;
  cost: number;
  avgResponseTimeMs: number;
  successRate: number;
}

interface UsageByDay {
  date: string;
  requests: number;
  tokens: number;
  cost: number;
}

interface TopGrant {
  grantId: string;
  grantNumber: string;
  requests: number;
  tokens: number;
  cost: number;
}

interface UsageSummary {
  totalRequests: number;
  successfulRequests: number;
  totalTokens: number;
  totalCost: number;
  avgResponseTimeMs: number;
  successRate: number;
  byFeature: UsageByFeature[];
  byDay: UsageByDay[];
  topGrants: TopGrant[];
}

@Component({
  selector: 'app-usage-dashboard',
  standalone: true,
  imports: [
    CommonModule, FormsModule, DecimalPipe,
    MatCardModule, MatIconModule, MatButtonModule,
    MatSelectModule, MatProgressSpinnerModule, MatTableModule, MatTooltipModule
  ],
  template: `
    <div class="usage-dashboard">
      <div class="page-header">
        <h1 class="page-title">
          <mat-icon>analytics</mat-icon>
          AI Usage Dashboard
        </h1>
        <div class="controls">
          <mat-form-field appearance="outline" class="days-select">
            <mat-label>Period</mat-label>
            <mat-select [(ngModel)]="selectedDays" (ngModelChange)="load()">
              <mat-option [value]="7">Last 7 days</mat-option>
              <mat-option [value]="30">Last 30 days</mat-option>
              <mat-option [value]="90">Last 90 days</mat-option>
            </mat-select>
          </mat-form-field>
          <button mat-icon-button (click)="load()" matTooltip="Refresh">
            <mat-icon>refresh</mat-icon>
          </button>
        </div>
      </div>

      <div *ngIf="loading" class="loading-center">
        <mat-spinner diameter="48"></mat-spinner>
      </div>

      <div *ngIf="error" class="error-banner">
        <mat-icon>error_outline</mat-icon> {{ error }}
      </div>

      <ng-container *ngIf="summary && !loading">

        <!-- Summary cards -->
        <div class="stat-grid">
          <mat-card class="stat-card">
            <mat-card-content>
              <div class="stat-icon primary"><mat-icon>send</mat-icon></div>
              <div class="stat-value">{{ summary.totalRequests | number }}</div>
              <div class="stat-label">Total Requests</div>
              <div class="stat-sub">{{ summary.successRate | number:'1.0-0' }}% success</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card">
            <mat-card-content>
              <div class="stat-icon accent"><mat-icon>token</mat-icon></div>
              <div class="stat-value">{{ summary.totalTokens | number }}</div>
              <div class="stat-label">Total Tokens</div>
              <div class="stat-sub">{{ summary.totalTokens / summary.totalRequests | number:'1.0-0' }} avg/request</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card">
            <mat-card-content>
              <div class="stat-icon warn"><mat-icon>attach_money</mat-icon></div>
              <div class="stat-value">\${{ summary.totalCost | number:'1.2-4' }}</div>
              <div class="stat-label">Estimated Cost</div>
              <div class="stat-sub">\${{ summary.totalCost / summary.totalRequests | number:'1.4-4' }} per request</div>
            </mat-card-content>
          </mat-card>

          <mat-card class="stat-card">
            <mat-card-content>
              <div class="stat-icon success"><mat-icon>speed</mat-icon></div>
              <div class="stat-value">{{ summary.avgResponseTimeMs | number:'1.0-0' }}ms</div>
              <div class="stat-label">Avg Response Time</div>
              <div class="stat-sub">{{ summary.successfulRequests | number }} successful</div>
            </mat-card-content>
          </mat-card>
        </div>

        <!-- By feature -->
        <mat-card class="section-card">
          <mat-card-header>
            <mat-card-title><mat-icon>category</mat-icon> Usage by Feature</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="summary.byFeature" class="usage-table">
              <ng-container matColumnDef="featureType">
                <th mat-header-cell *matHeaderCellDef>Feature</th>
                <td mat-cell *matCellDef="let row">{{ row.featureType }}</td>
              </ng-container>
              <ng-container matColumnDef="requests">
                <th mat-header-cell *matHeaderCellDef>Requests</th>
                <td mat-cell *matCellDef="let row">{{ row.requests | number }}</td>
              </ng-container>
              <ng-container matColumnDef="tokens">
                <th mat-header-cell *matHeaderCellDef>Tokens</th>
                <td mat-cell *matCellDef="let row">{{ row.tokens | number }}</td>
              </ng-container>
              <ng-container matColumnDef="cost">
                <th mat-header-cell *matHeaderCellDef>Cost</th>
                <td mat-cell *matCellDef="let row">\${{ row.cost | number:'1.2-4' }}</td>
              </ng-container>
              <ng-container matColumnDef="avgResponseTimeMs">
                <th mat-header-cell *matHeaderCellDef>Avg Time</th>
                <td mat-cell *matCellDef="let row">{{ row.avgResponseTimeMs | number:'1.0-0' }}ms</td>
              </ng-container>
              <ng-container matColumnDef="successRate">
                <th mat-header-cell *matHeaderCellDef>Success</th>
                <td mat-cell *matCellDef="let row">{{ row.successRate | number:'1.0-0' }}%</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="featureCols"></tr>
              <tr mat-row *matRowDef="let row; columns: featureCols;"></tr>
            </table>
          </mat-card-content>
        </mat-card>

        <!-- Top grants -->
        <mat-card class="section-card" *ngIf="summary.topGrants.length">
          <mat-card-header>
            <mat-card-title><mat-icon>workspace_premium</mat-icon> Top Grants by Usage</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="summary.topGrants" class="usage-table">
              <ng-container matColumnDef="grantNumber">
                <th mat-header-cell *matHeaderCellDef>Grant</th>
                <td mat-cell *matCellDef="let row">{{ row.grantNumber }}</td>
              </ng-container>
              <ng-container matColumnDef="requests">
                <th mat-header-cell *matHeaderCellDef>Requests</th>
                <td mat-cell *matCellDef="let row">{{ row.requests | number }}</td>
              </ng-container>
              <ng-container matColumnDef="tokens">
                <th mat-header-cell *matHeaderCellDef>Tokens</th>
                <td mat-cell *matCellDef="let row">{{ row.tokens | number }}</td>
              </ng-container>
              <ng-container matColumnDef="cost">
                <th mat-header-cell *matHeaderCellDef>Cost</th>
                <td mat-cell *matCellDef="let row">\${{ row.cost | number:'1.2-4' }}</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="grantCols"></tr>
              <tr mat-row *matRowDef="let row; columns: grantCols;"></tr>
            </table>
          </mat-card-content>
        </mat-card>

        <!-- Daily trend (inline bar chart using CSS) -->
        <mat-card class="section-card" *ngIf="summary.byDay.length">
          <mat-card-header>
            <mat-card-title><mat-icon>bar_chart</mat-icon> Daily Requests</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="bar-chart">
              <div class="bar-wrap" *ngFor="let d of summary.byDay"
                   [matTooltip]="d.date + ': ' + d.requests + ' requests, ' + d.tokens + ' tokens'">
                <div class="bar" [style.height.%]="barHeight(d.requests)"></div>
                <div class="bar-label" *ngIf="summary.byDay.length <= 14">{{ d.date | slice:5 }}</div>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

      </ng-container>
    </div>
  `,
  styles: [`
    .usage-dashboard { max-width: 1100px; margin: 0 auto; padding: 24px; }
    .page-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 24px; }
    .page-title { display: flex; align-items: center; gap: 8px; font-size: 1.6rem; margin: 0; }
    .controls { display: flex; align-items: center; gap: 8px; }
    .days-select { width: 140px; }

    .stat-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 16px; margin-bottom: 24px; }
    .stat-card mat-card-content { display: flex; flex-direction: column; align-items: center; padding: 20px 16px; }
    .stat-icon { width: 48px; height: 48px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-bottom: 12px; }
    .stat-icon.primary { background: #e3f2fd; color: #1565c0; }
    .stat-icon.accent { background: #f3e5f5; color: #7b1fa2; }
    .stat-icon.warn { background: #fff3e0; color: #e65100; }
    .stat-icon.success { background: #e8f5e9; color: #2e7d32; }
    .stat-value { font-size: 1.8rem; font-weight: 600; }
    .stat-label { color: #666; font-size: 0.85rem; margin-top: 2px; }
    .stat-sub { color: #999; font-size: 0.78rem; margin-top: 4px; }

    .section-card { margin-bottom: 24px; }
    .section-card mat-card-title { display: flex; align-items: center; gap: 8px; }
    .usage-table { width: 100%; }

    .bar-chart { display: flex; align-items: flex-end; gap: 3px; height: 120px; padding: 8px 0; overflow-x: auto; }
    .bar-wrap { display: flex; flex-direction: column; align-items: center; flex: 1; min-width: 8px; }
    .bar { width: 100%; min-height: 2px; background: #1565c0; border-radius: 2px 2px 0 0; transition: height 0.3s; }
    .bar-label { font-size: 0.65rem; color: #999; margin-top: 2px; transform: rotate(-45deg); white-space: nowrap; }

    .loading-center { display: flex; flex-direction: column; align-items: center; padding: 60px; gap: 16px; }
    .error-banner { display: flex; align-items: center; gap: 8px; padding: 12px 16px; background: #fdecea; color: #c62828; border-radius: 4px; margin-bottom: 16px; }
  `]
})
export class UsageDashboardComponent implements OnInit {
  private http = inject(HttpClient);

  summary: UsageSummary | null = null;
  loading = false;
  error: string | null = null;
  selectedDays = 30;

  featureCols = ['featureType', 'requests', 'tokens', 'cost', 'avgResponseTimeMs', 'successRate'];
  grantCols = ['grantNumber', 'requests', 'tokens', 'cost'];

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.error = null;
    this.http.get<UsageSummary>(`${environment.apiUrl}/admin/usage?days=${this.selectedDays}`).subscribe({
      next: s => { this.summary = s; this.loading = false; },
      error: () => { this.error = 'Failed to load usage data.'; this.loading = false; }
    });
  }

  barHeight(requests: number): number {
    const max = Math.max(...(this.summary?.byDay.map(d => d.requests) ?? [1]));
    return max === 0 ? 0 : (requests / max) * 100;
  }
}
