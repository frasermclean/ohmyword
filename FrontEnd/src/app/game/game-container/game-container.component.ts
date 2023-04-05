import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { AuthState } from '@state/auth/auth.state';
import { Hub } from '../../state/game/game.actions';
import { GameState } from '@state/game/game.state';
import { GuessState } from '@state/guess/guess.state';

@Component({
  selector: 'game-container',
  templateUrl: './game-container.component.html',
  styleUrls: ['./game-container.component.scss'],
})
export class GameContainerComponent implements OnInit, OnDestroy {
  registered$ = this.store.select(GameState.registered);
  connectionState$ = this.store.select(GameState.connectionState);
  roundActive$ = this.store.select(GameState.roundActive);
  interval$ = this.store.select(GameState.interval);
  guessedCorrectly$ = this.store.select(GuessState.guessedCorrectly);
  authBusy$ = this.store.select(AuthState.busy);
  showKeyboard = false;

  constructor(private store: Store) {}

  ngOnInit(): void {}

  connect() {
    this.store.dispatch(new Hub.Connect());
  }

  ngOnDestroy(): void {
    this.store.dispatch(new Hub.Disconnect());
  }
}
