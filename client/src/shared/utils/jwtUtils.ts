import { AUTH_CLAIMS } from '@/shared/constants/auth';

interface JWTPayload {
  [AUTH_CLAIMS.IS_ADMIN]?: boolean;
  [AUTH_CLAIMS.USER_ID]?: string;
  email?: string;
  [key: string]: any;
}

export function decodeJWT(token: string): JWTPayload | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => `%${`00${c.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch (error) {
    return null;
  }
}
