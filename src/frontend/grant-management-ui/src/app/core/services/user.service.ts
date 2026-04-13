import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DemoUser {
  userId: string;
  fullName: string;
  organizationName: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  getAll(): Observable<DemoUser[]> {
    return this.http.get<DemoUser[]>(`${this.base}/users`);
  }
}
