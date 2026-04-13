import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Report, UpdateSectionRequest } from '../models/report.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  getByGrant(grantId: string): Observable<Report[]> {
    return this.http.get<Report[]>(`${this.base}/reports/grant/${grantId}`);
  }

  getById(reportId: string): Observable<Report> {
    return this.http.get<Report>(`${this.base}/reports/${reportId}`);
  }

  updateSection(sectionId: string, req: UpdateSectionRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/reports/sections/${sectionId}`, req);
  }
}
