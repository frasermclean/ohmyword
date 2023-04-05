import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';

// ngxs modules
import { getActionTypeFromInstance, NgxsModule } from '@ngxs/store';
import { NgxsLoggerPluginModule } from '@ngxs/logger-plugin';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';
import { NgxsRouterPluginModule } from '@ngxs/router-plugin';

// angular material modules
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

import { MsalModule, MsalGuard, MsalInterceptor, MsalRedirectComponent } from '@azure/msal-angular';

import { msalInstance, guardConfig, interceptorConfig } from './auth.config';

import { environment } from 'src/environments/environment';
import { AppComponent } from './app.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { Game } from '@state/game/game.actions';
import { AuthState } from '@state/auth/auth.state';

const routes: Routes = [
  {
    path: 'admin',
    loadChildren: () => import('./admin/admin.module').then((m) => m.AdminModule),
    canActivate: [MsalGuard],
  },
  { path: 'game', loadChildren: () => import('./game/game.module').then((m) => m.GameModule) },
  { path: '**', redirectTo: 'game', pathMatch: 'full' },
];

@NgModule({
  declarations: [AppComponent, ToolbarComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    RouterModule.forRoot(routes),
    NgxsModule.forRoot([AuthState], {
      developmentMode: environment.name === 'development',
    }),
    NgxsRouterPluginModule.forRoot(),
    NgxsLoggerPluginModule.forRoot({
      disabled: environment.name !== 'development', // disable logger in production
      filter: (action) => {
        const actionType = getActionTypeFromInstance(action);
        if (!actionType) return true;
        const filteredActions = [Game.LetterHintReceived.type];
        return filteredActions.indexOf(actionType) === -1;
      },
    }),
    NgxsReduxDevtoolsPluginModule.forRoot({
      disabled: environment.name !== 'development', // disable devtools in production
    }),
    MatToolbarModule,
    MatButtonModule,
    MsalModule.forRoot(msalInstance, guardConfig, interceptorConfig),
  ],
  providers: [{ provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true }, MsalGuard],
  bootstrap: [AppComponent, MsalRedirectComponent],
})
export class AppModule {}
