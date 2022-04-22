import { Injectable } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';

import { environment } from 'src/environments/environment';

import { RegisterPlayerResponse } from '../models/responses/register-player.response';
import { GuessResponse } from '../models/responses/guess.response';

import { WordHint } from '../models/word-hint';

import { FingerprintService } from './fingerprint.service';
import { SoundService, SoundSprite } from './sound.service';
import { LetterHintResponse } from '../models/responses/letter-hint.response';
import { LetterHint } from '../models/letter-hint';
import { RoundStartResponse } from '../models/responses/round-start.response';
import { RoundEndResponse } from '../models/responses/round-end.response';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private playerId = '';
  private roundId = '';
  private hubConnection = new HubConnectionBuilder()
    .withUrl(environment.api.hubUrl)
    .configureLogging(environment.production ? LogLevel.Error : LogLevel.Information)
    .build();

  private readonly registeredSubject = new BehaviorSubject<boolean>(false);
  public get registered$() {
    return this.registeredSubject.asObservable();
  }

  private readonly roundActiveSubject = new BehaviorSubject<boolean>(false);
  public get roundActive$() {
    return this.roundActiveSubject.asObservable();
  }

  private readonly wordHintSubject = new BehaviorSubject<WordHint>(WordHint.default);
  public get wordHint$() {
    return this.wordHintSubject.asObservable();
  }

  private readonly letterHintSubject = new Subject<LetterHint>();
  public get letterHint$() {
    return this.letterHintSubject.asObservable();
  }

  private readonly expirySubject = new BehaviorSubject<Date>(new Date());
  public get expiry$() {
    return this.expirySubject.asObservable();
  }

  private guessSubject = new BehaviorSubject<string>('');
  public get guess$() {
    return this.guessSubject.asObservable();
  }

  public get guess() {
    return this.guessSubject.value;
  }

  public set guess(value: string) {
    if (value.length > this.wordHintSubject.value.length) return;
    this.guessSubject.next(value);
  }

  constructor(private fingerprintService: FingerprintService, private soundService: SoundService) {}

  /**
   * Attempt to register with game service.
   */
  async registerPlayer() {
    await this.initialize();
    const visitorId = await this.fingerprintService.getVisitorId();
    const response = await this.hubConnection.invoke<RegisterPlayerResponse>('RegisterPlayer', visitorId);

    this.playerId = response.playerId;
    this.registeredSubject.next(true);
    this.roundActiveSubject.next(response.roundActive);
    this.expirySubject.next(new Date(response.expiry));
    if (response.wordHint) {
      this.wordHintSubject.next(new WordHint(response.wordHint));
    }
  }

  public async submitGuess() {
    // check for mismatched length
    if (this.guessSubject.value.length !== this.wordHintSubject.value.length) {
      this.soundService.play(SoundSprite.Incorrect);
      return;
    }

    const response = await this.hubConnection.invoke<GuessResponse>('SubmitGuess', {
      playerId: this.playerId,
      value: this.guessSubject.value,
    });

    // play sound to indicate correct / incorrect
    const sprite = response.correct ? SoundSprite.Correct : SoundSprite.Incorrect;
    this.soundService.play(sprite);

    // reset guess
    this.guess = '';

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
      this.hubConnection.on('SendRoundStarted', (response: RoundStartResponse) => {
        this.roundId = response.roundId;
        this.roundActiveSubject.next(true);
        this.wordHintSubject.next(new WordHint(response.wordHint));
        this.expirySubject.next(new Date(response.roundEnds));
        this.guess = '';
      });
      this.hubConnection.on('SendRoundEnded', (response: RoundEndResponse) => {
        this.roundActiveSubject.next(false);
        this.expirySubject.next(new Date(response.nextRoundStart));
      });
      this.hubConnection.on('SendLetterHint', (response: LetterHintResponse) => {
        this.letterHintSubject.next(new LetterHint(response));
      });

      await this.hubConnection.start();
    }
  }
}
