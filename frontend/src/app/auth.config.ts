import { MsalGuardConfiguration, MsalInterceptorConfiguration } from '@azure/msal-angular';
import {
  BrowserCacheLocation,
  InteractionType,
  IPublicClientApplication,
  LogLevel,
  PublicClientApplication
} from '@azure/msal-browser';

import { environment } from '@environment';

const tenantName = 'ohmywordauth';

/**
 * MSAL Instance Factory
 */
export function msalInstanceFactory(): IPublicClientApplication {
  const logLevel = environment.name !== 'development' ? LogLevel.Error : LogLevel.Info;
  return new PublicClientApplication({
    auth: {
      clientId: environment.auth.clientId,
      authority: `https://${tenantName}.b2clogin.com/${tenantName}.onmicrosoft.com/B2C_1A_SignUp_SignIn`,
      redirectUri: environment.auth.redirectUri,
      knownAuthorities: [`https://${tenantName}.b2clogin.com`],
    },
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage
    },
    system: {
      allowNativeBroker: false,
      loggerOptions: {
        loggerCallback,
        logLevel: logLevel,
        piiLoggingEnabled: false
      }
    }
  })
}

/**
 * MSAL guard configuration factory
 */
export function msalGuardConfigurationFactory(): MsalGuardConfiguration {
  return {
    interactionType: InteractionType.Redirect,
    authRequest: {
      scopes: [...environment.auth.scopes]
    },
    loginFailedRoute: 'login-failed' // TODO: Implement login failed route
  }
}

/**
 * MSAL interceptor configuration factory
 */
export function msalInterceptorConfigurationFactory(): MsalInterceptorConfiguration {
  const protectedResourceMap = new Map<string, Array<string>>();
  protectedResourceMap.set(`https://${environment.apiHost}/api/*`, environment.auth.scopes);

  return {
    interactionType: InteractionType.Redirect,
    protectedResourceMap
  }
}

function loggerCallback(logLevel: LogLevel, message: string): void {
  switch (logLevel) {
    case LogLevel.Error:
      return console.error(message);
    case LogLevel.Warning:
      return console.warn(message);
    case LogLevel.Info:
      return console.info(message);
    case LogLevel.Verbose:
      return console.debug(message);
    default:
      return console.log(message);
  }
}


