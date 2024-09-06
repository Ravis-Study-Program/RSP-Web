import { useAuth0 } from '@auth0/auth0-react';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

const ProtectedRouteGuard = () => {
  const { isAuthenticated, isLoading } = useAuth0();
  const location = useLocation();

  if (!isLoading && isAuthenticated) {
    return <Outlet />;
  }

  return <Navigate to="/login" state={{ from: location }} replace />;
};

export default ProtectedRouteGuard;
