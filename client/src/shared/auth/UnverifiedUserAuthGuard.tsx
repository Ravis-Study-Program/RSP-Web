import { useAuth0 } from '@auth0/auth0-react';
import { Navigate, Outlet } from 'react-router-dom';
import NotFoundPage from '@/pages/NotFound/NotFound.page';

const UnverifiedUserAuthGuard = () => {
  const { isAuthenticated, isLoading, user } = useAuth0();

  if (isLoading) {
    return null;
  }

  if (!isAuthenticated) {
    return <NotFoundPage />;
  }

  if (user?.email_verified) {
    return <Navigate to="/" />;
  }

  return <Outlet />;
};

export default UnverifiedUserAuthGuard;
