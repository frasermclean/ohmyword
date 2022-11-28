import {Component, OnDestroy, OnInit} from '@angular/core';
import {FormControl} from '@angular/forms';
import {MatDialog} from '@angular/material/dialog';
import {PageEvent} from '@angular/material/paginator';
import {Sort} from '@angular/material/sort';
import {Store} from '@ngxs/store';
import {Subject} from 'rxjs';
import {debounceTime, map, takeUntil, tap, withLatestFrom} from 'rxjs/operators';
import {GetWordsOrderBy} from 'src/app/models/enums/get-words-order-by.enum';
import {SortDirection} from 'src/app/models/enums/sort-direction.enum';
import {GetWordsRequest} from 'src/app/models/requests/get-words.request';
import {Word} from 'src/app/models/word.model';
import {WordEditComponent} from '../word-edit/word-edit.component';
import {Words} from '../words.actions';
import {WordsState} from '../words.state';
import {
  ConfirmationPromptComponent,
  ConfirmationPromptData
} from "../confirmation-prompt/confirmation-prompt.component";

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

  readonly displayedColumns = ['value', 'partOfSpeech', 'definition', 'lastModifiedTime', 'actions'];

  constructor(private store: Store, private dialog: MatDialog) {
  }

  ngOnInit(): void {
    this.getWords({});

    this.searchInput.valueChanges
      .pipe(
        debounceTime(500),
        takeUntil(this.destroy$),
        tap((value) => {
          if (typeof value !== 'string') return;
          this.store.dispatch(new Words.Search(value));
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
    this.getWords({
      orderBy,
      direction: event.direction === 'asc' ? SortDirection.Ascending : SortDirection.Descending,
    });
  }

  onPageEvent(event: PageEvent) {
    const offset = event.pageIndex * event.pageSize;
    this.getWords({offset, limit: event.pageSize});
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
      .open(WordEditComponent, {data: {word}})
      .afterClosed()
      .subscribe((result) => {
        if (!result) return;
        this.store.dispatch(new Words.UpdateWord({...result, id: word.id}));
      });
  }

  deleteWord(word: Word) {
    this.dialog
      .open<ConfirmationPromptComponent, ConfirmationPromptData, boolean>(ConfirmationPromptComponent, {
        data: {
          title: `Delete word: ${word.value}`,
          question: `Are you sure you wish to delete the word: ${word.value}?`
        }
      })
      .afterClosed()
      .subscribe(result => {
        if (!result) return;
        this.store.dispatch(new Words.DeleteWord(word.partOfSpeech, word.id));
      });
  }

  private static parseOrderByString(value: string) {
    switch (value) {
      case 'partOfSpeech':
        return GetWordsOrderBy.PartOfSpeech;
      case 'definition':
        return GetWordsOrderBy.Definition;
      case 'lastModifiedTime':
        return GetWordsOrderBy.LastModifiedTime;
      default:
        return GetWordsOrderBy.Value;
    }
  }
}
