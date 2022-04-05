import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';

import { environment } from 'src/environments/environment';

import { WordHintResponse } from '../models/responses/word-hint.response';
import { RegisterPlayerResponse } from '../models/responses/register-player.response';
import { GuessResponse } from '../models/responses/guess.response';
import { GameStatusResponse } from '../models/responses/game-status.response';

import { WordHint } from '../models/word-hint';

import { FingerprintService } from './fingerprint.service';
import { SoundService, SoundSprite } from './sound.service';
import { GameStatus } from '../models/game-status';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private playerId = '';
  private hubConnection = new HubConnectionBuilder()
    .withUrl(environment.api.hubUrl)
    .configureLogging(environment.production ? LogLevel.Error : LogLevel.Information)
    .build();

  private readonly registeredSubject = new BehaviorSubject<boolean>(false);
  public get registered$() {
    return this.registeredSubject.asObservable();
  }

  private roundActiveSubject = new BehaviorSubject<boolean>(false);
  public get roundActive$() {
    return this.roundActiveSubject.asObservable();
  }

  private readonly statusSubject = new BehaviorSubject<GameStatus>(GameStatus.default);
  public get status$() {
    return this.statusSubject.asObservable();
  }

  private readonly hintSubject = new BehaviorSubject<WordHint>(WordHint.default);
  public get hint$() {
    return this.hintSubject.asObservable();
  }

  constructor(private fingerprintService: FingerprintService, private soundService: SoundService) {}

  /**
   * Attempt to register with game service.
   * @returns
   */
  async registerPlayer() {
    await this.initialize();
    const visitorId = await this.fingerprintService.getVisitorId();
    const response = await this.hubConnection.invoke<RegisterPlayerResponse>('registerPlayer', visitorId);

    if (!response.success) throw new Error('Failed to register player');

    this.playerId = response.playerId;
    this.registeredSubject.next(true);
    const status = new GameStatus(response.status);
    this.statusSubject.next(status);

    return status;
  }

  /**
   * Get hint about the current word.
   * @returns
   */
  private async getGameStatus() {
    await this.initialize();

    const response = await this.hubConnection.invoke<GameStatusResponse>('getStatus', this.playerId);

    const status = new GameStatus(response);
    this.statusSubject.next(status);

    return status;
  }

  public async guessWord(value: string) {
    await this.initialize();
    const args = {
      playerId: this.playerId,
      value,
    };
    const response = await this.hubConnection.invoke<GuessResponse>(this.guessWord.name, args);

    // play sound to indicate correct / incorrect
    const sprite = response.correct ? SoundSprite.Correct : SoundSprite.Incorrect;
    this.soundService.play(sprite);

    return response;
  }

  /**
   * Initialize connection to game hub (if not already connected)
   */
  private async initialize() {
    if (this.hubConnection.state === HubConnectionState.Disconnected) {
      // server sends us a hint
      this.hubConnection.on('SendHint', (response: WordHintResponse) => {
        this.hintSubject.next(new WordHint(response));
      });

      this.hubConnection.on('SendGameStatus', (response: GameStatusResponse) => {
        this.statusSubject.next(new GameStatus(response));
      });

      // register callback for connection closed error
      this.hubConnection.onclose((error) => {
        console.error('Connection closed: ', error);
        this.registeredSubject.next(false);
      });


      this.hubConnection.on('SendRoundActive', (value: boolean) => this.roundActiveSubject.next(value));

      await this.hubConnection.start();
    }
  }
}
