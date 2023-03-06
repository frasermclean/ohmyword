import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType, BrowserCacheLocation, LogLevel } from '@azure/msal-browser';

import { environment } from 'src/environments/environment';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;
const tenantName = 'ohmywordauth';

/**
 * MSAL client application
 */
export const msalInstance = new PublicClientApplication({
  auth: {
    clientId: environment.auth.clientId,
    authority: `https://${tenantName}.b2clogin.com/${tenantName}.onmicrosoft.com/${environment.auth.signUpSignInPolicy}`,
    redirectUri: environment.auth.redirectUri,
    knownAuthorities: [`https://${tenantName}.b2clogin.com`],
  },
  cache: {
    cacheLocation: BrowserCacheLocation.LocalStorage,
    storeAuthStateInCookie: isIE,
  },
  system: {
    loggerOptions: {
      loggerCallback: (logLevel, message, containsPii) => {
        console.log(message);
      },
      logLevel: environment.name !== 'development' ? LogLevel.Error : LogLevel.Warning,
      piiLoggingEnabled: false,
    },
  },
});

/**
 * MSAL guard configuration
 */
export const guardConfig: MsalGuardConfiguration = {
  interactionType: InteractionType.Redirect,
};

/**
 * MSAL interceptor configuration
 */
export const interceptorConfig: MsalInterceptorConfiguration = {
  interactionType: InteractionType.Redirect,
  protectedResourceMap: new Map([[`https://${environment.apiHost}/api/*`, environment.auth.scopes]]),
};
