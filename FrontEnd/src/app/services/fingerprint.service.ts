import { Injectable } from '@angular/core';
import FingerprintJS from '@fingerprintjs/fingerprintjs';

@Injectable({
  providedIn: 'root',
})
export class FingerprintService {
  /**
   * Get unique visitor ID from FingerprintJS
   * @returns A string of the vistor ID.
   */
  public async getVisitorId() {
    const agent = await FingerprintJS.load();
    const result = await agent.get();
    return result.visitorId;
  }
}
