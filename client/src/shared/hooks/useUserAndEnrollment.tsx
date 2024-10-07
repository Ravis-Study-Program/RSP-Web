import { useGetCurrentUser, useGetIsUserEnrolled } from '@/generated/api/client';

export const useUserAndEnrollment = (seasonSlug: string) => {
  const {
    data: userResponse,
    isError: isLoadingUserError,
    isFetching: isFetchingUser,
    isLoading: isLoadingUser,
  } = useGetCurrentUser();

  const {
    data: enrollmentsResponse,
    isError: isLoadingEnrollmentsError,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useGetIsUserEnrolled(seasonSlug);

  const user = userResponse?.responseBody?.user;
  const isAdmin = user?.isAdmin || false;
  const roleName = enrollmentsResponse?.responseBody?.role?.name || '';

  const isLoading =
    isLoadingUser || isFetchingUser || isLoadingEnrollments || isFetchingEnrollments;

  return {
    user,
    isAdmin,
    roleName,
    isLoading,
    isError: isLoadingUserError || isLoadingEnrollmentsError,
  };
};
