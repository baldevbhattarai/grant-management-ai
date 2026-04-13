import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SessionService {
  private _userId = '';

  set userId(id: string) { this._userId = id; }
  get userId(): string { return this._userId; }
}
