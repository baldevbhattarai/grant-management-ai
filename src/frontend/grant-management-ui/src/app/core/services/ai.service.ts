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
}
