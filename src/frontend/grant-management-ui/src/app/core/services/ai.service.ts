import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SuggestionRequest, SuggestionResponse, FeedbackRequest } from '../models/ai.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AiService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  getSuggestion(req: SuggestionRequest): Observable<SuggestionResponse> {
    return this.http.post<SuggestionResponse>(`${this.base}/ai/suggestions`, req);
  }

  sendFeedback(req: FeedbackRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/ai/suggestions/feedback`, req);
  }

  async *streamSuggestion(req: SuggestionRequest): AsyncGenerator<string> {
    const response = await fetch(`${this.base}/ai/suggestions/stream`, {
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
        const data = line.slice('data:'.length).trim();
        if (data === '[DONE]') return;
        yield data.replace(/\\n/g, '\n');
      }
    }
  }
}
