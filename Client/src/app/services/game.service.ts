import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';

import { environment } from 'src/environments/environment';

import { RegisterPlayerResponse } from '../models/responses/register-player.response';
import { GuessResponse } from '../models/responses/guess.response';

import { FingerprintService } from './fingerprint.service';
import { SoundService, SoundSprite } from './sound.service';
import { LetterHintResponse } from '../models/responses/letter-hint.response';
import { RoundStartResponse } from '../models/responses/round-start.response';
import { RoundEndResponse } from '../models/responses/round-end.response';
import { RoundEnd } from '../models/round-end';
import { Store } from '@ngxs/store';
import { Game } from '../game/game.state';
import { Hub } from '../game/hub.state';

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

  private readonly roundEndSubject = new BehaviorSubject<RoundEnd | null>(null);
  public get roundEnd$() {
    return this.roundEndSubject.asObservable();
  }  

  constructor(
    private fingerprintService: FingerprintService,
    private soundService: SoundService,
    private store: Store
  ) {
    // register callbacks
    this.hubConnection.onclose((error) => this.store.dispatch(new Hub.Disconnected(error)));
    this.hubConnection.on('SendRoundStarted', (response: RoundStartResponse) =>
      this.store.dispatch(new Game.RoundStarted(response))
    );
    this.hubConnection.on('SendRoundEnded', (response: RoundEndResponse) =>
      this.store.dispatch(new Game.RoundEnded(response))
    );
    this.hubConnection.on('SendLetterHint', (response: LetterHintResponse) => {
      this.store.dispatch(new Game.LetterHintReceived(response));
    });
  }

  /**
   * Connect to game hub (if not already connected)
   */
  public async connect() {
    if (this.hubConnection.state !== HubConnectionState.Disconnected) {
      console.warn('Invalid hub connection state to perform connect:', this.hubConnection.state);
      return;
    }

    try {
      await this.hubConnection.start();
      this.store.dispatch(new Hub.Connected());
    } catch (error) {
      this.store.dispatch(new Hub.ConnectFailed(error));
    }
  }

  /**
   * Disconnect from game hub
   */
  public disconnect() {
    if (this.hubConnection.state !== HubConnectionState.Connected) {
      console.warn('Invalid hub connection state to perform disconnect:', this.hubConnection.state);
      return;
    }

    this.hubConnection.stop();
  }

  /**
   * Attempt to register with game service.
   */
  public async registerPlayer() {
    const visitorId = await this.fingerprintService.getVisitorId();
    const response = await this.hubConnection.invoke<RegisterPlayerResponse>('RegisterPlayer', visitorId);
    this.store.dispatch(new Game.PlayerRegistered(response));
  }

  public async submitGuess() {
    // check for mismatched length
    // if (this.guessSubject.value.length !== this.wordHintSubject.value.length) {
    //   this.soundService.play(SoundSprite.Incorrect);
    //   return;
    // }

    // const response = await this.hubConnection.invoke<GuessResponse>('SubmitGuess', {
    //   playerId: this.playerId,
    //   roundId: this.roundId,
    //   value: this.guessSubject.value,
    // });

    // // play sound to indicate correct / incorrect
    // const sprite = response.correct ? SoundSprite.Correct : SoundSprite.Incorrect;
    // this.soundService.play(sprite);

    // return response;
  }
}
