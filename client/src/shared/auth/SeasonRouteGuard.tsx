import NotFoundPage from '@/pages/NotFound/NotFound.page';
import { Outlet, useParams } from 'react-router-dom';
import { useUserAndEnrollment } from '../hooks/useUserAndEnrollment';

const SeasonRouteGuard = () => {
  const { seasonSlug } = useParams<{ seasonSlug: string }>();

  if (!seasonSlug) {
    return <NotFoundPage />;
  }

  const { user, isAdmin, role, isLoading } = useUserAndEnrollment(seasonSlug);

  if (isLoading) {
    return null;
  }

  // Admin can bypass all seasons without a role
  if (isAdmin) {
    return <Outlet />;
  }

  // Non-admin must have a role in the season
  if (user === null || role === null) {
    return <NotFoundPage />;
  }

  return <Outlet />;
};

export default SeasonRouteGuard;
