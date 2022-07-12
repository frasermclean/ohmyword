import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';

// ngxs modules
import { getActionTypeFromInstance, NgxsModule } from '@ngxs/store';
import { NgxsLoggerPluginModule } from '@ngxs/logger-plugin';

// angular material modules
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

import { MsalModule, MsalGuard, MsalInterceptor, MsalRedirectComponent } from '@azure/msal-angular';

import { msalInstance, guardConfig, interceptorConfig } from './auth-config';

import { environment } from 'src/environments/environment';
import { AppComponent } from './app.component';
import { ToolbarComponent } from './toolbar/toolbar.component';
import { Game } from './game/game.actions';


const routes: Routes = [
  { path: 'admin', loadChildren: () => import('./admin/admin.module').then((m) => m.AdminModule), canActivate: [MsalGuard] },
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
    NgxsModule.forRoot([], { 
      developmentMode: !environment.production
    }),
    NgxsLoggerPluginModule.forRoot({
      disabled: environment.production, // disable logger in production
      filter: (action) => {
        const actionType = getActionTypeFromInstance(action);
        if (!actionType) return true;
        const filteredActions = [
          Game.LetterHintReceived.type
        ]
        return filteredActions.indexOf(actionType) === -1;
      }
    }),
    MatToolbarModule,
    MatButtonModule,
    MsalModule.forRoot(msalInstance, guardConfig, interceptorConfig),
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: MsalInterceptor, multi: true }, 
    MsalGuard
  ],
  bootstrap: [AppComponent, MsalRedirectComponent],
})
export class AppModule {}
