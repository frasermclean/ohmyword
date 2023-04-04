import { Component, Input, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { Auth } from '../auth/auth.actions';
import { AuthState } from '../auth/auth.state';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent {
  @Input() appName = 'Oh My Word!';
  busy$ = this.store.select(AuthState.busy);
  displayName$ = this.store.select(AuthState.displayName);
  role$ = this.store.select(AuthState.role);
  environmentName = environment.name === 'production' ? '' : environment.name;

  constructor(private store: Store) {}

  onLogin() {
    this.store.dispatch(new Auth.Login());
  }

  onLogout = () => {
    this.store.dispatch(new Auth.Logout());
  };
}
