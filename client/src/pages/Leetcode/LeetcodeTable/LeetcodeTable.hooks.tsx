import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { mockProblems, Problem } from '../Leetcode.data';

// CREATE hook (post new problem to api)
export function useCreateProblem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (_) => {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return Promise.resolve();
    },
    onMutate: (newProblemInfo: Problem) => {
      queryClient.setQueryData(
        ['problems'],
        (prevProblems: any) =>
          [
            ...prevProblems,
            {
              ...newProblemInfo,
              id: (Math.random() + 1).toString(36).substring(7),
            },
          ] as Problem[]
      );
    },
  });
}

// READ hook (get problems from api)
export function useGetProblems() {
  return useQuery<Problem[]>({
    queryKey: ['problems'],
    queryFn: async () => {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return Promise.resolve(mockProblems);
    },
    refetchOnWindowFocus: false,
  });
}

// UPDATE hook (put problem in api)
export function useUpdateProblem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (_: Problem) => {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return Promise.resolve();
    },
    onMutate: (newProblemInfo: Problem) => {
      queryClient.setQueryData(['problems'], (prevProblems: any) =>
        prevProblems?.map((prevProblem: Problem) =>
          prevProblem.ProblemId?.toString() === newProblemInfo.ProblemId?.toString()
            ? newProblemInfo
            : prevProblem
        )
      );
    },
  });
}

// DELETE hook (delete problem in api)
export function useDeleteProblem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (_: string) => {
      await new Promise((resolve) => setTimeout(resolve, 1000));
      return Promise.resolve();
    },
    onMutate: (problemId: string) => {
      queryClient.setQueryData(['problems'], (prevProblems: Problem[]) => {
        return prevProblems?.filter(
          (problem: Problem) => problem.ProblemId?.toString() !== problemId
        );
      });
    },
  });
}
