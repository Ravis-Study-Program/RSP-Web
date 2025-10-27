import {
  GetUserParams,
  useGetCurrentUser,
  useGetIsCurrentUserEnrolled,
  useGetUser,
} from '@/generated/api/client';

export const useUserAndEnrollment = (
  seasonSlug: string,
  userId: string | null = null,
  slug: string | null = null
) => {
  const {
    data: currentUserResponse,
    isLoading: isCurrentUserLoading,
    isError: isCurrentUserError,
  } = useGetCurrentUser();

  const currentUserId = currentUserResponse?.responseBody?.user?.userId ?? userId;

  // Prioritize slug if available
  const params: GetUserParams = {};
  if (slug != null) {
    params.Slug = slug;
  } else if (currentUserId != null) {
    params.UserId = currentUserId;
  }

  // When an email is provided explicitly, enable fetching the user.
  // Otherwise, rely on the current user query.
  const isUserQueryEnabled = Boolean(currentUserId);

  const {
    data: userResponse,
    isError: isUserError,
    isFetching: isUserFetching,
    isLoading: isUserLoading,
  } = useGetUser(params, { query: { enabled: isUserQueryEnabled } });

  // Prioritize the fetched user if available (when querying with an email);
  // Otherwise, fall back to the current user data.
  const user = userResponse?.responseBody?.user || currentUserResponse?.responseBody?.user;

  const {
    data: enrollmentsResponse,
    isError: isEnrollmentError,
    isFetching: isEnrollmentFetching,
    isLoading: isEnrollmentLoading,
  } = useGetIsCurrentUserEnrolled({ seasonSlug });

  const role = enrollmentsResponse?.responseBody?.role ?? null;
  const enrollmentId = enrollmentsResponse?.responseBody?.enrollmentId;
  const seasonId = enrollmentsResponse?.responseBody?.seasonId;
  const isLoading =
    isCurrentUserLoading ||
    isUserLoading ||
    isUserFetching ||
    isEnrollmentLoading ||
    isEnrollmentFetching;
  const isError = isCurrentUserError || isUserError || isEnrollmentError;

  return {
    user,
    isAdmin: user?.isAdmin || false,
    seasonId,
    role,
    enrollmentId,
    isLoading,
    isError,
    userId: user?.userId || '',
  };
};
