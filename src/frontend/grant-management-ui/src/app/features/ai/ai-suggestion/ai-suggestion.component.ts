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
          <button mat-stroked-button color="accent" (click)="getSuggestion()">
            <mat-icon>refresh</mat-icon> Regenerate
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
  tokensUsed = 0;
  cost = 0;
  lastLogId: string | null = null;

  getSuggestion() {
    this.loading = true;
    this.suggestion = null;
    this.aiService.getSuggestion({
      reportId: this.reportId,
      sectionName: this.sectionName,
      userId: this.userId || null
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
