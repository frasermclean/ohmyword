import { Injectable } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { Store } from '@ngxs/store';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Auth } from '../state/auth/auth.actions';

import { Role } from '@models/role.enum';
import { DEFAULT_DISPLAY_NAME } from '../state/auth/auth.state';

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
      .subscribe(async () => {
        const account = this.getActiveAccount();

        if (account) {
          const name = account.name || '';
          const role = (account.idTokenClaims?.role as Role) || Role.Guest;
          this.store.dispatch(new Auth.Complete(name, role));
        } else {
          this.store.dispatch(new Auth.Complete(DEFAULT_DISPLAY_NAME, Role.Guest));
        }
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

  private getActiveAccount() {
    let account = this.msalService.instance.getActiveAccount();

    if (!account && this.msalService.instance.getAllAccounts().length > 0) {
      const accounts = this.msalService.instance.getAllAccounts();
      account = accounts[0];
      this.msalService.instance.setActiveAccount(account);
    }

    return account;
  }

  public async getApiAccessToken() {
    const account = this.getActiveAccount();
    if (!account) return '';

    const tokenResult = await this.msalService.instance.acquireTokenSilent({
      account: account,
      scopes: environment.auth.scopes,
    });

    return tokenResult.accessToken;
  }
}
