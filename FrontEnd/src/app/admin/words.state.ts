import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { GetWordsOrderBy } from '../models/enums/get-words-order-by.enum';
import { SortDirection } from '../models/enums/sort-direction.enum';
import { Word } from '../models/word.model';
import { WordsService } from '../services/words.service';
import { Words } from './words.actions';

interface WordsStateModel {
  status: 'ready' | 'busy' | 'error';
  error: any;
  words: Word[];
  offset: number;
  limit: number;
  total: number;
  filter: string;
  orderBy: GetWordsOrderBy;
  direction: SortDirection;
}

const WORDS_STATE_TOKEN = new StateToken<WordsStateModel>('words');

@State<WordsStateModel>({
  name: WORDS_STATE_TOKEN,
  defaults: {
    status: 'ready',
    error: null,
    words: [],
    offset: 0,
    limit: 10,
    total: 0,
    filter: '',
    orderBy: GetWordsOrderBy.Id,
    direction: SortDirection.Ascending,
  },
})
@Injectable()
export class WordsState {
  constructor(private wordsService: WordsService) {}

  @Action(Words.GetWords)
  getWords(context: StateContext<WordsStateModel>, action: Words.GetWords) {
    context.patchState({ status: 'busy' });
    const state = context.getState();
    return this.wordsService
      .getWords({
        offset: action.request.offset || state.offset,
        limit: action.request.limit || state.limit,
        filter: typeof (action.request.filter === 'string') ? action.request.filter! : state.filter,
        orderBy: action.request.orderBy || state.orderBy,
        direction: action.request.direction || state.direction,
      })
      .pipe(
        tap((response) =>
          context.setState({
            status: 'ready',
            words: response.words,
            error: null,
            offset: response.offset,
            limit: response.limit,
            total: response.total,
            filter: response.filter,
            orderBy: response.orderBy,
            direction: response.direction,
          })
        ),
        catchError((error) => {
          context.patchState({ status: 'error', error });
          return of(error);
        })
      );
  }

  @Action(Words.CreateWord)
  createWord(context: StateContext<WordsStateModel>, action: Words.CreateWord) {
    context.patchState({ status: 'busy' });
    const state = context.getState();
    return this.wordsService.createWord(action.request).pipe(
      tap((word) =>
        context.patchState({
          status: 'ready',
          total: state.total + 1,
          words: [...state.words, word],
        })
      ),
      catchError((error) => {
        context.patchState({ status: 'error', error });
        return of(error);
      })
    );
  }

  @Action(Words.UpdateWord)
  updateWord(context: StateContext<WordsStateModel>, action: Words.UpdateWord) {
    context.patchState({ status: 'busy' });
    return this.wordsService.updateWord(action.request).pipe(
      tap((word) => {
        const words = context.getState().words.map((w) => (w.id === word.id ? word : w));
        context.patchState({ status: 'ready', words });
      }),
      catchError((error) => {
        context.patchState({ status: 'error', error });
        return of(error);
      })
    );
  }

  @Action(Words.DeleteWord)
  deleteWord(context: StateContext<WordsStateModel>, action: Words.DeleteWord) {
    context.patchState({ status: 'busy' });
    const state = context.getState();
    return this.wordsService.deleteWord(action.partOfSpeech, action.id).pipe(
      tap(() => {
        const words = context.getState().words.filter((word) => word.id !== action.id);
        context.patchState({ status: 'ready', total: state.total - 1, words });
      }),
      catchError((error) => {
        context.patchState({ status: 'error', error });
        return of(error);
      })
    );
  }

  @Action(Words.Search)
  search(context: StateContext<WordsStateModel>, action: Words.Search) {
    const state = context.getState();
    return context.dispatch(
      new Words.GetWords({
        offset: 0,
        limit: state.limit,
        filter: action.filter,
        orderBy: state.orderBy,
        direction: SortDirection.Ascending,
      })
    );
  }

  @Selector([WORDS_STATE_TOKEN])
  static status(state: WordsStateModel) {
    return state.status;
  }

  @Selector([WORDS_STATE_TOKEN])
  static words(state: WordsStateModel) {
    return state.words;
  }

  @Selector([WORDS_STATE_TOKEN])
  static offset(state: WordsStateModel) {
    return state.offset;
  }

  @Selector([WORDS_STATE_TOKEN])
  static limit(state: WordsStateModel) {
    return state.limit;
  }

  @Selector([WORDS_STATE_TOKEN])
  static total(state: WordsStateModel) {
    return state.total;
  }

  @Selector([WORDS_STATE_TOKEN])
  static sorting(state: WordsStateModel) {
    return {
      orderBy: state.orderBy,
      direction: state.direction,
    };
  }
}
