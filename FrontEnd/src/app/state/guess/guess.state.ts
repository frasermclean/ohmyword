import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken, Store } from '@ngxs/store';
import { HubService } from '@services/hub.service';
import { SoundService } from '@services/sound.service';
import { Game } from '../game/game.actions';
import { GAME_STATE_TOKEN } from '../game/game.state';
import { Guess } from './guess.actions';

interface GuessStateModel {
  value: string;
  count: number;
  maxLength: number;
  guessedCorrectly: boolean;
  message: string;
}

const GUESS_VALUE_DEFAULT = '';
const GUESS_STATE_TOKEN = new StateToken<GuessStateModel>('guess');
export const GUESS_DEFAULT_CHAR = '_';

@State<GuessStateModel>({
  name: GUESS_STATE_TOKEN,
  defaults: {
    value: GUESS_VALUE_DEFAULT,
    count: 0,
    maxLength: 0,
    guessedCorrectly: false,
    message: '',
  },
})
@Injectable()
export class GuessState {
  constructor(private store: Store, private hubService: HubService, private soundService: SoundService) {
  }

  @Action(Guess.SetValue)
  setValue(context: StateContext<GuessStateModel>, action: Guess.SetValue) {
    context.patchState({
      value: action.value,
    });
  }

  @Action(Guess.Submit)
  submit(context: StateContext<GuessStateModel>, action: Guess.Submit) {
    const state = context.getState();
    context.patchState({
      value: action.value,
      count: state.count + 1,
    });

    const roundId = this.store.selectSnapshot(GAME_STATE_TOKEN).roundId;
    this.hubService.submitGuess(roundId, action.value);
  }

  @Action(Guess.Succeeded)
  guessSucceeded(context: StateContext<GuessStateModel>, action: Guess.Succeeded) {
    context.patchState({
      value: GUESS_VALUE_DEFAULT,
      guessedCorrectly: true,
    });
    context.dispatch(new Game.AddPoints(action.points));
    this.soundService.playCorrect();
  }

  @Action(Guess.Failed)
  guessFailed(context: StateContext<GuessStateModel>, action: Guess.Failed) {
    this.soundService.playIncorrect();
    context.patchState({
      value: GUESS_VALUE_DEFAULT,
      message: action.message
    })
  }

  @Action(Game.RoundStarted)
  setNewWord(context: StateContext<GuessStateModel>, action: Game.RoundStarted) {
    context.setState({
      value: GUESS_VALUE_DEFAULT,
      count: 0,
      maxLength: action.data.wordHint?.length || 0,
      guessedCorrectly: false,
      message: '',
    });
  }

  @Selector([GUESS_STATE_TOKEN])
  static maxLength(state: GuessStateModel) {
    return state.maxLength;
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
