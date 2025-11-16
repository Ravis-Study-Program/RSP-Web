import { useMemo, useState } from 'react';
import { Flex, Group, MultiSelect, SegmentedControl, Text } from '@mantine/core';
import { LeetcodeDifficultyChart } from '@/components/LeetcodeDifficultyChart/LeetcodeDifficultyChart';
import {
  useGetCurrentUserMentees,
  useGetEnrollmentUsers,
  useGetIsCurrentUserEnrolled,
  useGetSeasonWeeksBySeasonSlug,
  useListMockInterview,
  useListProblemAttempt,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { createOptionsFilter, getSeasonWeeks } from '@/shared/table/globalFilters';
import { LeetcodeTable } from '../Leetcode/LeetcodeTable/LeetcodeTable';
import { MockInterviewTable } from '../MockInterviews/MockInterviewTable/MockInterviewTable';
import { MenteesTable } from './MenteesTable';
import classes from './Mentees.module.css';

export default function MenteesPage() {
  const { seasonSlug } = useSeasonSlug();
  const { data: userResponse } = useGetIsCurrentUserEnrolled({ seasonSlug });
  const userId = userResponse?.responseBody?.userId ?? '';
  const { enrollmentId, seasonId } = useUserAndEnrollment(seasonSlug);
  const [section, setSection] = useState<'Portfolio' | 'Performance'>('Portfolio');
  const [selectedMentees, setSelectedMentees] = useState<string[]>([]);
  const [selectedSeasonWeeks, setSelectedSeasonWeeks] = useState<string[]>([]);

  const { data: mentorshipResponse, refetch: refetchMentorships } = useGetCurrentUserMentees(
    { SeasonSlug: seasonSlug, UserId: userId },
    { query: { enabled: userId !== '' && seasonSlug !== '' } }
  );

  const { data: enrollmentUsersResponse } = useGetEnrollmentUsers(
    { SeasonSlug: seasonSlug },
    { query: { enabled: seasonSlug !== '' } }
  );

  const { data: seasonWeeksResponse } = useGetSeasonWeeksBySeasonSlug(
    { SeasonSlug: seasonSlug },
    { query: { enabled: seasonSlug !== '' } }
  );

  const mentees = mentorshipResponse?.responseBody?.mentorships.map((mentorship) => {
    return {
      label: mentorship.menteeName,
      value: mentorship.menteeName,
    };
  });

  const seasonWeeksOptions = getSeasonWeeks(seasonWeeksResponse?.responseBody?.seasonWeeks || []);

  const getMenteeUserIds = useMemo(() => {
    const enrollmentUsers = enrollmentUsersResponse?.responseBody?.enrollmentUsers || [];
    const mentorships = mentorshipResponse?.responseBody?.mentorships || [];

    const selectedMenteeNames =
      selectedMentees.length === 0 ? mentorships.map((m) => m.menteeName) : selectedMentees;

    return enrollmentUsers
      .filter((user) => selectedMenteeNames.includes(user.name))
      .map((user) => user.userId);
  }, [mentorshipResponse, enrollmentUsersResponse, selectedMentees]);

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useListMockInterview(
    {
      SeasonId: seasonId || undefined,
      IncludeCustom: true,
      IncludeLeetcode: true,
      IncludeBehavioural: true,
      UserIds: getMenteeUserIds,
    },
    { query: { enabled: getMenteeUserIds.length > 0 && seasonId !== null } }
  );

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useListProblemAttempt(
    {
      SeasonId: seasonId || undefined,
      IncludeCustom: false,
      IncludeLeetcode: true,
      UserIds: getMenteeUserIds,
    },
    { query: { enabled: getMenteeUserIds.length > 0 && seasonId !== null } }
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
        // Mentees filter
        (selectedMentees.length === 0 ||
          (problemAttempt.user?.name != null &&
            selectedMentees.includes(problemAttempt.user?.name.toString()))) &&
        // Season weeks filter
        (selectedSeasonWeeks.length === 0 ||
          (problemAttempt.seasonWeekId != null &&
            selectedSeasonWeeks.includes(problemAttempt.seasonWeekId.toString())))
    );

    return (
      <>
        <LeetcodeDifficultyChart problemAttempts={problemAttempts} />
        <LeetcodeTable
          refetchProblemAttempts={refetchProblemAttempts}
          problemAttempts={problemAttempts}
          enrollmentId={enrollmentId || ''}
          enableEditing={false}
          showAuthor
          showCategory
        />
      </>
    );
  }, [
    refetchProblemAttempts,
    problemAttemptsResponse,
    selectedMentees,
    selectedSeasonWeeks,
    enrollmentId,
  ]);

  const MockInterviewComponent = useMemo(() => {
    const mocks = (mockInterviewsResponse?.responseBody?.mockInterviews || []).filter(
      (mock) =>
        // Mentees filter
        (selectedMentees.length === 0 ||
          (mock.interviewee?.name != null &&
            selectedMentees.includes(mock.interviewee?.name.toString()))) &&
        // Season weeks filter
        (selectedSeasonWeeks.length === 0 ||
          (mock.seasonWeekId != null && selectedSeasonWeeks.includes(mock.seasonWeekId.toString())))
    );

    return (
      <MockInterviewTable
        refetchMockInterviews={refetchMockInterviews}
        mockInterviews={mocks}
        seasonId={seasonId || ''}
        enableEditing={false}
      />
    );
  }, [
    refetchMockInterviews,
    mockInterviewsResponse,
    selectedMentees,
    selectedSeasonWeeks,
    seasonId,
  ]);

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
    <>
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
          {section === 'Performance' && (
            <MultiSelect
              classNames={{ inputField: classes.inputField }}
              label="Season Week"
              placeholder="Pick value(s)"
              data={seasonWeeksOptions}
              filter={createOptionsFilter()}
              miw={150}
              searchable
              nothingFoundMessage="Nothing found..."
              value={selectedSeasonWeeks}
              onChange={(values) => {
                setSelectedSeasonWeeks(values as string[]);
              }}
            />
          )}
        </Group>
      </Flex>
      {section === 'Portfolio' ? PortfolioComponent : null}

      {section === 'Performance' ? PerformanceComponent : null}
    </>
  );
}
