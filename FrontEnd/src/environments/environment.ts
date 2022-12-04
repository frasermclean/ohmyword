import { Environment } from './environment.interface';

/**
 * Development environment settings - will be replaced by the build process
 */
export const environment: Environment = {
  name: 'development',
  apiBaseUrl: 'http://localhost:5000',
};
