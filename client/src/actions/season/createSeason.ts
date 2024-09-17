import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Season } from '../../shared/Season';

export const createSeason = (season: Season) => {
  // This will be replaced with actual API calls in the future.
  return Promise.resolve(season);
};

const useCreateSeasonMutation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (season: Season) => {
      return createSeason(season);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['seasons'],
      });
    },
  });
};

export default useCreateSeasonMutation;
