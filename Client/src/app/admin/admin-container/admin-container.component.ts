
import { Component, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';

import { Words } from '../words.actions';
import { WordsState } from '../words.state';

@Component({
  selector: 'admin-container',
  templateUrl: './admin-container.component.html',
  styleUrls: ['./admin-container.component.scss'],
})
export class AdminContainerComponent implements OnInit {
  status$ = this.store.select(WordsState.status);
  words$ = this.store.select(WordsState.words);

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.store.dispatch(new Words.GetAllWords());
  }

  onReload() {
    this.store.dispatch(new Words.GetAllWords());
  }
}
