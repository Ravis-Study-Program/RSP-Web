import { useGetCurrentUser, useGetIsUserEnrolled } from '@/generated/api/client';

export const useUserAndEnrollment = (seasonSlug: string, email: string | null = null) => {
  const queryOptions = email ? { email } : {};

  const {
    data: userResponse,
    isError: isLoadingUserError,
    isFetching: isFetchingUser,
    isLoading: isLoadingUser,
  } = useGetCurrentUser(queryOptions);

  const {
    data: enrollmentsResponse,
    isError: isLoadingEnrollmentsError,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useGetIsUserEnrolled(seasonSlug);

  const user = userResponse?.responseBody?.user;
  const isAdmin = user?.isAdmin || false;
  const role =
    enrollmentsResponse?.responseBody?.role !== undefined
      ? enrollmentsResponse?.responseBody?.role
      : null;

  const isLoading =
    isLoadingUser || isFetchingUser || isLoadingEnrollments || isFetchingEnrollments;

  return {
    user,
    isAdmin,
    role,
    enrollmentId: enrollmentsResponse?.responseBody?.enrollmentId,
    isLoading,
    isError: isLoadingUserError || isLoadingEnrollmentsError,
  };
};
