import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { AiService } from '../../../core/services/ai.service';

@Component({
  selector: 'app-ai-suggestion',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule,
    MatDialogModule, MatProgressSpinnerModule, MatSnackBarModule
  ],
  template: `
    <div class="ai-suggestion-wrap">

      <!-- Key points input -->
      <div class="key-points-wrap">
        <label class="key-points-label">
          <mat-icon style="font-size:15px;vertical-align:middle;margin-right:4px">lightbulb</mat-icon>
          Key highlights for this period <span class="optional">(optional)</span>
        </label>
        <textarea class="key-points-input" [(ngModel)]="keyPoints" rows="3"
          placeholder="e.g. 90% improvement in health service delivery, extended to 3 new rural counties, telehealth adoption up 45%"
          maxlength="1000"></textarea>
        <div class="key-points-hint">{{ keyPoints.length }}/1000 — include notable achievements to personalise the suggestion</div>
      </div>

      <!-- Trigger button -->
      <button mat-stroked-button color="accent" class="suggest-btn"
        [disabled]="loading" (click)="getSuggestion()">
        <mat-spinner *ngIf="loading" diameter="16" style="display:inline-block;margin-right:6px"></mat-spinner>
        <mat-icon *ngIf="!loading">auto_awesome</mat-icon>
        {{ loading ? 'Generating…' : 'Get AI Suggestion' }}
      </button>

      <!-- Inline suggestion panel -->
      <div *ngIf="suggestion" class="suggestion-panel">
        <div class="panel-header">
          <mat-icon color="accent">auto_awesome</mat-icon>
          <span>AI Suggestion</span>
          <span class="meta">{{ tokensUsed }} tokens · ~\${{ cost | number:'1.4-4' }}</span>
        </div>

        <textarea class="suggestion-text" [(ngModel)]="suggestion" rows="8"></textarea>

        <div class="panel-actions">
          <button mat-flat-button color="primary" (click)="accept()">
            <mat-icon>check</mat-icon> Accept
          </button>
          <button mat-stroked-button color="accent" [disabled]="loading" (click)="getSuggestion()">
            <mat-spinner *ngIf="loading" diameter="14" style="display:inline-block;margin-right:4px"></mat-spinner>
            <mat-icon *ngIf="!loading">refresh</mat-icon>
            {{ loading ? 'Regenerating…' : 'Regenerate' }}
          </button>
          <button mat-stroked-button (click)="dismiss()">
            <mat-icon>close</mat-icon> Dismiss
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .ai-suggestion-wrap { margin-top: 6px; }
    .suggest-btn { font-size: 0.82rem; }

    .key-points-wrap { margin-bottom: 10px; }
    .key-points-label {
      display: block; font-size: 0.82rem; font-weight: 600;
      color: #555; margin-bottom: 4px;
    }
    .optional { font-weight: 400; color: #999; }
    .key-points-input {
      width: 100%; box-sizing: border-box; border: 1px solid #ccc;
      border-radius: 6px; padding: 8px 10px; font-size: 0.87rem;
      font-family: inherit; resize: vertical; background: #fafafa;
      transition: border-color 0.2s;
    }
    .key-points-input:focus { outline: none; border-color: #9c27b0; background: #fff; }
    .key-points-hint { font-size: 0.75rem; color: #aaa; margin-top: 3px; text-align: right; }

    .suggestion-panel {
      margin-top: 12px; border: 1px solid #ce93d8; border-radius: 8px;
      padding: 16px; background: #fce4ec10;
    }
    .panel-header {
      display: flex; align-items: center; gap: 8px;
      font-weight: 600; margin-bottom: 10px; color: #6a1b9a;
    }
    .meta { margin-left: auto; font-size: 0.78rem; color: #999; font-weight: 400; }
    .suggestion-text {
      width: 100%; box-sizing: border-box; border: 1px solid #ddd;
      border-radius: 6px; padding: 10px; font-size: 0.9rem;
      font-family: inherit; resize: vertical; background: #fff;
    }
    .panel-actions { display: flex; gap: 8px; margin-top: 10px; flex-wrap: wrap; }
  `]
})
export class AiSuggestionComponent {
  @Input() reportId!: string;
  @Input() sectionName!: string;
  @Input() userId: string = 'demo-user';
  @Output() suggestionAccepted = new EventEmitter<string>();

  private aiService = inject(AiService);
  private snackBar = inject(MatSnackBar);

  loading = false;
  suggestion: string | null = null;
  keyPoints = '';
  tokensUsed = 0;
  cost = 0;
  lastLogId: string | null = null;

  getSuggestion() {
    this.loading = true;
    this.suggestion = null;
    this.aiService.getSuggestion({
      reportId: this.reportId,
      sectionName: this.sectionName,
      userId: this.userId || null,
      keyPoints: this.keyPoints.trim() || null
    }).subscribe({
      next: res => {
        this.loading = false;
        if (res.success) {
          this.suggestion = res.suggestedText;
          this.tokensUsed = res.tokensUsed;
          this.cost = res.estimatedCost;
        } else {
          this.snackBar.open(res.errorMessage || 'AI service unavailable', 'Close', { duration: 5000 });
        }
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Error contacting AI service. Is the OpenAI key configured?', 'Close', { duration: 6000 });
      }
    });
  }

  accept() {
    if (this.suggestion) {
      this.suggestionAccepted.emit(this.suggestion);
      this.suggestion = null;
      this.snackBar.open('Suggestion accepted!', '', { duration: 2000 });
    }
  }

  dismiss() {
    this.suggestion = null;
  }
}
