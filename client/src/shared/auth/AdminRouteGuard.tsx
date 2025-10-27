import { Outlet } from 'react-router-dom';
import { useGetCurrentUser } from '@/generated/api/client';
import NotFoundPage from '../../pages/NotFound/NotFound.page';

const AdminRouteGuard = () => {
  const { data: currentUserResponse, isLoading } = useGetCurrentUser();

  const isAdmin = currentUserResponse?.responseBody?.user?.isAdmin || false;

  if (isLoading) {
    return null;
  }

  if (isAdmin) {
    return <Outlet />;
  }

  return <NotFoundPage />;
};

export default AdminRouteGuard;
