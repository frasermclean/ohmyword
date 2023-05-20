import { Component } from '@angular/core';
import { Store } from '@ngxs/store';
import { GameState } from '@state/game/game.state';

@Component({
  selector: 'app-round-summary',
  templateUrl: './round-summary.component.html',
  styleUrls: ['./round-summary.component.scss'],
})
export class RoundSummaryComponent {
  roundSummary$ = this.store.select(GameState.roundSummary);

  constructor(private store: Store) {}
}
