export const AUTH_DOMAIN = 'https://app.rsp.org.au/';

export const AUTH_CLAIMS = {
  IS_ADMIN: `${AUTH_DOMAIN}isAdmin`,
  USER_ID: `${AUTH_DOMAIN}userId`,
} as const;
