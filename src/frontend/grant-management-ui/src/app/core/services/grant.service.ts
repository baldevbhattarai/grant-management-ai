import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Grant } from '../models/grant.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class GrantService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  getByUser(userId: string): Observable<Grant[]> {
    return this.http.get<Grant[]>(`${this.base}/grants/user/${userId}`);
  }

  getById(grantId: string): Observable<Grant> {
    return this.http.get<Grant>(`${this.base}/grants/${grantId}`);
  }
}
