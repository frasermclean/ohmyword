import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { Hub } from '../game.actions';
import { GameState } from '../game.state';

@Component({
  selector: 'game-container',
  templateUrl: './game-container.component.html',
  styleUrls: ['./game-container.component.scss'],
})
export class GameContainerComponent implements OnInit, OnDestroy {
  registered$ = this.store.select(GameState.registered);
  connectionState$ = this.store.select(GameState.connectionState);
  round$ = this.store.select(GameState.round);
  showKeyboard = false

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.store.dispatch(new Hub.Connect());
  }

  ngOnDestroy(): void {
    this.store.dispatch(new Hub.Disconnect());
  }
}
