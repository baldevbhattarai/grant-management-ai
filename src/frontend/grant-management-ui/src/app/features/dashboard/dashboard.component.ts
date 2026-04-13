import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatBadgeModule } from '@angular/material/badge';
import { GrantService } from '../../core/services/grant.service';
import { UserService, DemoUser } from '../../core/services/user.service';
import { SessionService } from '../../core/services/session.service';
import { Grant } from '../../core/models/grant.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, MatCardModule, MatButtonModule,
    MatIconModule, MatChipsModule, MatProgressSpinnerModule, MatBadgeModule
  ],
  template: `
    <div class="dashboard">
      <h1 class="page-title">
        <mat-icon>dashboard</mat-icon>
        My Grants
      </h1>

      <div *ngIf="loading" class="loading-center">
        <mat-spinner diameter="48"></mat-spinner>
        <p>Loading grants…</p>
      </div>

      <div *ngIf="error" class="error-banner">
        <mat-icon>error_outline</mat-icon> {{ error }}
      </div>

      <!-- User selector (demo) -->
      <div *ngIf="!loading && !error && demoUsers.length" class="user-selector">
        <span class="label">Demo user:</span>
        <mat-chip-set>
          <mat-chip *ngFor="let u of demoUsers" [class.selected]="selectedUserId === u.userId"
            (click)="selectUser(u)">{{ u.fullName }}</mat-chip>
        </mat-chip-set>
      </div>

      <div *ngIf="!loading" class="grants-grid">
        <mat-card *ngFor="let grant of grants" class="grant-card" appearance="outlined">
          <mat-card-header>
            <mat-card-title>{{ grant.grantNumber }}</mat-card-title>
            <mat-card-subtitle>{{ grant.grantType }} · {{ grant.programName }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="grant-meta">
              <span class="meta-row"><mat-icon inline>business</mat-icon> {{ grant.organizationName }}</span>
              <span class="meta-row"><mat-icon inline>person</mat-icon> {{ grant.userName }}</span>
              <span class="meta-row" *ngIf="grant.fundingAmount">
                <mat-icon inline>attach_money</mat-icon>
                {{ grant.fundingAmount | currency }}
              </span>
              <span class="meta-row" *ngIf="grant.focusAreas">
                <mat-icon inline>label</mat-icon> {{ grant.focusAreas }}
              </span>
            </div>
            <span class="status-chip" [class]="'status-' + grant.status.toLowerCase()">
              {{ grant.status }}
            </span>
          </mat-card-content>
          <mat-card-actions align="end">
            <button mat-flat-button color="primary" (click)="viewReports(grant)">
              <mat-icon>description</mat-icon> View Reports
            </button>
          </mat-card-actions>
        </mat-card>

        <mat-card *ngIf="!loading && grants.length === 0 && !error" class="empty-card" appearance="outlined">
          <mat-card-content>
            <mat-icon class="empty-icon">folder_open</mat-icon>
            <p>No grants found for this user.</p>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard { max-width: 1100px; margin: 0 auto; }
    .page-title { display: flex; align-items: center; gap: 8px; font-size: 1.6rem; margin-bottom: 20px; color: #1565c0; }
    .loading-center { display: flex; flex-direction: column; align-items: center; padding: 60px; gap: 16px; color: #666; }
    .error-banner { background: #ffebee; border: 1px solid #ef9a9a; border-radius: 8px; padding: 16px; display: flex; align-items: center; gap: 8px; color: #c62828; }
    .user-selector { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; flex-wrap: wrap; }
    .user-selector .label { font-size: 0.85rem; color: #666; }
    mat-chip { cursor: pointer; }
    mat-chip.selected { background: #1565c0 !important; color: white !important; }
    .grants-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 20px; }
    .grant-card { transition: box-shadow 0.2s; }
    .grant-card:hover { box-shadow: 0 4px 20px rgba(0,0,0,0.12); }
    .grant-meta { display: flex; flex-direction: column; gap: 6px; margin: 12px 0; }
    .meta-row { display: flex; align-items: center; gap: 6px; font-size: 0.88rem; color: #555; }
    .status-chip { display: inline-block; padding: 2px 10px; border-radius: 12px; font-size: 0.78rem; font-weight: 600; }
    .status-active { background: #e8f5e9; color: #2e7d32; }
    .status-closed { background: #fce4ec; color: #880e4f; }
    .empty-card { text-align: center; padding: 40px; }
    .empty-icon { font-size: 48px; width: 48px; height: 48px; color: #ccc; }
  `]
})
export class DashboardComponent implements OnInit {
  private grantService = inject(GrantService);
  private userService = inject(UserService);
  private session = inject(SessionService);
  private router = inject(Router);

  grants: Grant[] = [];
  loading = true;
  error = '';
  selectedUserId = '';
  demoUsers: DemoUser[] = [];

  ngOnInit() {
    this.userService.getAll().subscribe({
      next: users => {
        this.demoUsers = users;
        if (users.length > 0) {
          this.selectUser(users[0]);
        } else {
          this.loading = false;
        }
      },
      error: err => {
        this.error = err.status === 0
          ? 'Cannot reach API — make sure the .NET API is running on localhost:5266'
          : `Error loading users: ${err.statusText}`;
        this.loading = false;
      }
    });
  }

  selectUser(user: DemoUser) {
    this.selectedUserId = user.userId;
    this.session.userId = user.userId;
    this.loadGrants(user.userId);
  }

  loadGrants(userId: string) {
    this.loading = true;
    this.error = '';
    this.grants = [];
    this.grantService.getByUser(userId).subscribe({
      next: grants => { this.grants = grants; this.loading = false; },
      error: err => {
        this.error = err.status === 0
          ? 'Cannot reach API — make sure the .NET API is running on localhost:5266'
          : `Error loading grants: ${err.statusText}`;
        this.loading = false;
      }
    });
  }

  viewReports(grant: Grant) {
    this.router.navigate(['/grants', grant.grantId, 'reports']);
  }
}
