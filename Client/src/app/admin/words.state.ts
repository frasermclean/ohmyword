import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Word } from '../models/word.model';
import { WordsService } from '../services/words.service';
import { Words } from './words.actions';

interface WordsStateModel {
  status: 'initial' | 'loading' | 'loaded' | 'error';
  error: any;
  words: Word[];
}

const WORDS_STATE_TOKEN = new StateToken<WordsStateModel>('words');

@State<WordsStateModel>({
  name: WORDS_STATE_TOKEN,
  defaults: {
    status: 'initial',
    error: null,
    words: [],
  },
})
@Injectable()
export class WordsState {
  constructor(private wordsService: WordsService) {}

  @Action(Words.GetAllWords)
  getAllWords(context: StateContext<WordsStateModel>) {
    context.patchState({ status: 'loading', words: [] });
    return this.wordsService.getAllWords().pipe(
      tap((words) => context.patchState({ status: 'loaded', words })),
      catchError((error) => {
        context.patchState({ status: 'error', error });
        return of(error);
      }));
  }

  @Selector([WORDS_STATE_TOKEN])
  static status(state: WordsStateModel) {
    return state.status;
  }

  @Selector([WORDS_STATE_TOKEN])
  static words(state: WordsStateModel) {
    return state.words;
  }
}
