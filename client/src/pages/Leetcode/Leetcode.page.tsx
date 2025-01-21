import { useState } from 'react';
import { Layout } from '@/components/Layout/Layout';
import {
  useGetCurrentUser,
  useGetIsCurrentUserEnrolled,
  useListProblemAttempt,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { LeetcodeTable } from './LeetcodeTable/LeetcodeTable';
import { ProblemAttemptsGraph } from './ProblemAttemptsGraph/ProblemAttemptsGraph';

export default function LeetcodePage() {
  const [isLeetcode, _] = useState(true);
  const { seasonSlug } = useSeasonSlug();

  const { data: currentUserResponse } = useGetCurrentUser();
  const email = currentUserResponse?.responseBody?.user.email ?? '';

  // TODO: Handle error and loading states using skeleton
  // TODO: Add Custom Problems support
  const { data: userResponse } = useGetIsCurrentUserEnrolled({ seasonSlug });

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useListProblemAttempt(
    {
      EnrollmentId: userResponse?.responseBody?.enrollmentId || undefined,
      IncludeCustom: !isLeetcode,
      IncludeLeetcode: isLeetcode,
      Email: email,
    },
    { query: { enabled: email !== '' } }
  );

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
