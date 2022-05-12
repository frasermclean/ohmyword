import { LogLevel } from "@azure/msal-browser";

/**
 * Development environment settings - will be replaced by the build process
 */
export const environment = {
  production: false,
  api: {
    baseUrl: 'https://localhost:5001/api',
    hubUrl: 'https://localhost:5001/hub',
  },
  authLogLevel: LogLevel.Info
};
