import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { Subscription } from 'rxjs';
import { tap } from 'rxjs/operators';
import { GameService } from 'src/app/services/game.service';
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
export class HintComponent implements OnInit, OnDestroy {
  wordHint$ = this.store.select(GameState.wordHint).pipe(
    tap((wordHint) => {
      this.letterData = [];
      for (let i = 0; i < wordHint.length; i++) {
        this.letterData.push({
          hint: wordHint.letterHints[i] || null,
          guess: null,
        });
      }
    })
  );
  letterData: LetterData[] = [];
  guessSubscription: Subscription = null!;

  constructor(private gameService: GameService, private store: Store) {}

  ngOnInit(): void {
    this.guessSubscription = this.gameService.guess$.subscribe((guess) => {
      this.letterData.forEach((letter, i) => {
        letter.guess = guess[i] || null;
      });
    });
  }

  ngOnDestroy(): void {
    this.guessSubscription.unsubscribe();
  }
}
