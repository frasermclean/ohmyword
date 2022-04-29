import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { InteractionStatus } from '@azure/msal-browser';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { scopes } from '../auth-config';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss'],
})
export class ToolbarComponent implements OnInit, OnDestroy {
  @Input() appName = 'Oh My Word!';
  isLoggedIn = false;
  private readonly destroyingSubject = new Subject<void>();

  constructor(private authService: MsalService, private broadcastService: MsalBroadcastService) {}

  ngOnInit(): void {
    this.broadcastService.inProgress$
      .pipe(
        filter((status) => status === InteractionStatus.None),
        takeUntil(this.destroyingSubject)
      )
      .subscribe(() => {
        this.isLoggedIn = this.authService.instance.getAllAccounts().length > 0;
      });
  }

  ngOnDestroy(): void {
    this.destroyingSubject.next(undefined);
    this.destroyingSubject.complete();
  }

  onLogin() {
    this.authService.loginRedirect({
      scopes: scopes,
    });
  }

  onLogout() {
    this.authService.logoutRedirect({
      postLogoutRedirectUri: window.location.origin,
    });
  }
}
