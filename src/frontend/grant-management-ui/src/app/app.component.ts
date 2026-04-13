import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ChatWidgetComponent } from './features/ai/chat-widget/chat-widget.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, MatToolbarModule, MatButtonModule, MatIconModule, ChatWidgetComponent],
  template: `
    <mat-toolbar color="primary">
      <mat-icon>local_hospital</mat-icon>
      <span style="margin-left:8px; font-weight:600;">Grant Management AI</span>
      <span style="flex:1"></span>
      <a mat-button routerLink="/dashboard">
        <mat-icon>dashboard</mat-icon> Dashboard
      </a>
    </mat-toolbar>

    <main class="main-content">
      <router-outlet></router-outlet>
    </main>

    <!-- Chat widget always visible -->
    <app-chat-widget></app-chat-widget>
  `,
  styles: [`
    .main-content { padding: 24px; max-width: 1200px; margin: 0 auto; }
    mat-toolbar mat-icon { font-size: 28px; }
  `]
})
export class AppComponent {}
