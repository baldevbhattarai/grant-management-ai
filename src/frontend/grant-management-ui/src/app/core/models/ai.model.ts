export interface SuggestionRequest {
  reportId: string;
  sectionName: string;
  userId: string | null;
  keyPoints?: string | null;
}

export interface SuggestionResponse {
  success: boolean;
  suggestedText: string | null;
  errorMessage: string | null;
  tokensUsed: number;
  estimatedCost: number;
}

export interface FeedbackRequest {
  logId: string;
  userAction: 'Accepted' | 'Rejected' | 'Edited' | 'Regenerated';
  userRating?: number;
}

export interface ChatRequest {
  userId: string;
  grantId: string;
  question: string;
  conversationId?: string;
}

export interface ChatSource {
  reportPeriod: string;
  sectionName: string;
  snippet: string;
}

export interface ChatResponse {
  success: boolean;
  answer: string | null;
  conversationId: string;
  errorMessage: string | null;
  sources: ChatSource[];
  confidenceScore: number | null;
}

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
  sources?: ChatSource[];
  timestamp: Date;
}
