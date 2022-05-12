import { Component, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { Guess } from '../game.actions';

@Component({
  selector: 'app-keyboard',
  templateUrl: './keyboard.component.html',
  styleUrls: ['./keyboard.component.scss'],
})
export class KeyboardComponent implements OnInit {
  readonly keys = [
    ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'],
    ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l'],
    ['z', 'x', 'c', 'v', 'b', 'n', 'm'],
  ];

  constructor(private store: Store) {}

  ngOnInit(): void {}

  onKeyClick(key: string) {
    this.store.dispatch(new Guess.Append(key));
  }

  onBackspaceClick() {
    this.store.dispatch(new Guess.Backspace());
  }

  onSubmitClick() {
    this.store.dispatch(new Guess.Submit());
  }
}
