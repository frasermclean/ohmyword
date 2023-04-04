import { Component, Input, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { map } from 'rxjs/operators';
import { GuessState, GUESS_DEFAULT_CHAR } from '../../guess.state';


@Component({
  selector: 'app-letter',
  templateUrl: './letter.component.html',
  styleUrls: ['./letter.component.scss'],
})
export class LetterComponent implements OnInit {
  @Input() hintChar: string = '';
  @Input() index: number = 0;

  guessChar$ = this.store.select(GuessState.guessChar).pipe(map((indexer) => indexer(this.index)));

  constructor(private store: Store) {}

  ngOnInit(): void {}

  getLetter(guessChar: string) {
    const letter = guessChar === GUESS_DEFAULT_CHAR ? this.hintChar : guessChar;
    return letter.toUpperCase();
  }

  getClass(guessChar: string): 'correct' | 'incorrect' | 'hint' | 'default' {
    return this.hintChar
      ? guessChar !== GUESS_DEFAULT_CHAR
        ? guessChar === this.hintChar
          ? 'correct' // guess matched with hint
          : 'incorrect' // guess didn't match with hint
        : 'hint' // hint only
      : 'default';
  }
}
