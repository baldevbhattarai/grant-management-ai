import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChatRequest, ChatResponse } from '../models/ai.model';
import { environment } from '../../../environments/environment';

export interface ChatSessionSummary {
  sessionId: string;
  firstQuestion: string;
  summary: string | null;
  messageCount: number;
  startedAt: string;
  lastActivityAt: string;
}

export interface ChatMessage {
  role: string;
  content: string;
  createdDate: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  ask(req: ChatRequest): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${this.base}/ai/chat`, req);
  }

  getSessions(userId: string, grantId: string): Observable<ChatSessionSummary[]> {
    return this.http.get<ChatSessionSummary[]>(`${this.base}/ai/chat/sessions?userId=${userId}&grantId=${grantId}`);
  }

  getSessionHistory(sessionId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.base}/ai/chat/sessions/${sessionId}/history`);
  }

  // Returns an async generator that yields each token from the SSE stream.
  async *askStream(req: ChatRequest): AsyncGenerator<string> {
    const response = await fetch(`${this.base}/ai/chat/stream`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(req)
    });

    if (!response.ok || !response.body) return;

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n');
      buffer = lines.pop() ?? '';

      for (const line of lines) {
        if (!line.startsWith('data:')) continue;
        // Use slice(1) not trim() — tokens arrive space-prefixed; trim() removes word-separator spaces
        const data = line.slice('data:'.length + 1);
        if (data.trim() === '[DONE]') return;
        yield data.replace(/\\n/g, '\n');
      }
    }
  }
}
