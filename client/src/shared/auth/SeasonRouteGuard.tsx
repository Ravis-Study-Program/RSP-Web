import { Outlet, useParams } from 'react-router-dom';
import { useGetIsUserEnrolled } from '@/generated/api/client';
import NotFoundPage from '@/pages/NotFound/NotFound.page';

const SeasonRouteGuard = () => {
  const { seasonSlug } = useParams<{ seasonSlug: string }>();

  if (!seasonSlug) {
    return <NotFoundPage />;
  }

  const {
    data: userResponse,
    isError: isLoadingUserError,
    isFetching: isFetchingUser,
    isLoading: isLoadingUser,
  } = useGetIsUserEnrolled(seasonSlug);

  if (isLoadingUser || isFetchingUser || isLoadingUserError) {
    return null;
  }

  if (!userResponse?.responseBody?.isEnrolled) {
    return <NotFoundPage />;
  }

  return <Outlet />;
};

export default SeasonRouteGuard;
