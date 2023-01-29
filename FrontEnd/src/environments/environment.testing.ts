import { Environment } from './environment.interface';

export const environment: Environment = {
  name: 'testing',
  apiHost: 'test.api.ohmyword.live',
  auth: {
    clientId: '', // TODO: Add client ID
    redirectUri: 'https://test.ohmyword.live',
  }
};
