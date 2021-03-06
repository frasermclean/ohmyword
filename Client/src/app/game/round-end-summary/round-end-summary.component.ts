import { Component, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { GameState } from '../game.state';

@Component({
  selector: 'app-round-end-summary',
  templateUrl: './round-end-summary.component.html',
  styleUrls: ['./round-end-summary.component.scss'],
})
export class RoundEndSummaryComponent implements OnInit {
  summary$ = this.store.select(GameState.roundEndSummary);
  constructor(private store: Store) {}

  ngOnInit(): void {}
}
