import { useAuth0 } from '@auth0/auth0-react';
import { Outlet } from 'react-router-dom';
import { useGetCurrentUser } from '@/generated/api/client';
import NotFoundPage from '../../pages/NotFound/NotFound.page';

interface Auth0User {
  role?: string[];
}

const AdminRouteGuard = () => {
  const { isAuthenticated, isLoading } = useAuth0<Auth0User>();
  const { data, isLoading: isGetCurrentUserLoading } = useGetCurrentUser();
  const user = data?.responseBody?.user;

  const isAdmin = isAuthenticated && user?.isAdmin;

  if (isLoading || isGetCurrentUserLoading) {
    return null;
  }

  if (isAdmin) {
    return <Outlet />;
  }

  return <NotFoundPage />;
};

export default AdminRouteGuard;
