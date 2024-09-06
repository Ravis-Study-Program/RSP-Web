import { QueryClient } from 'react-query';
import { LoaderFunction, LoaderFunctionArgs } from 'react-router-dom';
import { SeasonRole } from '../../shared/Season';

export const getSeasonRole = (seasonSlug: string) => {
  // Hardcoded values that will be deleted and be replaced with actual API calls
  const base = 'adl-2023';
  const studentSeason = `${base}-student`;
  const mentorSeason = `${base}-mentor`;
  const coordinatorSeason = `${base}-coordinator`;

  switch (seasonSlug) {
    case studentSeason:
      return SeasonRole.Student;
    case mentorSeason:
      return SeasonRole.Mentor;
    case coordinatorSeason:
      return SeasonRole.Coordinator;
    default:
      return SeasonRole.Unregistered;
  }
};

export const getSeasonRoleQuery = (seasonSlug: string) => ({
  queryKey: ['season', 'role', seasonSlug],
  queryFn: () => getSeasonRole(seasonSlug),
});

type QueryResult = SeasonRole;

export const getSeasonRoleLoader = (queryClient: QueryClient): LoaderFunction => {
  return async ({ params }: LoaderFunctionArgs<{ seasonSlug: string }>): Promise<QueryResult> => {
    if (params.seasonSlug === undefined) {
      throw new Error('seasonSlug parameter is missing');
    }

    const query = getSeasonRoleQuery(params.seasonSlug);
    return (
      queryClient.getQueryData<QueryResult>(query.queryKey) ??
      queryClient.fetchQuery<QueryResult, unknown, QueryResult, string[]>(query.queryKey, query)
    );
  };
};

export default getSeasonRoleLoader;
