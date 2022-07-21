import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Store } from '@ngxs/store';
import { Word } from 'src/app/models/word.model';
import { WordEditComponent } from '../word-edit/word-edit.component';
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

  constructor(private store: Store, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.store.dispatch(new Words.GetAllWords());
  }

  reloadWords() {
    this.store.dispatch(new Words.GetAllWords());
  }

  createWord() {
    this.dialog
      .open(WordEditComponent)
      .afterClosed()
      .subscribe((result) => {
        if (!result) return;
        console.log(result);
      });
  }

  editWord(word: Word) {
    this.dialog
      .open(WordEditComponent, { data: { word } })
      .afterClosed()
      .subscribe((result) => {
        if (!result) return;
        console.log(result);
      });
  }

  deleteWord(word: Word) {}
}
