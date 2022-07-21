import { Component, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { Word } from 'src/app/models/word.model';
import { Words } from '../words.actions';
import { WordsState } from '../words.state';

@Component({
  selector: 'app-word-list',
  templateUrl: './word-list.component.html',
  styleUrls: ['./word-list.component.scss'],
})
export class WordListComponent implements OnInit {
  status$ = this.store.select(WordsState.status);
  words$ = this.store.select(WordsState.words);
  readonly displayedColumns = ['value', 'partOfSpeech', 'definition', 'actions'];

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.store.dispatch(new Words.GetAllWords());
  }

  reloadWords() {
    this.store.dispatch(new Words.GetAllWords());
  }

  createWord() {}

  editWord(word: Word) {}

  deleteWord(word: Word) {}
}
