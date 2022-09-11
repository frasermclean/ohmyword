import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType, BrowserCacheLocation, LogLevel } from '@azure/msal-browser';

import { environment } from 'src/environments/environment';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

const authorityDomain = 'ohmywordb2c.b2clogin.com';
const policyNames = {
  signUpSignIn: 'B2C_1A_signup_signin',
};

export const scopes = [
  'https://ohmywordb2c.onmicrosoft.com/api/access',
]

/**
 * MSAL client application
 */
export const msalInstance = new PublicClientApplication({
  auth: {
    clientId: '14801bf2-30c2-4d1a-8b65-d0fc34aacc5f',
    authority: `https://${authorityDomain}/ohmywordb2c.onmicrosoft.com/${policyNames.signUpSignIn}`,
    redirectUri: '/',
    knownAuthorities: [authorityDomain],
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
      logLevel: environment.production ? LogLevel.Error : LogLevel.Warning,
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
  protectedResourceMap: new Map([
    [`${environment.api.baseUrl}/*`, [scopes[0]]],
  ]),
};
