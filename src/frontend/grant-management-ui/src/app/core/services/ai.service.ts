import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SuggestionRequest, SuggestionResponse, FeedbackRequest } from '../models/ai.model';
import { environment } from '../../../environments/environment';

export interface SectionDraft {
  sectionName: string;
  sectionTitle: string;
  draftedText: string | null;
  success: boolean;
  errorMessage: string | null;
  qualityScore: number | null;
  tokensUsed: number;
  estimatedCost: number;
}

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

  draftReport(reportId: string, userId: string): Observable<SectionDraft[]> {
    return this.http.post<SectionDraft[]>(
      `${this.base}/ai/suggestions/draft-report/${reportId}?userId=${userId}`, {});
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
        // Use slice(1) not trim() — tokens arrive space-prefixed; trim() removes word-separator spaces
        const data = line.slice('data:'.length + 1);
        if (data.trim() === '[DONE]') return;
        yield data.replace(/\\n/g, '\n');
      }
    }
  }
}
