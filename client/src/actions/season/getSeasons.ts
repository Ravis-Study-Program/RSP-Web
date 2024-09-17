import { QueryClient } from '@tanstack/react-query';
import { LoaderFunction } from 'react-router-dom';
import { Season, SeasonRole } from '../../shared/Season';

export const getSeasons = (): Season[] => {
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
  queryFn: getSeasons, // Use the getSeasons function directly
});

type QueryResult = Season[];

export const getSeasonsLoader = (queryClient: QueryClient): LoaderFunction => {
  return async (): Promise<QueryResult> => {
    const query = getSeasonsQuery();

    const cachedData = queryClient.getQueryData<QueryResult>(query.queryKey);
    if (cachedData) {
      return cachedData;
    }

    const fetchedData = await queryClient.fetchQuery<QueryResult>({
      queryKey: query.queryKey,
      queryFn: query.queryFn,
    });

    return fetchedData;
  };
};

export default getSeasonsLoader;
