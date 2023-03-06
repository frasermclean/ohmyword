import { Environment } from './environment.interface';

export const environment: Environment = {
  name: 'production',
  apiHost: 'api.ohmyword.live',
  auth: {
    clientId: 'ee95c3c0-c6f7-4675-9097-0e4d9bca14e3',
    redirectUri: 'https://ohmyword.live',
    scopes: ['https://auth.ohmyword.live/prod-api/access'],
    signUpSignInPolicy: 'B2C_1A_SignUp_SignIn',
  },
};
