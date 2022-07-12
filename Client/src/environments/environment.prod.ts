import { LogLevel } from '@azure/msal-browser';

export const environment = {
  production: true,
  api: {
    baseUrl: `/api`,
    hubUrl: `/hub`,
  },
  authLogLevel: LogLevel.Error,
};
