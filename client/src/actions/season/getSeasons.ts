import { QueryClient } from 'react-query';
import { LoaderFunction } from 'react-router-dom';
import { Season, SeasonRole } from '../../shared/Season';

export const getSeasons = () => {
  // Hardcoded values that will be deleted and be replaced with actual API calls
  const results = [
    {
      slug: 'adl-2023-student',
      title: 'Adelaide Summer 2023 (Student)',
      role: SeasonRole.Student,
      startDate: new Date('11/1/23'),
      coordinator: 'Bob',
    },
    {
      slug: 'adl-2023-mentor',
      title: 'Adelaide Summer 2023 (Mentor)',
      role: SeasonRole.Mentor,
      startDate: new Date('12/1/23'),
      coordinator: 'Alice',
    },
    {
      slug: 'adl-2023-coordinator',
      title: 'Adelaide Summer 2023 (Coordinator)',
      role: SeasonRole.Coordinator,
      startDate: new Date('9/1/23'),
      coordinator: 'John',
    },
  ];

  return results;
};

export const getSeasonsQuery = () => ({
  queryKey: ['seasons'],
  queryFn: () => getSeasons(),
});

type QueryResult = Season[];

export const getSeasonsLoader = (queryClient: QueryClient): LoaderFunction => {
  return async (): Promise<QueryResult> => {
    const query = getSeasonsQuery();
    return (
      queryClient.getQueryData<QueryResult>(query.queryKey) ??
      queryClient.fetchQuery<QueryResult, unknown, QueryResult, string[]>(query.queryKey, query)
    );
  };
};

export default getSeasonsLoader;
