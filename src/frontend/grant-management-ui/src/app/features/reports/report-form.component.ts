import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { ReportService } from '../../core/services/report.service';
import { SessionService } from '../../core/services/session.service';
import { Report, ReportSection } from '../../core/models/report.model';
import { AiSuggestionComponent } from '../ai/ai-suggestion/ai-suggestion.component';

@Component({
  selector: 'app-report-form',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDividerModule, MatChipsModule, AiSuggestionComponent
  ],
  template: `
    <div class="report-form">
      <div class="breadcrumb">
        <button mat-button (click)="goBack()"><mat-icon>arrow_back</mat-icon> Reports</button>
        <span *ngIf="report"> / {{ report.reportingYear }} {{ report.reportingQuarter }}</span>
      </div>

      <div *ngIf="loading" class="loading-center"><mat-spinner diameter="48"></mat-spinner></div>

      <div *ngIf="report && !loading">
        <!-- Report header -->
        <mat-card class="report-header-card" appearance="outlined">
          <mat-card-content>
            <div class="header-grid">
              <div><span class="field-label">Grant</span><span>{{ report.grantNumber }}</span></div>
              <div><span class="field-label">Period</span><span>{{ report.reportingYear }} {{ report.reportingQuarter }}</span></div>
              <div><span class="field-label">Type</span><span>{{ report.reportType }}</span></div>
              <div>
                <span class="field-label">Status</span>
                <span class="status-chip" [class]="'status-' + report.status.toLowerCase()">{{ report.status }}</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>

        <!-- Sections -->
        <div *ngFor="let section of report.sections; let i = index" class="section-card-wrap">
          <mat-card appearance="outlined" class="section-card">
            <mat-card-header>
              <mat-card-title>
                {{ section.sectionOrder }}. {{ section.sectionTitle }}
                <span *ngIf="section.isRequired" class="required-badge">Required</span>
              </mat-card-title>
              <mat-card-subtitle>{{ section.questionText }}</mat-card-subtitle>
            </mat-card-header>

            <mat-card-content>
              <!-- Text field -->
              <ng-container *ngIf="section.responseType === 'Text'">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Your response</mat-label>
                  <textarea matInput [(ngModel)]="sectionValues[section.sectionId]"
                    [rows]="6" [maxlength]="section.maxLength || 5000"
                    [placeholder]="'Enter up to ' + (section.maxLength || 5000) + ' characters'"
                    [disabled]="report.status === 'Approved'">
                  </textarea>
                  <mat-hint align="end">
                    {{ (sectionValues[section.sectionId] || '').length }} / {{ section.maxLength || 5000 }}
                  </mat-hint>
                </mat-form-field>

                <!-- AI Suggestion button (only for Draft) -->
                <app-ai-suggestion
                  *ngIf="report.status === 'Draft'"
                  [reportId]="report.reportId"
                  [sectionName]="section.sectionName"
                  [userId]="demoUserId"
                  (suggestionAccepted)="onSuggestionAccepted(section.sectionId, $event)">
                </app-ai-suggestion>
              </ng-container>

              <!-- Number field -->
              <ng-container *ngIf="section.responseType === 'Number'">
                <mat-form-field appearance="outline">
                  <mat-label>{{ section.sectionTitle }}</mat-label>
                  <input matInput type="number" [(ngModel)]="sectionValues[section.sectionId]"
                    [disabled]="report.status === 'Approved'" />
                </mat-form-field>
              </ng-container>

              <!-- MultiSelect -->
              <ng-container *ngIf="section.responseType === 'MultiSelect'">
                <p class="multi-hint">Select all that apply:</p>
                <mat-chip-set>
                  <mat-chip *ngFor="let opt of getMultiOptions(section)"
                    [class.selected-chip]="isSelected(section.sectionId, opt)"
                    (click)="toggleChip(section.sectionId, opt)"
                    [disabled]="report.status === 'Approved'">
                    {{ opt }}
                  </mat-chip>
                </mat-chip-set>
              </ng-container>

              <!-- Radio -->
              <ng-container *ngIf="section.responseType === 'Radio'">
                <p class="multi-hint">Select one:</p>
                <mat-chip-set>
                  <mat-chip *ngFor="let opt of radioOptions"
                    [class.selected-chip]="sectionValues[section.sectionId] === opt"
                    (click)="sectionValues[section.sectionId] = opt"
                    [disabled]="report.status === 'Approved'">
                    {{ opt }}
                  </mat-chip>
                </mat-chip-set>
              </ng-container>
            </mat-card-content>

            <!-- Save button per section (Draft only) -->
            <mat-card-actions align="end" *ngIf="report.status === 'Draft'">
              <button mat-stroked-button color="primary" [disabled]="savingSection === section.sectionId"
                (click)="saveSection(section)">
                <mat-spinner *ngIf="savingSection === section.sectionId" diameter="16"
                  style="display:inline-block;margin-right:6px"></mat-spinner>
                <mat-icon *ngIf="savingSection !== section.sectionId">save</mat-icon>
                {{ savingSection === section.sectionId ? 'Saving…' : 'Save' }}
              </button>
            </mat-card-actions>
          </mat-card>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .report-form { max-width: 860px; margin: 0 auto; }
    .breadcrumb { margin-bottom: 12px; display: flex; align-items: center; color: #666; }
    .loading-center { display:flex; justify-content:center; padding:60px; }
    .report-header-card { margin-bottom: 20px; }
    .header-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); gap: 16px; }
    .header-grid > div { display: flex; flex-direction: column; gap: 4px; }
    .field-label { font-size: 0.75rem; color: #888; text-transform: uppercase; letter-spacing: 0.05em; }
    .section-card-wrap { margin-bottom: 20px; }
    .section-card mat-card-title { font-size: 1rem; display: flex; align-items: center; gap: 8px; }
    .required-badge { font-size: 0.7rem; background: #ffebee; color: #c62828; padding: 1px 6px; border-radius: 4px; }
    .full-width { width: 100%; }
    .multi-hint { font-size: 0.85rem; color: #666; margin-bottom: 8px; }
    mat-chip { cursor: pointer; }
    .selected-chip { background: #1565c0 !important; color: white !important; }
    .status-chip { padding: 3px 10px; border-radius: 12px; font-size: 0.78rem; font-weight: 600; }
    .status-draft { background: #fff3e0; color: #e65100; }
    .status-approved { background: #e8f5e9; color: #2e7d32; }
  `]
})
export class ReportFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private reportService = inject(ReportService);
  private session = inject(SessionService);
  private snackBar = inject(MatSnackBar);

  report: Report | null = null;
  loading = true;
  savingSection: string | null = null;
  get demoUserId() { return this.session.userId; }

  // Flat map sectionId → current value (string, number, or comma-list)
  sectionValues: Record<string, any> = {};

  radioOptions = ['Yes - Fully Implemented', 'Yes - Partially', 'Planned', 'No'];
  multiOptions: Record<string, string[]> = {};

  ngOnInit() {
    const reportId = this.route.snapshot.paramMap.get('reportId')!;
    this.reportService.getById(reportId).subscribe({
      next: r => {
        this.report = r;
        // Seed sectionValues from existing data
        r.sections.forEach(s => {
          this.sectionValues[s.sectionId] = s.responseText ?? s.responseNumber ?? '';
        });
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onSuggestionAccepted(sectionId: string, text: string) {
    this.sectionValues[sectionId] = text;
  }

  saveSection(section: ReportSection) {
    this.savingSection = section.sectionId;
    const val = this.sectionValues[section.sectionId];
    const req = section.responseType === 'Number'
      ? { responseNumber: Number(val) }
      : section.responseType === 'MultiSelect'
        ? { responseOptions: JSON.stringify(this.getSelectedChips(section.sectionId)) }
        : section.responseType === 'Radio'
          ? { responseSingle: val }
          : { responseText: val as string };

    this.reportService.updateSection(section.sectionId, req).subscribe({
      next: () => {
        this.savingSection = null;
        this.snackBar.open('Saved!', '', { duration: 2000 });
      },
      error: () => {
        this.savingSection = null;
        this.snackBar.open('Save failed', 'Close', { duration: 4000 });
      }
    });
  }

  getMultiOptions(section: ReportSection): string[] {
    // Parse from the section's existing responseOptions or default set
    try {
      if (section.responseText) return JSON.parse(section.responseText);
    } catch {}
    return ['Primary Care', 'Dental Services', 'Behavioral Health', 'Enabling Services', 'Pharmacy', 'Laboratory'];
  }

  isSelected(sectionId: string, opt: string): boolean {
    const val = this.sectionValues[sectionId];
    if (!val) return false;
    const arr: string[] = typeof val === 'string' ? JSON.parse(val || '[]') : val;
    return arr.includes(opt);
  }

  toggleChip(sectionId: string, opt: string) {
    let arr: string[] = [];
    try { arr = JSON.parse(this.sectionValues[sectionId] || '[]'); } catch {}
    const idx = arr.indexOf(opt);
    if (idx >= 0) arr.splice(idx, 1); else arr.push(opt);
    this.sectionValues[sectionId] = JSON.stringify(arr);
  }

  getSelectedChips(sectionId: string): string[] {
    try { return JSON.parse(this.sectionValues[sectionId] || '[]'); } catch { return []; }
  }

  goBack() {
    if (this.report) this.router.navigate(['/grants', this.report.grantId, 'reports']);
    else this.router.navigate(['/dashboard']);
  }
}
