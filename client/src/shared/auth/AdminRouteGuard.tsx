import { useAuth0 } from '@auth0/auth0-react';
import { Outlet } from 'react-router-dom';
import NotFoundPage from '../../pages/NotFound/NotFound.page';
import { useUserAndEnrollment } from '../hooks/useUserAndEnrollment';

interface Auth0User {
  role?: string[];
}

const AdminRouteGuard = () => {
  const { isAuthenticated, isLoading } = useAuth0<Auth0User>();
  const { user } = useUserAndEnrollment('');

  const isAdmin = isAuthenticated && user?.isAdmin;

  if (isLoading) {
    return null;
  }

  if (isAdmin) {
    return <Outlet />;
  }

  return <NotFoundPage />;
};

export default AdminRouteGuard;
