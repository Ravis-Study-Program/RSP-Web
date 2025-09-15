import { Outlet } from 'react-router-dom';
import { useAuth0User } from '@/shared/hooks/useAuth0User';
import NotFoundPage from '../../pages/NotFound/NotFound.page';

const AdminRouteGuard = () => {
  const { isAuthenticated, isLoading, isAdmin: auth0IsAdmin } = useAuth0User();

  const isAdmin = isAuthenticated && auth0IsAdmin;

  if (isLoading) {
    return null;
  }

  if (isAdmin) {
    return <Outlet />;
  }

  return <NotFoundPage />;
};

export default AdminRouteGuard;
