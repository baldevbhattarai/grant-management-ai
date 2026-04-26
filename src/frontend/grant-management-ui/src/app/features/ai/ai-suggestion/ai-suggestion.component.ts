import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { AiService } from '../../../core/services/ai.service';

interface DiffToken { text: string; type: 'same' | 'added' | 'removed'; }

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

        <!-- Tone + word count controls -->
        <div class="controls-row">
          <div class="control-group">
            <span class="control-label">Words:</span>
            <button *ngFor="let w of wordCountOptions" class="ctrl-btn"
              [class.active]="selectedWordCount === w" (click)="selectedWordCount = w" type="button">
              {{w}}
            </button>
          </div>
          <div class="control-group">
            <span class="control-label">Tone:</span>
            <button *ngFor="let t of toneOptions" class="ctrl-btn"
              [class.active]="selectedTone === t" (click)="selectedTone = t" type="button">
              {{t}}
            </button>
          </div>
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
            <span class="quality-stars" *ngIf="qualityScore" [title]="'Quality score: ' + qualityScore + '/5'">
              <mat-icon *ngFor="let s of [1,2,3,4,5]" class="star-icon"
                [class.filled]="s <= (qualityScore || 0)">star</mat-icon>
            </span>
            <span class="meta">{{ tokensUsed ? (tokensUsed + ' tokens · ~$' + (cost | number:'1.4-4')) : '' }}</span>
          </div>
          <textarea class="suggestion-text" [(ngModel)]="suggestion" rows="8"></textarea>
          <!-- Diff view -->
          <div *ngIf="showDiff && diffTokens.length > 0" class="diff-view">
            <div class="diff-legend">
              <span class="diff-added-legend">+ added</span>
              <span class="diff-removed-legend">- removed</span>
            </div>
            <div class="diff-content">
              <span *ngFor="let t of diffTokens"
                [class.diff-added]="t.type === 'added'"
                [class.diff-removed]="t.type === 'removed'">{{ t.text }}</span>
            </div>
          </div>

          <!-- Refinement instruction for regeneration -->
          <div class="refinement-row">
            <input class="refinement-input" [(ngModel)]="regenerationFeedback"
              placeholder="Refinement hint, e.g. 'make it shorter' or 'focus on telehealth outcomes'…"
              maxlength="200" />
          </div>
          <div class="result-actions">
            <button mat-stroked-button color="primary" class="action-btn" (click)="accept()">
              <mat-icon>check</mat-icon> Accept
            </button>
            <button *ngIf="existingContent" mat-stroked-button class="action-btn diff-btn"
              (click)="toggleDiff()" type="button">
              <mat-icon>compare_arrows</mat-icon> {{ showDiff ? 'Hide diff' : 'Show diff' }}
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
    .controls-row {
      display: flex; gap: 16px; padding: 6px 14px; background: #faf5ff;
      border-bottom: 1px solid #ede7f6; flex-wrap: wrap;
    }
    .control-group { display: flex; align-items: center; gap: 4px; }
    .control-label { font-size: 0.72rem; color: #888; margin-right: 2px; }
    .ctrl-btn {
      border: 1px solid #d7b8e8; border-radius: 4px; background: white;
      padding: 2px 7px; font-size: 0.75rem; cursor: pointer; color: #666;
      transition: all 0.15s;
    }
    .ctrl-btn:hover { background: #f3e5f5; }
    .ctrl-btn.active { background: #9c27b0; color: white; border-color: #9c27b0; font-weight: 600; }

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
    .meta { font-size: 0.74rem; color: #ab87c4; flex: 1; text-align: right; }
    .quality-stars { display: flex; align-items: center; gap: 1px; }
    .star-icon { font-size: 14px; width: 14px; height: 14px; color: #ddd; }
    .star-icon.filled { color: #f59e0b; }
    .suggestion-text {
      width: 100%; box-sizing: border-box; border: 1px solid #e1bee7;
      border-radius: 6px; padding: 10px; font-size: 0.88rem;
      font-family: inherit; resize: vertical; background: #fff;
    }
    .suggestion-text:focus { outline: none; border-color: #9c27b0; }
    .diff-view { margin-top: 8px; border: 1px solid #e0e0e0; border-radius: 6px; padding: 10px; background: #fafafa; }
    .diff-legend { display: flex; gap: 12px; font-size: 0.72rem; margin-bottom: 6px; }
    .diff-added-legend { color: #16a34a; font-weight: 600; }
    .diff-removed-legend { color: #dc2626; font-weight: 600; text-decoration: line-through; }
    .diff-content { font-size: 0.85rem; line-height: 1.6; white-space: pre-wrap; }
    .diff-added { background: #dcfce7; color: #166534; border-radius: 2px; }
    .diff-removed { background: #fee2e2; color: #991b1b; text-decoration: line-through; border-radius: 2px; }
    .diff-btn { color: #6a1b9a; border-color: #ce93d8; }
    .refinement-row { margin-top: 8px; }
    .refinement-input {
      width: 100%; box-sizing: border-box; border: 1px solid #e1bee7; border-radius: 6px;
      padding: 6px 10px; font-size: 0.82rem; font-family: inherit; color: #555;
    }
    .refinement-input:focus { outline: none; border-color: #9c27b0; }
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
  @Input() existingContent: string = '';
  @Output() suggestionAccepted = new EventEmitter<string>();

  private aiService = inject(AiService);
  private snackBar = inject(MatSnackBar);

  loading = false;
  suggestion: string | null = null;
  keyPoints = '';
  showKeyPoints = false;
  regenerationFeedback = '';
  selectedWordCount = 150;
  selectedTone = 'Professional';
  wordCountOptions = [100, 150, 200, 250];
  toneOptions = ['Professional', 'Concise', 'Detailed'];
  tokensUsed = 0;
  cost = 0;
  qualityScore: number | null = null;
  lastLogId: string | null = null;
  showDiff = false;
  diffTokens: DiffToken[] = [];

  getSuggestion() {
    this.loading = true;
    this.suggestion = '';

    const req = {
      reportId: this.reportId,
      sectionName: this.sectionName,
      userId: this.userId || null,
      keyPoints: this.keyPoints.trim() || null,
      regenerationFeedback: this.regenerationFeedback.trim() || null,
      wordCount: this.selectedWordCount,
      tone: this.selectedTone
    };

    (async () => {
      try {
        for await (const token of this.aiService.streamSuggestion(req)) {
          if (token.startsWith('[META:') && token.endsWith(']')) {
            try {
              const meta = JSON.parse(token.slice('[META:'.length, -1));
              this.lastLogId = meta.logId ?? null;
              this.qualityScore = meta.qualityScore ?? null;
            } catch { /* ignore malformed meta */ }
            continue;
          }
          this.suggestion = (this.suggestion ?? '') + token;
        }
      } catch {
        this.snackBar.open('Error contacting AI service.', 'Close', { duration: 6000 });
        this.suggestion = null;
      } finally {
        this.loading = false;
      }
    })();
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
    this.showDiff = false;
  }

  toggleDiff() {
    this.showDiff = !this.showDiff;
    if (this.showDiff && this.diffTokens.length === 0)
      this.diffTokens = this.computeDiff(this.existingContent, this.suggestion ?? '');
  }

  // Simple word-level diff using LCS
  private computeDiff(oldText: string, newText: string): DiffToken[] {
    const oldWords = oldText.split(/(\s+)/);
    const newWords = newText.split(/(\s+)/);
    const m = oldWords.length, n = newWords.length;
    const dp: number[][] = Array.from({ length: m + 1 }, () => new Array(n + 1).fill(0));
    for (let i = 1; i <= m; i++)
      for (let j = 1; j <= n; j++)
        dp[i][j] = oldWords[i-1] === newWords[j-1] ? dp[i-1][j-1] + 1 : Math.max(dp[i-1][j], dp[i][j-1]);

    const tokens: DiffToken[] = [];
    let i = m, j = n;
    while (i > 0 || j > 0) {
      if (i > 0 && j > 0 && oldWords[i-1] === newWords[j-1]) {
        tokens.unshift({ text: oldWords[i-1], type: 'same' }); i--; j--;
      } else if (j > 0 && (i === 0 || dp[i][j-1] >= dp[i-1][j])) {
        tokens.unshift({ text: newWords[j-1], type: 'added' }); j--;
      } else {
        tokens.unshift({ text: oldWords[i-1], type: 'removed' }); i--;
      }
    }
    return tokens;
  }
}
