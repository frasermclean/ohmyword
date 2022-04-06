import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { WordHint } from 'src/app/models/word-hint';

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
export class HintComponent implements OnChanges {
  @Input() guess: string = '';
  @Input() wordHint: WordHint = null!;

  letters: LetterData[] = [];

  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
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
}
