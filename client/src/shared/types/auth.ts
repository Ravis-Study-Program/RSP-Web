import { AUTH_CLAIMS } from '@/shared/constants/auth';

export interface Auth0User {
  role?: string[];
  [AUTH_CLAIMS.IS_ADMIN]?: boolean;
  [AUTH_CLAIMS.USER_ID]?: string;
  email?: string;
  nickname?: string;
}
