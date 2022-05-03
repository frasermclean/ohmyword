import { Injectable } from '@angular/core';
import { BehaviorSubject, from, Subject } from 'rxjs';
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
import { RoundEnd } from '../models/round-end';
import { Store } from '@ngxs/store';
import { Hub } from '../game/game.state';

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

  private readonly roundEndSubject = new BehaviorSubject<RoundEnd | null>(null);
  public get roundEnd$() {
    return this.roundEndSubject.asObservable();
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

  constructor(
    private fingerprintService: FingerprintService,
    private soundService: SoundService,
    private store: Store
  ) {
    // register callback for connection closed error
    this.hubConnection.onclose((error) => {
      if (error) console.error(error);
      this.registeredSubject.next(false);
      this.store.dispatch(new Hub.Disconnected(error));
    });

    // register game callbacks
    this.hubConnection.on('SendRoundStarted', (response) => this.onRoundStart(response));
    this.hubConnection.on('SendRoundEnded', (response) => this.onRoundEnd(response));
    this.hubConnection.on('SendLetterHint', (response: LetterHintResponse) => {
      this.letterHintSubject.next(new LetterHint(response));
    });
  }

  /**
   * Connect to game hub (if not already connected)
   */
  public connect = () => this.hubConnection.start();

  public disconnect = () => this.hubConnection.stop();

  /**
   * Attempt to register with game service.
   */
  async registerPlayer() {
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

  disconnectAndReset() {
    if (this.hubConnection.state == HubConnectionState.Connected) this.hubConnection.stop();
    this.playerId = '';
  }

  public async submitGuess() {
    // check for mismatched length
    if (this.guessSubject.value.length !== this.wordHintSubject.value.length) {
      this.soundService.play(SoundSprite.Incorrect);
      return;
    }

    const response = await this.hubConnection.invoke<GuessResponse>('SubmitGuess', {
      playerId: this.playerId,
      roundId: this.roundId,
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
   * Callback method to run upon round start
   * @param response The round start response
   */
  private onRoundStart(response: RoundStartResponse) {
    this.roundId = response.roundId;
    this.roundActiveSubject.next(true);
    this.wordHintSubject.next(new WordHint(response.wordHint));
    this.expirySubject.next(new Date(response.roundEnds));
    this.guess = '';
  }

  /**
   * Callback method to run upon round end
   * @param response The round end response
   */
  private onRoundEnd(response: RoundEndResponse) {
    this.roundActiveSubject.next(false);
    this.roundEndSubject.next(new RoundEnd(response));
    this.expirySubject.next(new Date(response.nextRoundStart));
  }
}
