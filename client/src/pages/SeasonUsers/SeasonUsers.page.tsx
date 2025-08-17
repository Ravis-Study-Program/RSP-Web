import { Grid } from '@mantine/core';
import {
  useFilterUsers,
  UserCards,
  UserFilterPanel,
  UserSkeletonCards,
} from '@/components/UserCards/UserCards';
import { useGetEnrollmentUsers } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';

export default function SeasonUsersPage() {
  const { seasonSlug } = useSeasonSlug();
  const {
    data: enrollmentUsersResponse,
    isError: isLoadingEnrollmentUsersError,
    isFetching: isFetchingEnrollmentUsers,
    isLoading: isLoadingEnrollmentUsers,
  } = useGetEnrollmentUsers({ SeasonSlug: seasonSlug });

  const users = enrollmentUsersResponse?.responseBody?.enrollmentUsers || [];
  const {
    selectedNames,
    setSelectedNames,
    selectedRoles,
    setSelectedRoles,
    selectedPromotions,
    setSelectedPromotions,
    filtered: filteredUsers,
  } = useFilterUsers(users);
  return (
    <>
      <UserFilterPanel
        allUsers={users}
        selectedNames={selectedNames}
        onChangeName={setSelectedNames}
        selectedRoles={selectedRoles}
        onChangeRole={setSelectedRoles}
        selectedPromotions={selectedPromotions}
        onChangePromotion={setSelectedPromotions}
        filteredCount={filteredUsers.length}
      />

      <Grid mt="lg" gutter={{ base: 'md', xs: 'md', md: 'xl', xl: 50 }}>
        {!isLoadingEnrollmentUsers &&
        !isFetchingEnrollmentUsers &&
        !isLoadingEnrollmentUsersError ? (
          <UserCards users={filteredUsers} />
        ) : (
          <UserSkeletonCards />
        )}
      </Grid>
    </>
  );
}
