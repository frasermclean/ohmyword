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
}

const WORDS_STATE_TOKEN = new StateToken<WordsStateModel>('words');

@State<WordsStateModel>({
  name: WORDS_STATE_TOKEN,
  defaults: {
    status: 'ready',
    error: null,
    words: [],
  },
})
@Injectable()
export class WordsState {
  constructor(private wordsService: WordsService) {}

  @Action(Words.GetAllWords)
  getAllWords(context: StateContext<WordsStateModel>) {
    context.patchState({ status: 'busy', words: [] });
    return this.wordsService.getAllWords().pipe(
      tap((words) => context.patchState({ status: 'ready', words })),
      catchError((error) => {
        context.patchState({ status: 'error', error });
        return of(error);
      })
    );
  }

  @Action(Words.CreateWord)
  createWord(context: StateContext<WordsStateModel>, action: Words.CreateWord) {
    context.patchState({ status: 'busy' });
    return this.wordsService.createWord(action.request).pipe(
      tap((word) =>
        context.patchState({
          status: 'ready',
          words: [...context.getState().words, word],
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
    return this.wordsService.deleteWord(action.partOfSpeech, action.id).pipe(
      tap(() => {
        const words = context.getState().words.filter((word) => word.id !== action.id);
        context.patchState({ status: 'ready', words });
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
}
