import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
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

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private playerId = '';
  private hubConnection = new HubConnectionBuilder()
    .withUrl(environment.api.hubUrl)
    .configureLogging(
      environment.production ? LogLevel.Error : LogLevel.Information
    )
    .build();

  private readonly hintSubject = new Subject<HintResponse>();
  public get hint$() {
    return this.hintSubject.asObservable();
  }

  private readonly isRegisteredSubject = new BehaviorSubject<boolean>(false);
  public get isRegistered$() {
    return this.isRegisteredSubject.asObservable();
  }

  constructor() {}

  /**
   * Attempt to register with game service.
   * @returns
   */
  async register() {
    try {
      await this.initialize();
      const visitorId = await this.getVisitorId();
      const response = await this.hubConnection.invoke<RegisterResponse>(
        'register',
        visitorId
      );
      this.playerId = response.playerId;
      this.isRegisteredSubject.next(response.successful);
      return response.successful;
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
      clientId: this.playerId,
    };
    const response = await this.hubConnection.invoke<HintResponse>(
      'getHint',
      args
    );
    this.hintSubject.next(response);
    return response;
  }

  public async guessWord(value: string) {
    await this.initialize();
    const args = {
      playerId: this.playerId,
      value,
    };
    const response = await this.hubConnection.invoke<GuessResponse>(
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
    if (this.hubConnection.state === HubConnectionState.Disconnected) {
      // server sends us a hint
      this.hubConnection.on('sendHint', (hint: HintResponse) => {
        this.hintSubject.next(hint);
      });

      await this.hubConnection.start();
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
