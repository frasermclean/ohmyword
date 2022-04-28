import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType, BrowserCacheLocation } from '@azure/msal-browser';

import { environment } from 'src/environments/environment';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

const authorityDomain = 'ohmywordlive.b2clogin.com';
const policyNames = {
  signUpSignIn: 'B2C_1_signup_signin',
};

/**
 * MSAL client application
 */
export const msalInstance = new PublicClientApplication({
  auth: {
    clientId: 'efc19f5d-3291-4bfc-8890-63d851954414',
    authority: `https://${authorityDomain}/ohmyword.live/${policyNames.signUpSignIn}`,
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
      logLevel: environment.authLogLevel,
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
  protectedResourceMap: new Map([]),
};
