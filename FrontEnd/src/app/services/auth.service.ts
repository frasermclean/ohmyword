import { Injectable } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { Store } from '@ngxs/store';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Auth } from '../auth/auth.actions';

import { Role } from '../models/role.enum';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly destroyingSubject = new Subject<void>();

  constructor(
    private store: Store,
    private msalService: MsalService,
    private msalBroadcastService: MsalBroadcastService
  ) {
    this.msalBroadcastService.inProgress$
      .pipe(
        filter((status) => status === InteractionStatus.None),
        takeUntil(this.destroyingSubject)
      )
      .subscribe(() => {
        const accounts = this.msalService.instance.getAllAccounts();
        if (accounts.length === 0) return;

        const account = accounts[accounts.length - 1];
        const name = account.name || '';
        const role = account.idTokenClaims?.role as Role || Role.Guest;

        this.store.dispatch(new Auth.LoggedIn(name, role));
      });
  }

  login() {
    this.msalService.loginRedirect({
      scopes: environment.auth.scopes,
    });
  }

  logout() {
    this.msalService.logoutRedirect({
      postLogoutRedirectUri: window.location.origin,
    });
  }
}
