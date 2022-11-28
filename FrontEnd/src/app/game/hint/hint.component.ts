import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { GameState } from '../game.state';

export interface LetterData {
  hint: string | null;
  guess: string | null;
}

@Component({
  selector: 'app-hint',
  templateUrl: './hint.component.html',
  styleUrls: ['./hint.component.scss'],
})
export class HintComponent {
  wordHint$ = this.store.select(GameState.wordHint);

  constructor(private store: Store) {}
}
