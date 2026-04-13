import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReportService } from '../../core/services/report.service';
import { GrantService } from '../../core/services/grant.service';
import { Report } from '../../core/models/report.model';
import { Grant } from '../../core/models/grant.model';

@Component({
  selector: 'app-report-list',
  standalone: true,
  imports: [
    CommonModule, MatTableModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatCardModule, MatProgressSpinnerModule
  ],
  template: `
    <div class="report-list">
      <div class="breadcrumb">
        <button mat-button (click)="goBack()"><mat-icon>arrow_back</mat-icon> Dashboard</button>
        <span *ngIf="grant"> / {{ grant.grantNumber }}</span>
      </div>

      <h1 class="page-title" *ngIf="grant">
        <mat-icon>description</mat-icon>
        {{ grant.grantNumber }} — Reports
        <span class="subtitle">{{ grant.programName }}</span>
      </h1>

      <div *ngIf="loading" class="loading-center">
        <mat-spinner diameter="40"></mat-spinner>
      </div>

      <div *ngIf="!loading">
        <table mat-table [dataSource]="reports" class="reports-table" *ngIf="reports.length > 0">
          <ng-container matColumnDef="period">
            <th mat-header-cell *matHeaderCellDef>Period</th>
            <td mat-cell *matCellDef="let r">{{ r.reportingYear }} {{ r.reportingQuarter }}</td>
          </ng-container>
          <ng-container matColumnDef="type">
            <th mat-header-cell *matHeaderCellDef>Type</th>
            <td mat-cell *matCellDef="let r">{{ r.reportType }}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let r">
              <span class="status-chip" [class]="'status-' + r.status.toLowerCase()">
                {{ r.status }}
              </span>
            </td>
          </ng-container>
          <ng-container matColumnDef="approved">
            <th mat-header-cell *matHeaderCellDef>Approved</th>
            <td mat-cell *matCellDef="let r">
              {{ r.approvedDate ? (r.approvedDate | date:'mediumDate') : '—' }}
            </td>
          </ng-container>
          <ng-container matColumnDef="rating">
            <th mat-header-cell *matHeaderCellDef>Rating</th>
            <td mat-cell *matCellDef="let r">
              <span *ngIf="r.reviewerRating">{{ '★'.repeat(r.reviewerRating) }}</span>
              <span *ngIf="!r.reviewerRating" style="color:#ccc">—</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef></th>
            <td mat-cell *matCellDef="let r">
              <button mat-flat-button [color]="r.status === 'Draft' ? 'accent' : 'basic'"
                (click)="openReport(r)">
                <mat-icon>{{ r.status === 'Draft' ? 'edit' : 'visibility' }}</mat-icon>
                {{ r.status === 'Draft' ? 'Fill Report' : 'View' }}
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="columns"></tr>
          <tr mat-row *matRowDef="let row; columns: columns;" class="table-row"></tr>
        </table>

        <mat-card *ngIf="reports.length === 0" class="empty-card" appearance="outlined">
          <mat-card-content style="text-align:center; padding:40px;">
            <mat-icon style="font-size:48px;color:#ccc">folder_open</mat-icon>
            <p>No reports found.</p>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .report-list { max-width: 1000px; margin: 0 auto; }
    .breadcrumb { margin-bottom: 12px; color: #666; display: flex; align-items: center; }
    .page-title { display: flex; align-items: center; gap: 8px; font-size: 1.4rem; margin-bottom: 20px; color: #1565c0; }
    .subtitle { font-size: 1rem; color: #666; font-weight: 400; margin-left: 8px; }
    .loading-center { display: flex; justify-content: center; padding: 60px; }
    .reports-table { width: 100%; border-radius: 8px; overflow: hidden; }
    .table-row:hover { background: #f5f5f5; }
    .status-chip { padding: 3px 10px; border-radius: 12px; font-size: 0.78rem; font-weight: 600; }
    .status-draft { background: #fff3e0; color: #e65100; }
    .status-approved { background: #e8f5e9; color: #2e7d32; }
    .status-submitted { background: #e3f2fd; color: #1565c0; }
    .status-rejected { background: #fce4ec; color: #880e4f; }
  `]
})
export class ReportListComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private reportService = inject(ReportService);
  private grantService = inject(GrantService);

  reports: Report[] = [];
  grant: Grant | null = null;
  loading = true;
  columns = ['period', 'type', 'status', 'approved', 'rating', 'actions'];

  ngOnInit() {
    const grantId = this.route.snapshot.paramMap.get('grantId')!;
    this.grantService.getById(grantId).subscribe(g => this.grant = g);
    this.reportService.getByGrant(grantId).subscribe({
      next: r => { this.reports = r; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openReport(report: Report) {
    this.router.navigate(['/reports', report.reportId]);
  }

  goBack() { this.router.navigate(['/dashboard']); }
}
