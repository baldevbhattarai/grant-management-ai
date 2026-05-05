import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UploadedDocument {
  documentId: string;
  fileName: string;
  contentType: string;
  fileSizeBytes: number;
  chunkCount: number;
  uploadedAt: string;
  isIndexed: boolean;
}

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/ai/documents`;

  getDocuments(grantId: string): Observable<UploadedDocument[]> {
    return this.http.get<UploadedDocument[]>(`${this.base}?grantId=${grantId}`);
  }

  uploadDocument(file: File, grantId: string, userId: string): Observable<UploadedDocument> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<UploadedDocument>(
      `${this.base}/upload?grantId=${grantId}&userId=${userId}`, form);
  }

  deleteDocument(documentId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${documentId}`);
  }
}
