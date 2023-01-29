import { Environment } from './environment.interface';

export const environment: Environment = {
  name: 'production',
  apiHost: 'api.ohmyword.live',
  auth: {
    clientId: '', // TODO: Add client ID
    redirectUri: 'https://ohmyword.live',
  }
};
