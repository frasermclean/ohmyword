import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HintResponse } from '../models/hint.response';
import FingerprintJS from '@fingerprintjs/fingerprintjs';
import { RegisterResponse } from '../models/register.response';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private readonly baseUrl = 'api/game';
  private clientId = '';

  constructor(private httpClient: HttpClient) {}

  getHint() {
    const url = `${this.baseUrl}/hint`;
    return this.httpClient.get<HintResponse>(url);
  }

  /**
   * Attempt to register with game service.
   * @returns 
   */
  async register() {
    try {
      // get visitor id
      const visitorId = await this.getVisitorId();

      // register with game server
      const url = `${this.baseUrl}/register`;
      const body = {
        visitorId,
      };
      const response = await this.httpClient
        .post<RegisterResponse>(url, body)
        .toPromise();

      this.clientId = response.clientId;
      console.log(
        `Successfully registered with game server. Client ID: ${this.clientId}`
      );
      return true;
    } catch (error) {
      console.error(error);
      return false;
    }
  }

  /**
   * Get unique visitor ID from FingerprintJS
   * @returns A string of the vistor ID.
   */
  private async getVisitorId() {
    const agent = await FingerprintJS.load();
    const result = await agent.get();
    return result.visitorId;
  }
}
