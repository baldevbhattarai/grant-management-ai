import { Component, inject, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ChatService } from '../../../core/services/chat.service';
import { ChatMessage } from '../../../core/models/ai.model';
import { GrantService } from '../../../core/services/grant.service';
import { UserService } from '../../../core/services/user.service';
import { SessionService } from '../../../core/services/session.service';

@Component({
  selector: 'app-chat-widget',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatButtonModule, MatIconModule,
    MatInputModule, MatFormFieldModule, MatProgressSpinnerModule, MatTooltipModule
  ],
  template: `
    <!-- Floating FAB -->
    <button mat-fab class="chat-fab" color="accent"
      matTooltip="Ask AI about your grants"
      (click)="togglePanel()">
      <mat-icon>{{ open ? 'close' : 'smart_toy' }}</mat-icon>
    </button>

    <!-- Chat panel -->
    <div class="chat-panel" [class.visible]="open">
      <div class="chat-header">
        <mat-icon>smart_toy</mat-icon>
        <span>Grant Q&amp;A Assistant</span>
        <button mat-icon-button class="close-btn" (click)="togglePanel()">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <!-- Grant selector -->
      <div class="grant-selector" *ngIf="grants.length > 0">
        <label>Grant:</label>
        <select [(ngModel)]="selectedGrantId" (change)="clearChat()">
          <option *ngFor="let g of grants" [value]="g.grantId">{{ g.grantNumber }}</option>
        </select>
      </div>

      <!-- Messages -->
      <div class="messages" #messagesContainer>
        <div class="welcome-msg" *ngIf="messages.length === 0">
          <mat-icon>auto_awesome</mat-icon>
          <p>Ask me anything about your grant reports!</p>
          <div class="suggestions">
            <button *ngFor="let s of sampleQuestions" mat-stroked-button (click)="askSample(s)">{{ s }}</button>
          </div>
        </div>

        <div *ngFor="let msg of messages" class="message" [class.user]="msg.role === 'user'" [class.assistant]="msg.role === 'assistant'">
          <div class="bubble">
            <div class="content">{{ msg.content }}</div>

            <!-- Sources -->
            <div *ngIf="msg.sources && msg.sources.length > 0" class="sources">
              <div class="sources-label">Sources:</div>
              <div *ngFor="let src of msg.sources" class="source-item">
                <mat-icon inline>article</mat-icon>
                <strong>{{ src.reportPeriod }}</strong> · {{ src.sectionName }}
                <div class="snippet">{{ src.snippet }}</div>
              </div>
            </div>
          </div>
          <div class="timestamp">{{ msg.timestamp | date:'shortTime' }}</div>
        </div>

        <!-- Typing indicator -->
        <div *ngIf="loading" class="message assistant">
          <div class="bubble typing">
            <mat-spinner diameter="20"></mat-spinner>
            <span>Thinking…</span>
          </div>
        </div>
      </div>

      <!-- Input -->
      <div class="chat-input">
        <input #inputEl type="text" [(ngModel)]="currentQuestion"
          placeholder="Ask a question about your grants…"
          (keydown.enter)="send()"
          [disabled]="loading" />
        <button mat-icon-button color="primary" (click)="send()"
          [disabled]="!currentQuestion.trim() || loading">
          <mat-icon>send</mat-icon>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .chat-fab {
      position: fixed; bottom: 28px; right: 28px; z-index: 1000;
      box-shadow: 0 4px 16px rgba(0,0,0,0.25);
    }
    .chat-panel {
      position: fixed; bottom: 100px; right: 28px; z-index: 999;
      width: 380px; height: 560px; max-height: 80vh;
      background: white; border-radius: 16px;
      box-shadow: 0 8px 40px rgba(0,0,0,0.18);
      display: flex; flex-direction: column;
      transform: scale(0.9) translateY(20px);
      opacity: 0; pointer-events: none;
      transition: all 0.25s cubic-bezier(0.34,1.56,0.64,1);
    }
    .chat-panel.visible {
      transform: scale(1) translateY(0); opacity: 1; pointer-events: all;
    }
    .chat-header {
      background: #6a1b9a; color: white;
      padding: 14px 16px; border-radius: 16px 16px 0 0;
      display: flex; align-items: center; gap: 8px;
      font-weight: 600; font-size: 0.95rem;
    }
    .close-btn { margin-left: auto; color: white; }
    .grant-selector {
      padding: 8px 12px; background: #f3e5f5;
      display: flex; align-items: center; gap: 8px;
      font-size: 0.82rem; color: #555; border-bottom: 1px solid #e0e0e0;
    }
    .grant-selector select { border: 1px solid #ccc; border-radius: 4px; padding: 2px 6px; font-size: 0.82rem; }
    .messages {
      flex: 1; overflow-y: auto; padding: 12px;
      display: flex; flex-direction: column; gap: 10px;
    }
    .welcome-msg {
      text-align: center; padding: 20px 10px; color: #888;
    }
    .welcome-msg mat-icon { font-size: 40px; width: 40px; height: 40px; color: #ce93d8; }
    .suggestions { display: flex; flex-direction: column; gap: 6px; margin-top: 12px; }
    .suggestions button { font-size: 0.78rem; white-space: normal; text-align: left; }
    .message { display: flex; flex-direction: column; }
    .message.user { align-items: flex-end; }
    .message.assistant { align-items: flex-start; }
    .bubble {
      max-width: 90%; padding: 10px 14px; border-radius: 14px;
      font-size: 0.88rem; line-height: 1.5;
    }
    .message.user .bubble { background: #1565c0; color: white; border-radius: 14px 14px 4px 14px; }
    .message.assistant .bubble { background: #f5f5f5; color: #222; border-radius: 14px 14px 14px 4px; }
    .timestamp { font-size: 0.7rem; color: #aaa; margin-top: 2px; padding: 0 4px; }
    .typing { display: flex; align-items: center; gap: 8px; }
    .sources { margin-top: 10px; padding-top: 8px; border-top: 1px solid #ddd; }
    .sources-label { font-size: 0.72rem; color: #888; margin-bottom: 4px; }
    .source-item { font-size: 0.75rem; color: #555; margin-bottom: 6px; display: flex; flex-direction: column; gap: 2px; }
    .snippet { color: #777; font-style: italic; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }
    .chat-input {
      padding: 10px 12px; border-top: 1px solid #e0e0e0;
      display: flex; align-items: center; gap: 8px;
    }
    .chat-input input {
      flex: 1; border: 1px solid #ddd; border-radius: 20px;
      padding: 8px 14px; font-size: 0.88rem; outline: none;
    }
    .chat-input input:focus { border-color: #6a1b9a; }
  `]
})
export class ChatWidgetComponent implements AfterViewChecked {
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  private chatService = inject(ChatService);
  private grantService = inject(GrantService);
  private userService = inject(UserService);
  private session = inject(SessionService);

