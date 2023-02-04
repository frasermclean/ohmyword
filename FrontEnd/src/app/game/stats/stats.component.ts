import { Component, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { GameState } from '../game.state';

@Component({
  selector: 'app-stats',
  templateUrl: './stats.component.html',
  styleUrls: ['./stats.component.scss'],
})
export class StatsComponent implements OnInit {
  roundNumber$ = this.store.select(GameState.roundNumber);
  score$ = this.store.select(GameState.score);
  playerCount$ = this.store.select(GameState.playerCount);

  constructor(private store: Store) {}

  ngOnInit(): void {}
}
