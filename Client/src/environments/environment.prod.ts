import { LogLevel } from '@azure/msal-browser';

export const environment = {
  production: true,
  api: {
    baseUrl: `/api`,
    hubUrl: `/game`,
  },
  authLogLevel: LogLevel.Error,
};
