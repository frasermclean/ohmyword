import { Environment } from './environment.interface';

export const environment: Environment = {
  name: 'testing',
  apiHost: 'test.api.ohmyword.live',
  auth: {
    clientId: '1f427277-e4b2-4f9b-97b1-4f47f4ff03c0',
    redirectUri: 'https://test.ohmyword.live',
    scopes: ['https://auth.ohmyword.live/test-api/access'],
    signUpSignInPolicy: 'B2C_1A_SignUp_SignIn_DevTest',
  },
};
