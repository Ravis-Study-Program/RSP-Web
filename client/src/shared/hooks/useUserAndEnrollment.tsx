import { useGetCurrentUser, useGetIsCurrentUserEnrolled, useGetUser } from '@/generated/api/client';

export const useUserAndEnrollment = (seasonSlug: string, email: string | null = null) => {
  const {
    data: currentUserResponse,
    isLoading: isCurrentUserLoading,
    isError: isCurrentUserError,
  } = useGetCurrentUser();

  const currentUserEmail = currentUserResponse?.responseBody?.user?.email ?? null;
  const userEmail = email ?? currentUserEmail;

  // When an email is provided explicitly, enable fetching the user.
  // Otherwise, rely on the current user query.
  const isUserQueryEnabled = Boolean(email);

  const {
    data: userResponse,
    isError: isUserError,
    isFetching: isUserFetching,
    isLoading: isUserLoading,
  } = useGetUser({ Email: userEmail || '' }, { query: { enabled: isUserQueryEnabled } });

  // Prioritize the fetched user if available (when querying with an email);
  // Otherwise, fall back to the current user data.
  const user = userResponse?.responseBody?.user || currentUserResponse?.responseBody?.user;
  const isAdmin = user?.isAdmin ?? false;

  const {
    data: enrollmentsResponse,
    isError: isEnrollmentError,
    isFetching: isEnrollmentFetching,
    isLoading: isEnrollmentLoading,
  } = useGetIsCurrentUserEnrolled({ seasonSlug });

  const role = enrollmentsResponse?.responseBody?.role ?? null;
  const enrollmentId = enrollmentsResponse?.responseBody?.enrollmentId;
  const isLoading =
    isCurrentUserLoading ||
    isUserLoading ||
    isUserFetching ||
    isEnrollmentLoading ||
    isEnrollmentFetching;
  const isError = isCurrentUserError || isUserError || isEnrollmentError;

  return {
    user,
    isAdmin,
    role,
    enrollmentId,
    isLoading,
    isError,
  };
};
