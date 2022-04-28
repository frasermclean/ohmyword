import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import { PublicClientApplication, InteractionType, BrowserCacheLocation } from '@azure/msal-browser';

/**
 * MSAL application configuration
 */
export const msalInstance = new PublicClientApplication({
  auth: {
    clientId: 'efc19f5d-3291-4bfc-8890-63d851954414',
    authority: 'https://ohmywordlive.b2clogin.com/ohmyword.live/B2C_1_signup_signin',
    redirectUri: 'http://localhost:4200',
    knownAuthorities: ['https://ohmywordlive.b2clogin.com/ohmyword.live/B2C_1_signup_signin'],
  },
  cache: {
    cacheLocation: BrowserCacheLocation.LocalStorage,
    storeAuthStateInCookie: true, // set to true for IE 11
  },
  system: {
    loggerOptions: {
      loggerCallback: () => {},
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
