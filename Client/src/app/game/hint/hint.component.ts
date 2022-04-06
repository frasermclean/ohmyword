import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { LetterHint } from 'src/app/models/letter-hint';
import { WordHint } from 'src/app/models/word-hint';
import { GameService } from 'src/app/services/game.service';

export interface LetterData {
  position: number;
  hintValue: string | null;
  guessValue: string | null;
}

@Component({
  selector: 'app-hint',
  templateUrl: './hint.component.html',
  styleUrls: ['./hint.component.scss'],
})
export class HintComponent implements OnInit, OnDestroy, OnChanges {
  @Input() guess: string = '';
  @Input() wordHint: WordHint = null!;

  wordHintSubscription = this.gameService.wordHint$.subscribe((wordHint) => this.onWordHint(wordHint));
  letterHintSubscription = this.gameService.letterHint$.subscribe((letterHint) => this.onLetterHint(letterHint));

  letters: LetterData[] = [];

  constructor(private gameService: GameService) {}

  ngOnInit(): void {}

  ngOnDestroy(): void {
    this.wordHintSubscription.unsubscribe();
    this.letterHintSubscription.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log(changes);
    if (!this.wordHint) throw new Error('Word hint has not been set!');

    if (changes.guess) {
      this.letters = [];
      for (let i = 0; i < this.wordHint.length; i++) {
        this.letters.push({
          position: i + 1,
          hintValue: null,
          guessValue: this.guess[i] || null,
        });
      }
    }
  }

  onWordHint(wordHint: WordHint) {
    console.log('Word hint: ', wordHint);
  }

  onLetterHint(letterHint: LetterHint) {
    console.log('Letter hint: ', letterHint);
    if (letterHint === LetterHint.default) return;

    if (!this.letters) {
      console.warn('No letters to update!');
      return;
    }

    
    console.log(this.letters);    

    this.letters[letterHint.position - 1].hintValue = letterHint.value;
  }
}
