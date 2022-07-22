import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Store } from '@ngxs/store';
import { map, withLatestFrom } from 'rxjs/operators';
import { GetWordsRequest } from 'src/app/models/requests/get-words.request';
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
  totalWords$ = this.store.select(WordsState.total);
  pageSize$ = this.store.select(WordsState.limit);
  pageIndex$ = this.store.select(WordsState.offset).pipe(
    withLatestFrom(this.pageSize$),
    map(([offset, pageSize]) => offset / pageSize)
  );

  readonly displayedColumns = ['value', 'partOfSpeech', 'definition', 'lastModifiedTime', 'actions'];

  constructor(private store: Store, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.getWords({});
  }

  onSortChange(event: Sort) {
    if (!event.direction) return; // no sort direction
    this.getWords({ orderBy: event.active, desc: event.direction === 'desc' });
  }

  onPageEvent(event: PageEvent) {
    const offset = event.pageIndex * event.pageSize;
    this.getWords({ offset, limit: event.pageSize });
  }

  getWords(request: Partial<GetWordsRequest>) {
    this.store.dispatch(new Words.GetWords(request));
  }

  createWord() {
    this.dialog
      .open(WordEditComponent)
      .afterClosed()
      .subscribe((result) => {
        if (!result) return;
        this.store.dispatch(new Words.CreateWord(result));
      });
  }

  editWord(word: Word) {
    this.dialog
      .open(WordEditComponent, { data: { word } })
      .afterClosed()
      .subscribe((result) => {
        if (!result) return;
        this.store.dispatch(new Words.UpdateWord({ ...result, id: word.id }));
      });
  }

  deleteWord(word: Word) {
    this.store.dispatch(new Words.DeleteWord(word.partOfSpeech, word.id));
  }
}
