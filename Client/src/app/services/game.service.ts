import { Injectable } from '@angular/core';

import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import FingerprintJS from '@fingerprintjs/fingerprintjs';

import { HintResponse } from '../models/hint.response';
import { RegisterResponse } from '../models/register.response';
import { environment } from 'src/environments/environment';
import { GuessResponse } from '../models/guess.response';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private clientId = '';
  private connection = new HubConnectionBuilder()
    .withUrl(environment.api.hubUrl)
    .configureLogging(
      environment.production ? LogLevel.Error : LogLevel.Information
    )
    .build();

  private hintSubject = new Subject<HintResponse>();
  public hint$ = this.hintSubject.asObservable();

  constructor() {}

  /**
   * Attempt to register with game service.
   * @returns
   */
  async register() {
    try {
      await this.initialize();
      const args = {
        visitorId: await this.getVisitorId(),
      };
      const response = await this.connection.invoke<RegisterResponse>(
        'register',
        args
      );
      this.clientId = response.clientId;
      return this.clientId !== '';
    } catch (error) {
      console.error(error);
      return false;
    }
  }

  /**
   * Get hint about the current word.
   * @returns
   */
  async getHint() {
    await this.initialize();
    const args = {
      clientId: this.clientId,
    };
    return await this.connection.invoke<HintResponse>('getHint', args);
  }

  public async guessWord(value: string) {
    await this.initialize();
    const args = {
      clientId: this.clientId,
      value,
    };
    const response = await this.connection.invoke<GuessResponse>(
      this.guessWord.name,
      args
    );
    console.log(response);
    return response;
  }

  /**
   * Initialize connection to game hub (if not already connected)
   */
  private async initialize() {
    if (this.connection.state === HubConnectionState.Disconnected) {
      // server sends us a hint
      this.connection.on('sendHint', (hint: HintResponse) => {
        this.hintSubject.next(hint);
      });

      await this.connection.start();
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
