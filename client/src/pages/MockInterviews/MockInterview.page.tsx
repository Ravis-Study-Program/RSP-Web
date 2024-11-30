import { Layout } from '@/components/Layout/Layout';
import { useGetIsUserEnrolled, useGetMockInterviews } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { MockInterviewTable } from './MockInterviewTable/MockInterviewTable';

export default function MockInterviewPage() {
  const { seasonSlug } = useSeasonSlug();

  // TODO: Handle error and loading states using skeleton
  // TODO: Add Custom Mock Interviews support
  const { data: userResponse } = useGetIsUserEnrolled(seasonSlug);

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useGetMockInterviews({
    enrollmentId: userResponse?.responseBody?.enrollmentId || undefined,
    includeCustom: true,
    includeLeetcode: true,
    includeBehavioural: true,
    email: userResponse?.responseBody?.email,
  });

  return (
    <Layout>
      <MockInterviewTable
        refetchMockInterviews={refetchMockInterviews}
        mockInterviews={mockInterviewsResponse?.responseBody?.mockInterviews}
        enrollmentId={userResponse?.responseBody?.enrollmentId || ''}
        enableEditing
      />
    </Layout>
  );
}
