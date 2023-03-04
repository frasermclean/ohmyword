import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { Store } from '@ngxs/store';
import { Subject } from 'rxjs';
import { debounceTime, map, takeUntil, tap, withLatestFrom } from 'rxjs/operators';
import { SearchWordsOrderBy } from 'src/app/models/enums/search-words-order-by.enum';
import { SortDirection } from 'src/app/models/enums/sort-direction.enum';
import { SearchWordsRequest } from 'src/app/models/requests/search-words.request';
import { Word } from 'src/app/models/word.model';
import { WordEditComponent } from '../word-edit/word-edit.component';
import { Words } from '../words.actions';
import { WordsState } from '../words.state';
import {
  ConfirmationPromptComponent,
  ConfirmationPromptData,
} from '../confirmation-prompt/confirmation-prompt.component';

@Component({
  selector: 'app-word-list',
  templateUrl: './word-list.component.html',
  styleUrls: ['./word-list.component.scss'],
})
export class WordListComponent implements OnInit, OnDestroy {
  status$ = this.store.select(WordsState.status);
  words$ = this.store.select(WordsState.words);
  totalWords$ = this.store.select(WordsState.total);
  pageSize$ = this.store.select(WordsState.limit);
  pageIndex$ = this.store.select(WordsState.offset).pipe(
    withLatestFrom(this.pageSize$),
    map(([offset, pageSize]) => offset / pageSize)
  );
  private readonly destroy$ = new Subject<void>();

  searchInput = new FormControl('');
  highlightedWord: Word | null = null;

  readonly displayedColumns = ['id', 'length', 'definitionCount', 'lastModifiedTime', 'actions'];

  constructor(private store: Store, private dialog: MatDialog) {}

  ngOnInit(): void {
    this.searchWords();

    this.searchInput.valueChanges
      .pipe(
        debounceTime(500),
        takeUntil(this.destroy$),
        tap((value) => {
          if (typeof value !== 'string') return;
          this.store.dispatch(new Words.SearchWords({ filter: value }));
        })
      )
      .subscribe();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onSortChange(event: Sort) {
    if (!event.direction) return; // no sort direction
    const orderBy = WordListComponent.parseOrderByString(event.active);
    this.searchWords({
      orderBy,
      direction: event.direction === 'asc' ? SortDirection.Ascending : SortDirection.Descending,
    });
  }

  onPageEvent(event: PageEvent) {
    const offset = event.pageIndex * event.pageSize;
    this.searchWords({ offset, limit: event.pageSize });
  }

  searchWords(request?: Partial<SearchWordsRequest>) {
    this.store.dispatch(new Words.SearchWords(request || {}));
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
      .open(WordEditComponent, { data: { wordId: word.id } })
      .afterClosed()
      .subscribe((result) => {
        if (!result) return;
        this.store.dispatch(new Words.UpdateWord({ ...result, id: word.id }));
      });
  }

  deleteWord(word: Word) {
    this.dialog
      .open<ConfirmationPromptComponent, ConfirmationPromptData, boolean>(ConfirmationPromptComponent, {
        data: {
          title: `Delete word: ${word.id}`,
          question: `Are you sure you wish to delete the word: ${word.id}?`,
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (!result) return;
        this.store.dispatch(new Words.DeleteWord(word.id));
      });
  }

  private static parseOrderByString(value: string) {
    switch (value) {
      case 'length':
        return SearchWordsOrderBy.Length;
      case 'definitionCount':
        return SearchWordsOrderBy.DefinitionCount;
      case 'lastModifiedTime':
        return SearchWordsOrderBy.LastModifiedTime;
      default:
        return SearchWordsOrderBy.Id;
    }
  }
}