  open = false;
  loading = false;
  currentQuestion = '';
  messages: ChatMessage[] = [];
  conversationId: string | undefined;
  selectedGrantId = '';
  grants: { grantId: string; grantNumber: string }[] = [];

  sampleQuestions = [
    'What did I write about telehealth last quarter?',
    'How many patients did I serve in 2024?',
    'What challenges did I face in Q1 2024?'
  ];

  private scrolled = false;

  togglePanel() {
    this.open = !this.open;
    if (this.open && this.grants.length === 0) this.loadGrants();
  }

  loadGrants() {
    // Use session userId if already set (user selected from dashboard), else fetch first user
    const userId = this.session.userId;
    if (userId) {
      this.grantService.getByUser(userId).subscribe({
        next: grants => {
          this.grants = grants.map(g => ({ grantId: g.grantId, grantNumber: g.grantNumber }));
          if (grants.length > 0) this.selectedGrantId = grants[0].grantId;
        },
        error: () => {}
      });
    } else {
      this.userService.getAll().subscribe({
        next: users => {
          if (users.length === 0) return;
          const firstUser = users[0];
          this.session.userId = firstUser.userId;
          this.grantService.getByUser(firstUser.userId).subscribe({
            next: grants => {
              this.grants = grants.map(g => ({ grantId: g.grantId, grantNumber: g.grantNumber }));
              if (grants.length > 0) this.selectedGrantId = grants[0].grantId;
            },
            error: () => {}
          });
        },
        error: () => {}
      });
    }
  }

  clearChat() {
    this.messages = [];
    this.conversationId = undefined;
  }

  askSample(q: string) {
    this.currentQuestion = q;
    this.send();
  }

  send() {
    const question = this.currentQuestion.trim();
    if (!question || !this.selectedGrantId) return;

    this.messages.push({ role: 'user', content: question, timestamp: new Date() });
    this.currentQuestion = '';
    this.loading = true;
    this.scrolled = false;

    this.chatService.ask({
      userId: this.session.userId,
      grantId: this.selectedGrantId,
      question,
      conversationId: this.conversationId
    }).subscribe({
      next: res => {
        this.loading = false;
        this.conversationId = res.conversationId;
        this.messages.push({
          role: 'assistant',
          content: res.success ? (res.answer ?? '') : (res.errorMessage ?? 'Sorry, an error occurred.'),
          sources: res.sources,
          timestamp: new Date()
        });
        this.scrolled = false;
      },
      error: () => {
        this.loading = false;
        this.messages.push({
          role: 'assistant',
          content: 'Could not reach the AI service. Please ensure the API is running and OpenAI key is set.',
          timestamp: new Date()
        });
        this.scrolled = false;
      }
    });
  }

  ngAfterViewChecked() {
    if (!this.scrolled && this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
      this.scrolled = true;
    }
  }
}
