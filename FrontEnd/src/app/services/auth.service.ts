import { Injectable } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { Store } from '@ngxs/store';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { Auth } from '../auth/auth.actions';
import { scopes } from '../auth/auth.config';
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

        const account = accounts[0];
        const name = account.name || '';
        const role = account.localAccountId === '377398a2-f436-4f5f-9076-89cca8435f34' || // TODO: replace this with token claim
          account.localAccountId === 'f803a3a2-9edc-485e-98f9-3b6fea804d91' ? Role.Admin : Role.User;

        this.store.dispatch(new Auth.LoggedIn(name, role));
      });
  }

  login() {
    this.msalService.loginRedirect({
      scopes: scopes,
    });
  }

  logout() {
    this.msalService.logoutRedirect({
      postLogoutRedirectUri: window.location.origin,
    });
  }
}
