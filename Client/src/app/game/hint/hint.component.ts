import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { Hint } from 'src/app/models/hint';

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
  @Input() hint: Hint = null!;

  letters: LetterData[] = [];

  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.hint) throw new Error('Hint has not been set!');

    if (changes.guess) {
      this.letters = [];
      for (let i = 0; i < this.hint.length; i++) {
        this.letters.push({
          position: i + 1,
          hintValue: i === 0 ? 'r' : i === 2 ? 'o' : null,
          guessValue: this.guess[i] || null,
        });
      }
    }
  }
}
