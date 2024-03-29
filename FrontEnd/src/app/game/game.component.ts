import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';

import { GameState } from '@state/game/game.state';
import { GuessState } from '@state/guess/guess.state';
import { Hub } from '@state/hub/hub.actions';
import { HubState } from '@state/hub/hub.state';

@Component({
  selector: 'app-game-root',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
})
export class GameRootComponent implements OnInit, OnDestroy {
  connection$ = this.store.select(GameState.connection);
  roundActive$ = this.store.select(GameState.roundActive);
  interval$ = this.store.select(GameState.interval);
  guessedCorrectly$ = this.store.select(GuessState.guessedCorrectly);

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.connect();
  }

  ngOnDestroy(): void {
    this.store.dispatch(new Hub.Disconnect());
  }

  connect() {
    this.store.dispatch(new Hub.Connect());
  }
}
