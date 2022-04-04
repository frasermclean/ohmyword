import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';

import { environment } from 'src/environments/environment';

import { HintResponse } from '../models/responses/hint.response';
import { RegisterResponse } from '../models/responses/register.response';
import { GuessResponse } from '../models/responses/guess.response';
import { RoundOverResponse } from '../models/responses/round-over.response';

import { Hint } from '../models/hint';

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

  private readonly hintSubject = new Subject<Hint>();
  public get hint$() {
    return this.hintSubject.asObservable();
  }

  private readonly registeredSubject = new BehaviorSubject<boolean>(false);
  public get registered$() {
    return this.registeredSubject.asObservable();
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
    const registered = !!this.playerId;
    this.registeredSubject.next(registered);

    // automatically get a hint about the current word
    if (registered) {
      await this.getHint();
    }
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
    this.hintSubject.next(new Hint(response));
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
      this.hubConnection.on('sendHint', (response: HintResponse) => {
        this.hintSubject.next(new Hint(response));
      });

      this.hubConnection.on('sendRoundOver', (response: RoundOverResponse) => {
        const nextRoundStart = new Date(response.nextRoundStart);
        console.log(nextRoundStart);
        this.hintSubject.next(undefined);
      });

      await this.hubConnection.start();
    }
  }
}
