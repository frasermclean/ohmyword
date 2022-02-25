import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HintResponse } from '../models/hint.response';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private readonly baseUrl = 'api/game';

  constructor(private httpClient: HttpClient) {}

  getHint() {
    const url = `${this.baseUrl}/hint`;
    return this.httpClient.get<HintResponse>(url);
  }
}
