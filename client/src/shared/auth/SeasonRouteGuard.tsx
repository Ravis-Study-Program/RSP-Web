import { Navigate, Outlet, useLoaderData, useLocation } from 'react-router-dom';
import { SeasonRole } from '../Season';

const SeasonRouteGuard = () => {
  const role = useLoaderData() as SeasonRole;
  const location = useLocation();

  if (role === SeasonRole.Unregistered) {
    return <Navigate to="/seasons" state={{ from: location }} replace />;
  }

  return <Outlet />;
};

export default SeasonRouteGuard;
