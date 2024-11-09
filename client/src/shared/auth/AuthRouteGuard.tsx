import { useAuth0 } from '@auth0/auth0-react';
import { Outlet } from 'react-router-dom';
import  NotFoundPage  from '@/pages/NotFound/NotFound.page';

const AuthRouteGuard = () => {
  const { isAuthenticated, isLoading } = useAuth0();

  if (isLoading) {
    return null;
  }

  if (isAuthenticated) {
    return <Outlet />;
  }

  return <NotFoundPage />;
};

export default AuthRouteGuard;
