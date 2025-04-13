import { useMemo, useState } from 'react';
import { Flex, Group, MultiSelect, SegmentedControl, Text } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import {
  useGetCurrentUserMentees,
  useGetIsCurrentUserEnrolled,
  useListMockInterview,
  useListProblemAttempt,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { createOptionsFilter } from '@/shared/table/globalFilters';
import { LeetcodeTable } from '../Leetcode/LeetcodeTable/LeetcodeTable';
import { MockInterviewTable } from '../MockInterviews/MockInterviewTable/MockInterviewTable';
import { MenteesTable } from './MenteesTable';
import classes from './Mentees.module.css';

export default function MenteesPage() {
  const { seasonSlug } = useSeasonSlug();
  const { data: userResponse } = useGetIsCurrentUserEnrolled({ seasonSlug });
  const email = userResponse?.responseBody?.email ?? '';
  const { enrollmentId, seasonId } = useUserAndEnrollment(seasonSlug, email);
  const [section, setSection] = useState<'Portfolio' | 'Performance'>('Portfolio');
  const [selectedMentees, setSelectedMentees] = useState<string[]>([]);

  const { data: mentorshipResponse, refetch: refetchMentorships } = useGetCurrentUserMentees(
    { SeasonSlug: seasonSlug, Email: email },
    { query: { enabled: email !== '' && seasonSlug !== '' } }
  );

  const mentees = mentorshipResponse?.responseBody?.mentorships.map((mentorship) => {
    return {
      label: mentorship.menteeName,
      value: mentorship.menteeName,
    };
  });

  const menteesEmails =
    mentorshipResponse?.responseBody?.mentorships.map((mentorship) => mentorship.menteeEmail) || [];

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useListMockInterview(
    {
      SeasonId: seasonId || undefined,
      IncludeCustom: true,
      IncludeLeetcode: true,
      IncludeBehavioural: true,
      Emails: menteesEmails,
    },
    { query: { enabled: menteesEmails.length > 0 } }
  );

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useListProblemAttempt(
    {
      SeasonId: seasonId || undefined,
      IncludeCustom: false,
      IncludeLeetcode: true,
      Emails: menteesEmails,
    },
    { query: { enabled: menteesEmails.length > 0 } }
  );

  const PortfolioComponent = useMemo(() => {
    const mentorships = (mentorshipResponse?.responseBody?.mentorships || []).filter(
      (mentorship) =>
        selectedMentees.length === 0 ||
        selectedMentees.length === 0 ||
        (mentorship.menteeName != null &&
          selectedMentees.includes(mentorship.menteeName.toString()))
    );

    return <MenteesTable refetchMentorships={refetchMentorships} mentorships={mentorships} />;
  }, [refetchMentorships, mentorshipResponse, selectedMentees]);

  const LeetcodeComponent = useMemo(() => {
    const problemAttempts = (problemAttemptsResponse?.responseBody?.problemAttempts || []).filter(
      (problemAttempt) =>
        selectedMentees.length === 0 ||
        selectedMentees.length === 0 ||
        (problemAttempt.enrollment?.user?.name != null &&
          selectedMentees.includes(problemAttempt.enrollment?.user?.name.toString()))
    );

    return (
      <LeetcodeTable
        refetchProblemAttempts={refetchProblemAttempts}
        problemAttempts={problemAttempts}
        enrollmentId={enrollmentId || ''}
        enableEditing={false}
        showAuthor
      />
    );
  }, [refetchProblemAttempts, problemAttemptsResponse, selectedMentees, enrollmentId]);

  const MockInterviewComponent = useMemo(() => {
    const mocks = (mockInterviewsResponse?.responseBody?.mockInterviews || []).filter(
      (mock) =>
        selectedMentees.length === 0 ||
        selectedMentees.length === 0 ||
        (mock.interviewee?.name != null &&
          selectedMentees.includes(mock.interviewee?.name.toString()))
    );

    return (
      <MockInterviewTable
        refetchMockInterviews={refetchMockInterviews}
        mockInterviews={mocks}
        seasonId={seasonId || ''}
        enableEditing={false}
      />
    );
  }, [refetchMockInterviews, mockInterviewsResponse, selectedMentees, seasonId]);

  const PerformanceComponent = (
    <>
      <Text fw={500} size="md" my={18} mt={0}>
        Leetcode
      </Text>
      {LeetcodeComponent}
      <Text fw={500} size="md" my={18} mt={24}>
        Mock Interviews
      </Text>
      {MockInterviewComponent}
    </>
  );

  return (
    <Layout>
      <Flex justify="space-between" align="flex-start">
        <Group mb="lg" justify="flex-start">
          <SegmentedControl
            onChange={(value: any) =>
              setTimeout(() => {
                setSection(value);
              }, 150)
            }
            mb={10}
            className={classes.control}
            size="sm"
            color="blue"
            data={['Portfolio', 'Performance']}
          />
        </Group>
        <Group mb="lg" justify="flex-end">
          <MultiSelect
            classNames={{ inputField: classes.inputField }}
            label="Mentees"
            placeholder="Pick value(s)"
            data={mentees}
            filter={createOptionsFilter()}
            miw={150}
            searchable
            nothingFoundMessage="Nothing found..."
            value={selectedMentees}
            onChange={(values) => {
              setSelectedMentees(values as string[]);
            }}
          />
        </Group>
      </Flex>
      {section === 'Portfolio' ? PortfolioComponent : null}

      {section === 'Performance' ? PerformanceComponent : null}
    </Layout>
  );
}
