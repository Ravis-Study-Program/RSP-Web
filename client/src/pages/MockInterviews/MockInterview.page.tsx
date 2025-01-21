import { Layout } from '@/components/Layout/Layout';
import { useGetIsCurrentUserEnrolled, useListMockInterview } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { MockInterviewTable } from './MockInterviewTable/MockInterviewTable';

export default function MockInterviewPage() {
  const { seasonSlug } = useSeasonSlug();

  // TODO: Handle error and loading states using skeleton
  // TODO: Add Custom Mock Interviews support
  const { data: userResponse } = useGetIsCurrentUserEnrolled({ seasonSlug });
  const email = userResponse?.responseBody?.email ?? '';

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useListMockInterview(
    {
      EnrollmentId: userResponse?.responseBody?.enrollmentId || undefined,
      IncludeCustom: true,
      IncludeLeetcode: true,
      IncludeBehavioural: true,
      Email: email,
    },
    { query: { enabled: email !== '' } }
  );

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
