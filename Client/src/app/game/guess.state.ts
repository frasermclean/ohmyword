import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { Game } from './game.state';

export interface GuessStateModel {
  guess: string;
  count: number;
  maxLength: number;
}
export namespace Guess {
  export class Append {
    static readonly type = '[Guess Component] Guess.Append';
    constructor(public value: string) {}
  }

  export class Backspace {
    static readonly type = '[Guess Component] Guess.Backspace';
  }

  export class Submit {
    static readonly type = '[Guess Component] Guess.Submit';
  }
}

const GUESS_STATE_TOKEN = new StateToken<GuessStateModel>('guess');
export const GUESS_DEFAULT_CHAR = '_';

@State<GuessStateModel>({
  name: GUESS_STATE_TOKEN,
  defaults: {
    guess: '',
    count: 0,
    maxLength: 0,
  },
})
@Injectable()
export class GuessState {
  @Action(Guess.Append)
  append(context: StateContext<GuessStateModel>, action: Guess.Append) {
    const state = context.getState();
    if (state.guess.length === state.maxLength) return;
    context.patchState({
      guess: state.guess + action.value,
    });
  }

  @Action(Guess.Backspace)
  backspace(context: StateContext<GuessStateModel>) {
    const state = context.getState();
    if (state.guess.length === 0) return;
    context.patchState({
      guess: state.guess.slice(0, -1),
    });
  }

  @Action(Guess.Submit)
  submit(context: StateContext<GuessStateModel>) {
    const state = context.getState();
    context.patchState({
      guess: '',
      count: state.count + 1,
    });
  }

  @Action(Game.RoundStarted)
  roundStarted(context: StateContext<GuessStateModel>, action: Game.RoundStarted) {
    // reset state on new round
    context.setState({
      guess: '',
      count: 0,
      maxLength: action.wordHint.length,
    });
  }

  @Selector([GUESS_STATE_TOKEN])
  static state(state: GuessStateModel) {
    return state;
  }

  @Selector([GUESS_STATE_TOKEN])
  static guessChar(state: GuessStateModel) {
    return (index: number) => state.guess[index] || GUESS_DEFAULT_CHAR;
  }
}
