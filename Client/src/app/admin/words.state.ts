import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
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
  orderBy: 'value' | 'partOfSpeech' | 'definition' | 'lastModifiedTime';
  desc: boolean;
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
    orderBy: 'value',
    desc: false,
  },
})
@Injectable()
export class WordsState {
  constructor(private wordsService: WordsService) {}

  @Action(Words.GetWords)
  getWords(context: StateContext<WordsStateModel>, action: Words.GetWords) {
    context.patchState({ status: 'busy', words: [] });
    const state = context.getState();
    return this.wordsService
      .getWords({
        offset: action.request.offset || state.offset,
        limit: action.request.limit || state.limit,
        filter: action.request.filter || state.filter,
        orderBy: action.request.orderBy || state.orderBy,
        desc: action.request.desc || state.desc,
      })
      .pipe(
        tap((response) =>
          context.patchState({
            status: 'ready',
            words: response.words,
            offset: response.offset,
            limit: response.limit,
            total: response.total,
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
}
