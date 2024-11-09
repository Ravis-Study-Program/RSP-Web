import React from 'react';
import { useParams } from 'react-router-dom';
import { SeasonRole, useGetIsUserEnrolled } from '@/generated/api/client';
import NotFoundPage from '@/pages/NotFound/NotFound.page';

const SeasonRoleViewRouter = ({
  studentView = null,
  mentorView = null,
  coordinatorView = null,
}: SeasonRoleViewRouter) => {
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

  switch (userResponse?.responseBody?.role) {
    case SeasonRole.Student:
      return studentView ? <>{studentView}</> : <NotFoundPage />;
    case SeasonRole.Mentor:
      return mentorView ? <>{mentorView}</> : <NotFoundPage />;
    case SeasonRole.Coordinator:
      return coordinatorView ? <>{coordinatorView}</> : <NotFoundPage />;
    default:
      break;
  }

  return <NotFoundPage />;
};

type SeasonRoleViewRouter = {
  studentView?: React.ReactNode | null;
  mentorView?: React.ReactNode | null;
  coordinatorView?: React.ReactNode | null;
};

export default SeasonRoleViewRouter;
