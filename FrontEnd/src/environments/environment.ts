import { Environment } from './environment.interface';

/**
 * Development environment settings - will be replaced by the build process
 */
export const environment: Environment = {
  name: 'development',
  apiHost: 'localhost:5001',
  auth: {
    clientId: '4c6638cf-bf1b-4c19-9948-2396f0148c59',
    redirectUri: 'http://localhost:4200',
  }
};
