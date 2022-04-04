import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';

import { HintResponse } from '../models/hint.response';
import { RegisterResponse } from '../models/register.response';
import { environment } from 'src/environments/environment';
import { GuessResponse } from '../models/guess.response';
import { FingerprintService } from './fingerprint.service';
import { SoundService, SoundSprite } from './sound.service';

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

  constructor(
    private fingerprintService: FingerprintService,
    private soundService: SoundService
  ) {}

  /**
   * Attempt to register with game service.
   * @returns
   */
  async registerPlayer() {
    await this.initialize();
    const visitorId = await this.fingerprintService.getVisitorId();
    const response = await this.hubConnection.invoke<RegisterResponse>(
      'registerPlayer',
      visitorId
    );
    this.playerId = response.playerId;
    this.isRegisteredSubject.next(!!this.playerId);
  }

  /**
   * Get hint about the current word.
   * @returns
   */
  async getHint() {
    await this.initialize();
    const args = {
      playerId: this.playerId,
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

    // play sound to indicate correct / incorrect
    const sprite = response.correct
      ? SoundSprite.Correct
      : SoundSprite.Incorrect;
    this.soundService.play(sprite);

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
}
