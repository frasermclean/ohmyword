export interface Environment {
  name: 'development' | 'testing' | 'production';
  apiHost: string;
  auth: {
    clientId: string;
    redirectUri: string;
    scopes: string[];
  };
}
