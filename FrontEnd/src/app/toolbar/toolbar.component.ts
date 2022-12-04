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
export class ToolbarComponent implements OnInit {
  @Input() appName = 'Oh My Word!';
  loggedIn$ = this.store.select(AuthState.loggedIn);
  displayName$ = this.store.select(AuthState.displayName);
  role$ = this.store.select(AuthState.role);
  environmentName = environment.name === 'production' ? '' : environment.name;

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.store.dispatch(new Auth.Initialize());
  }

  onLogin() {
    this.store.dispatch(new Auth.Login());
  }

  onLogout = () => {
    this.store.dispatch(new Auth.Logout());
  };
}
