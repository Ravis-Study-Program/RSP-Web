import { Outlet, useLoaderData } from 'react-router-dom';
import { NotFoundPage } from '@/pages/NotFound/NotFound.page';
import { SeasonRole } from '../Season';

const SeasonRouteGuard = () => {
  const role = useLoaderData() as SeasonRole;

  if (role === SeasonRole.Unregistered) {
    return <NotFoundPage />;
  }

  return <Outlet />;
};

export default SeasonRouteGuard;
