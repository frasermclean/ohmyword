import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { Role } from '@models/enums';
import { AuthService } from '@services/auth.service';
import { Auth } from './auth.actions';

interface AuthStateModel {
  busy: boolean;
  name: string;
  role: Role;
}

export const DEFAULT_DISPLAY_NAME = 'Guest';

const AUTH_STATE_TOKEN = new StateToken<AuthStateModel>('auth');
@State<AuthStateModel>({
  name: AUTH_STATE_TOKEN,
  defaults: {
    busy: true,
    name: '',
    role: Role.Guest,
  },
})
@Injectable()
export class AuthState {
  constructor(private authService: AuthService) {}

  @Action(Auth.Login)
  login() {
    this.authService.login();
  }

  @Action(Auth.Logout)
  logout() {
    this.authService.logout();
  }

  @Action(Auth.Complete)
  loggedIn(context: StateContext<AuthStateModel>, action: Auth.Complete) {
    context.setState({
      busy: false,
      name: action.displayName,
      role: action.role,
    });
  }

  @Selector()
  static busy(state: AuthStateModel) {
    return state.busy;
  }

  @Selector()
  static displayName(state: AuthStateModel) {
    return state.name;
  }

  @Selector()
  static role(state: AuthStateModel) {
    return state.role;
  }
}
