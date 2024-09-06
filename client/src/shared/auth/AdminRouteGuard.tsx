import { useAuth0 } from '@auth0/auth0-react';
import { Outlet } from 'react-router-dom';
import NotFound from '../../pages/NotFound.page';

interface Auth0User {
  role?: string[];
}

const AdminRouteGuard = () => {
  const { isAuthenticated, isLoading, user } = useAuth0<Auth0User>();

  const isAdmin = isAuthenticated && user?.role?.includes('Admin');

  if (isLoading) {
    return null;
  }

  if (isAdmin) {
    return <Outlet />;
  }

  return <NotFound />;
};

export default AdminRouteGuard;
