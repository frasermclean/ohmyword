import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';

// ngxs modules
import { getActionTypeFromInstance, NgxsModule } from '@ngxs/store';
import { NgxsLoggerPluginModule } from '@ngxs/logger-plugin';
import { NgxsReduxDevtoolsPluginModule } from '@ngxs/devtools-plugin';
import { NgxsRouterPluginModule } from '@ngxs/router-plugin';

import {
  MSAL_GUARD_CONFIG,
  MSAL_INSTANCE,
  MSAL_INTERCEPTOR_CONFIG,
  MsalBroadcastService,
  MsalGuard,
  MsalInterceptor,
  MsalModule,
  MsalRedirectComponent,
  MsalService
} from '@azure/msal-angular';
import { msalGuardConfigurationFactory, msalInstanceFactory, msalInterceptorConfigurationFactory } from './auth.config';

import { environment } from '@environment';
import { CoreModule } from './core/core.module';
import { AppComponent } from './app.component';
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
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    RouterModule.forRoot(routes),
    MsalModule,
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
    CoreModule,
  ],
  providers: [
    MsalService,
    MsalBroadcastService,
    MsalGuard,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    {
      provide: MSAL_INSTANCE,
      useFactory: msalInstanceFactory
    },
    {
      provide: MSAL_GUARD_CONFIG,
      useFactory: msalGuardConfigurationFactory
    },
    {
      provide: MSAL_INTERCEPTOR_CONFIG,
      useFactory: msalInterceptorConfigurationFactory
    }
  ],
  bootstrap: [AppComponent, MsalRedirectComponent],
})
export class AppModule {}
