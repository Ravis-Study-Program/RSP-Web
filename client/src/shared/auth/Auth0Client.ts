import { Auth0Client } from '@auth0/auth0-spa-js';

export const auth0Client = new Auth0Client({
  domain: import.meta.env.VITE_APP_AUTH0_DOMAIN as string,
  clientId: import.meta.env.VITE_APP_AUTH0_CLIENT_ID as string,
  authorizationParams: {
    redirect_uri: import.meta.env.VITE_APP_AUTH0_REDIRECT_URI as string,
    audience: import.meta.env.VITE_APP_AUTH0_AUDIENCE as string,
  },
});
