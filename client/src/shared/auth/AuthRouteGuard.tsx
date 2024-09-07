import { useAuth0 } from '@auth0/auth0-react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

const AuthRouteGuard = () => {
  const { isAuthenticated, isLoading } = useAuth0();
  const location = useLocation();

  if (isLoading) {
    return null;
  }

  if (isAuthenticated) {
    return <Outlet />;
  }

  return <Navigate to="/login" state={{ from: location }} replace />;
};

export default AuthRouteGuard;
