import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken, Store } from '@ngxs/store';
import { GameService } from '@services/game.service';
import { SoundService } from '@services/sound.service';
import { Game } from '../game/game.actions';
import { GAME_STATE_TOKEN } from '../game/game.state';
import { Guess } from './guess.actions';

interface GuessStateModel {
  value: string;
  count: number;
  maxLength: number;
  guessedCorrectly: boolean;
}

const GUESS_STATE_TOKEN = new StateToken<GuessStateModel>('guess');
export const GUESS_DEFAULT_CHAR = '_';

@State<GuessStateModel>({
  name: GUESS_STATE_TOKEN,
  defaults: {
    value: '',
    count: 0,
    maxLength: 0,
    guessedCorrectly: false,
  },
})
@Injectable()
export class GuessState {
  constructor(private store: Store, private gameService: GameService, private soundService: SoundService) {}

  @Action(Guess.Append)
  append(context: StateContext<GuessStateModel>, action: Guess.Append) {
    const state = context.getState();
    if (state.value.length === state.maxLength) return;
    context.patchState({
      value: state.value + action.value,
    });
  }

  @Action(Guess.Backspace)
  backspace(context: StateContext<GuessStateModel>) {
    const state = context.getState();
    if (state.value.length === 0) return;
    context.patchState({
      value: state.value.slice(0, -1),
    });
  }

  @Action(Guess.Submit)
  submit(context: StateContext<GuessStateModel>) {
    const state = context.getState();
    context.patchState({
      value: '',
      count: state.count + 1,
    });

    const roundId = this.store.selectSnapshot(GAME_STATE_TOKEN).roundId;
    this.gameService.submitGuess(roundId, state.value);
  }

  @Action(Guess.Succeeded)
  guessSucceeded(context: StateContext<GuessStateModel>, action: Guess.Succeeded) {
    context.patchState({
      guessedCorrectly: true,
    });
    context.dispatch(new Game.AddPoints(action.points));
    this.soundService.playCorrect();
  }

  @Action(Guess.Failed)
  guessFailed() {
    this.soundService.playIncorrect();
  }

  @Action(Guess.SetNewWord)
  setNewWord(context: StateContext<GuessStateModel>, action: Guess.SetNewWord) {
    context.setState({
      value: '',
      count: 0,
      maxLength: action.wordHint.length,
      guessedCorrectly: false,
    });
  }

  @Selector([GUESS_STATE_TOKEN])
  static guessedCorrectly(state: GuessStateModel) {
    return state.guessedCorrectly;
  }

  @Selector([GUESS_STATE_TOKEN])
  static guessChar(state: GuessStateModel) {
    return (index: number) => (index < state.value.length ? state.value[index] : GUESS_DEFAULT_CHAR);
  }
}
