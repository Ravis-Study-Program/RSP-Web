import { Outlet } from 'react-router-dom';
import { useGetCurrentUser } from '@/generated/api/client';
import NotFoundPage from '../../pages/NotFound/NotFound.page';

const GraduateRouteGuard = () => {
  const { data: currentUserResponse, isLoading } = useGetCurrentUser();

  const isGraduate = currentUserResponse?.responseBody?.user?.isGraduate || false;

  if (isLoading) {
    return null;
  }

  if (isGraduate) {
    return <Outlet />;
  }

  return <NotFoundPage />;
};

export default GraduateRouteGuard;
