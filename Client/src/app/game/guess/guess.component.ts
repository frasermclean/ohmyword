import { Component, EventEmitter, HostListener, OnInit, Output } from '@angular/core';
import { GuessResponse } from 'src/app/models/responses/guess.response';
import { Store } from '@ngxs/store';
import { Guess } from '../guess.state';

@Component({
  selector: 'app-guess',
  templateUrl: './guess.component.html',
  styleUrls: ['./guess.component.scss'],
})
export class GuessComponent implements OnInit {
  response: GuessResponse | null = null;

  @Output()
  valueChanged = new EventEmitter<string>();

  @HostListener('window:keyup', ['$event'])
  onKeyUpEvent(event: KeyboardEvent) {
    switch (event.key) {
      case 'Backspace':
        this.deleteCharFromGuess();
        break;
      case 'Enter':
        this.submitGuess();
        break;
      default:
        if (event.key.length !== 1 || !event.key[0].match(/[A-z]/g)) return;
        this.addCharToGuess(event.key.toLowerCase());
        break;
    }
  }

  constructor(private store: Store) {}

  ngOnInit(): void {}

  addCharToGuess(char: string) {
    this.store.dispatch(new Guess.Append(char));
  }

  deleteCharFromGuess() {
    this.store.dispatch(new Guess.Backspace());
  }

  submitGuess() {
    this.store.dispatch(new Guess.Submit());
  }
}
