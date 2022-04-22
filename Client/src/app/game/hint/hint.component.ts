import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { Subscription } from 'rxjs';
import { WordHint } from 'src/app/models/word-hint';
import { GameService } from 'src/app/services/game.service';

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
  wordHint = WordHint.default;
  letters: LetterData[] = [];
  wordHintSubscription: Subscription = null!;
  letterHintSubscription: Subscription = null!;
  guessSubscription: Subscription = null!;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.wordHintSubscription = this.gameService.wordHint$.subscribe((wordHint) => {
      this.wordHint = wordHint;
      for (let i = 1; i <= wordHint.length; i++) {
        this.letters.push({
          hint: wordHint.letters.find((letterHint) => letterHint.position === i)?.value ?? null,
          guess: null,
        });
      }
    });

    this.letterHintSubscription = this.gameService.letterHint$.subscribe((letterHint) => {
      this.letters[letterHint.position - 1].hint = letterHint.value;
    });

    this.guessSubscription = this.gameService.guess$.subscribe((guess) => {
      this.letters.forEach((letter, i) => {
        letter.guess = guess[i] || null;
      });
    });
  }

  ngOnDestroy(): void {
    this.wordHintSubscription.unsubscribe();
    this.letterHintSubscription.unsubscribe();
    this.guessSubscription.unsubscribe();
  }
}
