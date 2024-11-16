import { useState } from 'react';
import { Layout } from '@/components/Layout/Layout';
import { useGetIsUserEnrolled, useGetProblemAttempts } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { LeetcodeTable } from './LeetcodeTable/LeetcodeTable';
import { ProblemAttemptsGraph } from './ProblemAttemptsGraph/ProblemAttemptsGraph';

export default function LeetcodePage() {
  const [isLeetcode, _] = useState(true);
  const { seasonSlug } = useSeasonSlug();

  // TODO: Handle error and loading states using skeleton
  // TODO: Add Custom Problems support
  const { data: userResponse } = useGetIsUserEnrolled(seasonSlug);

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useGetProblemAttempts({
    enrollmentId: userResponse?.responseBody?.enrollmentId || undefined,
    includeCustom: !isLeetcode,
    includeLeetcode: isLeetcode,
  });

  return (
    <Layout>
      <ProblemAttemptsGraph
        problemAttempts={problemAttemptsResponse?.responseBody?.problemAttempts}
      />
      <LeetcodeTable
        refetchProblemAttempts={refetchProblemAttempts}
        problemAttempts={problemAttemptsResponse?.responseBody?.problemAttempts}
        enrollmentId={userResponse?.responseBody?.enrollmentId || ''}
        enableEditing
      />
    </Layout>
  );
}
