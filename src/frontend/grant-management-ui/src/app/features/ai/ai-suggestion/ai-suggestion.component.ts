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

      <!-- Unified AI suggestion card -->
      <div class="ai-card">
        <!-- Card header -->
        <div class="ai-card-header">
          <mat-icon class="ai-icon">auto_awesome</mat-icon>
          <span class="ai-label">AI Suggestion</span>
          <button class="toggle-highlights" type="button" (click)="showKeyPoints = !showKeyPoints">
            <mat-icon>{{ showKeyPoints ? 'expand_less' : 'edit_note' }}</mat-icon>
            {{ showKeyPoints ? 'Hide highlights' : '+ Add key highlights' }}
          </button>
        </div>

        <!-- Collapsible key points -->
        <div class="key-points-body" *ngIf="showKeyPoints">
          <textarea class="key-points-input" [(ngModel)]="keyPoints" rows="3"
            placeholder="e.g. 90% improvement in health service delivery, extended to 3 new rural counties, telehealth adoption up 45%"
            maxlength="1000"></textarea>
          <div class="key-points-hint">{{ keyPoints.length }}/1000 · These highlights will be woven into the generated text</div>
        </div>

          <!-- Generated result — inside the same card -->
        <div *ngIf="suggestion" class="result-body">
          <div class="result-divider">
            <span class="meta">{{ tokensUsed }} tokens · ~\${{ cost | number:'1.4-4' }}</span>
          </div>
          <textarea class="suggestion-text" [(ngModel)]="suggestion" rows="8"></textarea>
          <div class="result-actions">
            <button mat-stroked-button color="primary" class="action-btn" (click)="accept()">
              <mat-icon>check</mat-icon> Accept
            </button>
            <button mat-button class="action-btn dismiss-btn" (click)="dismiss()">
              <mat-icon>close</mat-icon> Dismiss
            </button>
          </div>
        </div>

        <!-- Generate / Regenerate button -->
        <div class="generate-row">
          <button mat-stroked-button color="accent" class="generate-btn"
            [disabled]="loading" (click)="getSuggestion()">
            <mat-spinner *ngIf="loading" diameter="14" style="display:inline-block;margin-right:4px"></mat-spinner>
            <mat-icon *ngIf="!loading">{{ suggestion ? 'refresh' : 'auto_awesome' }}</mat-icon>
            {{ loading ? 'Generating…' : (suggestion ? 'Regenerate' : 'Get AI Suggestion') }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .ai-suggestion-wrap { margin-top: 8px; }

    /* Unified card */
    .ai-card {
      border: 1px solid #ce93d8; border-radius: 10px;
      overflow: hidden; background: #fdf6ff;
    }
    .ai-card-header {
      display: flex; align-items: center; gap: 8px;
      padding: 10px 14px; background: #f3e5f5;
      border-bottom: 1px solid #e1bee7;
    }
    .ai-icon { color: #9c27b0; font-size: 18px; }
    .ai-label { font-weight: 600; font-size: 0.88rem; color: #6a1b9a; flex: 1; }
    .toggle-highlights {
      display: flex; align-items: center; gap: 3px;
      background: none; border: none; cursor: pointer;
      font-size: 0.78rem; color: #9c27b0; font-weight: 500;
      padding: 2px 6px; border-radius: 4px; transition: background 0.15s;
    }
    .toggle-highlights:hover { background: #e1bee7; }
    .toggle-highlights mat-icon { font-size: 16px; height: 16px; width: 16px; }

    /* Key points collapsible body */
    .key-points-body { padding: 10px 14px 0; }
    .key-points-input {
      width: 100%; box-sizing: border-box;
      border: 1px solid #d7b8e8; border-radius: 6px;
      padding: 8px 10px; font-size: 0.87rem; font-family: inherit;
      resize: vertical; background: #fff; transition: border-color 0.2s;
    }
    .key-points-input:focus { outline: none; border-color: #9c27b0; }
    .key-points-hint { font-size: 0.74rem; color: #ab87c4; margin: 4px 0 2px; text-align: right; }

    /* Generate button */
    .generate-row {
      padding: 8px 14px 10px; display: flex; justify-content: flex-end;
    }
    .generate-btn {
      font-size: 0.8rem; height: 30px; line-height: 30px;
      padding: 0 12px;
    }
    .generate-btn mat-icon { font-size: 15px; height: 15px; width: 15px; margin-right: 4px; }

    /* Result — inside the card */
    .result-body { padding: 0 14px 4px; }
    .result-divider {
      display: flex; align-items: center; justify-content: flex-end;
      border-top: 1px dashed #e1bee7; padding: 8px 0 6px;
    }
    .meta { font-size: 0.74rem; color: #ab87c4; }
    .suggestion-text {
      width: 100%; box-sizing: border-box; border: 1px solid #e1bee7;
      border-radius: 6px; padding: 10px; font-size: 0.88rem;
      font-family: inherit; resize: vertical; background: #fff;
    }
    .suggestion-text:focus { outline: none; border-color: #9c27b0; }
    .result-actions { display: flex; gap: 6px; margin-top: 8px; flex-wrap: wrap; }
    .action-btn { font-size: 0.78rem; height: 28px; line-height: 28px; padding: 0 10px; }
    .action-btn mat-icon { font-size: 14px; height: 14px; width: 14px; margin-right: 3px; }
    .dismiss-btn { color: #999; }
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
  showKeyPoints = false;
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
          this.lastLogId = res.logId;
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
      const text = this.suggestion;
      this.suggestionAccepted.emit(text);

      // Send feedback with the accepted text so it can be promoted to the example pool
      if (this.lastLogId) {
        this.aiService.sendFeedback({
          logId: this.lastLogId,
          userAction: 'Accepted',
          userRating: 5,
          acceptedText: text
        }).subscribe();
      }

      this.suggestion = null;
      this.snackBar.open('Suggestion accepted and added to example pool!', '', { duration: 2500 });
    }
  }

  dismiss() {
    if (this.lastLogId) {
      this.aiService.sendFeedback({ logId: this.lastLogId, userAction: 'Rejected' }).subscribe();
    }
    this.suggestion = null;
  }
}
