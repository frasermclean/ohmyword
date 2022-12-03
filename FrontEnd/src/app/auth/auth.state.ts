import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { Role } from '../models/role.enum';
import { AuthService } from '../services/auth.service';
import { Auth } from './auth.actions';

interface AuthStateModel {
  loggedIn: boolean;
  name: string;
  role: Role;
}

const AUTH_STATE_TOKEN = new StateToken<AuthStateModel>('auth');
@State<AuthStateModel>({
  name: AUTH_STATE_TOKEN,
  defaults: {
    loggedIn: false,
    name: '',
    role: Role.Guest,
  },
})
@Injectable()
export class AuthState {
  constructor(private authService: AuthService) {}

  @Action(Auth.Initialize)
  initialize() {}

  @Action(Auth.Login)
  login() {
    this.authService.login();
  }

  @Action(Auth.LoggedIn)
  loggedIn(context: StateContext<AuthStateModel>, action: Auth.LoggedIn) {
    context.patchState({
      loggedIn: true,
      name: action.displayName,
      role: action.role,
    });
  }

  @Action(Auth.Logout)
  logout() {
    this.authService.logout();
  }

  @Selector()
  static loggedIn(state: AuthStateModel) {
    return state.loggedIn;
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
