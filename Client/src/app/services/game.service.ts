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

  private readonly gameStatusSubject = new BehaviorSubject<GameStatus>(GameStatus.default);
  public get gameStatus$() {
    return this.gameStatusSubject.asObservable();
  }

  private readonly wordHintSubject = new BehaviorSubject<WordHint>(WordHint.default);
  public get wordHint$() {
    return this.wordHintSubject.asObservable();
  }

  constructor(private fingerprintService: FingerprintService, private soundService: SoundService) {}

  /**
   * Attempt to register with game service.
   */
  async registerPlayer() {
    await this.initialize();
    const visitorId = await this.fingerprintService.getVisitorId();
    const response = await this.hubConnection.invoke<RegisterPlayerResponse>('registerPlayer', visitorId);

    this.playerId = response.playerId;
    this.registeredSubject.next(true);
    this.wordHintSubject.next(new WordHint(response.wordHint));
    this.gameStatusSubject.next(new GameStatus(response.gameStatus));
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
      // register callback for connection closed error
      this.hubConnection.onclose((error) => {
        console.error('Connection closed: ', error);
        this.registeredSubject.next(false);
      });

      // register game callbacks
      this.hubConnection.on('SendGameStatus', (response: GameStatusResponse) =>
        this.gameStatusSubject.next(new GameStatus(response))
      );
      this.hubConnection.on('SendWordHint', (value: WordHintResponse) =>
        this.wordHintSubject.next(new WordHint(value))
      );

      await this.hubConnection.start();
    }
  }
}
