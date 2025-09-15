import { useEffect, useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { AUTH_CLAIMS } from '@/shared/constants/auth';
import { Auth0User } from '@/shared/types/auth';
import { decodeJWT } from '@/shared/utils/jwtUtils';

export const useAuth0User = () => {
  const {
    user: auth0User,
    getAccessTokenSilently,
    isAuthenticated,
    ...rest
  } = useAuth0<Auth0User>();
  const [isAdmin, setIsAdmin] = useState(false);
  const [userId, setUserId] = useState<string | null>(null);

  useEffect(() => {
    const getClaims = async () => {
      if (!isAuthenticated) {
        setIsAdmin(false);
        setUserId(null);
        return;
      }

      try {
        const token = await getAccessTokenSilently();
        const payload = decodeJWT(token);

        if (payload) {
          setIsAdmin(payload[AUTH_CLAIMS.IS_ADMIN] ?? false);
          setUserId(payload[AUTH_CLAIMS.USER_ID] ?? null);
        }
      } catch (error) {
        setIsAdmin(false);
        setUserId(null);
      }
    };

    getClaims();
  }, [isAuthenticated, getAccessTokenSilently]);

  return {
    ...rest,
    auth0User,
    isAdmin,
    userId,
    isAuthenticated,
  };
};
