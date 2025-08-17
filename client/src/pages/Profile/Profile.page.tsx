import dayjs from 'dayjs';
import localizedFormat from 'dayjs/plugin/localizedFormat';
import { useMemo, useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import { IconCheck } from '@tabler/icons-react';
import { useSearchParams } from 'react-router-dom';
import { Avatar, Card, Grid, Group, SegmentedControl, Text, Timeline } from '@mantine/core';
import {
  EnrollmentResponseDto,
  SeasonRole,
  useGetUserEnrollments,
  useListMockInterview,
  useListProblemAttempt,
} from '@/generated/api/client';
import {
  SeasonRoleReverseIndex,
  SeasonStudentRolePromotionReverseIndex,
} from '@/shared/entities/reverseIndex';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { SeasonStudentRolePromotionColors } from '@/shared/utils/colorMappings';
import { LeetcodeGraphPreset } from '../Leetcode/Leetcode.page';
import { LeetcodeTable } from '../Leetcode/LeetcodeTable/LeetcodeTable';
import { ProblemAttemptsGraphContainer } from '../Leetcode/ProblemAttemptsGraph/ProblemAttemptsGraphContainer';
import { MockInterviewTable } from '../MockInterviews/MockInterviewTable/MockInterviewTable';
import classes from './Profile.module.css';

dayjs.extend(localizedFormat);

export default function ProfilePage() {
  const [searchParams] = useSearchParams();
  const { user: Auth0User } = useAuth0();
  const slug = searchParams.get('user') || '';

  const { seasonSlug } = useSeasonSlug();
  const { enrollmentId, seasonId, user, role, email } = useUserAndEnrollment(
    seasonSlug,
    Auth0User?.email || '',
    slug
  );
  const [section, setSection] = useState<'Leetcode' | 'Mock Interviews'>('Leetcode');
  const {
    data: enrollmentsResponse,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useGetUserEnrollments({
    email,
  });

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useListProblemAttempt(
    {
      SeasonId: seasonId || undefined,
      IncludeCustom: false,
      IncludeLeetcode: true,
      Emails: [email],
    },
    { query: { enabled: email !== '' } }
  );

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useListMockInterview(
    {
      SeasonId: seasonId,
      IncludeCustom: true,
      IncludeLeetcode: true,
      IncludeBehavioural: true,
      Emails: [email],
    },
    { query: { enabled: email !== '' } }
  );

  const MockInterviewComponent = useMemo(() => {
    return (
      <MockInterviewTable
        refetchMockInterviews={refetchMockInterviews}
        mockInterviews={mockInterviewsResponse?.responseBody?.mockInterviews}
        seasonId={seasonId || ''}
        enableEditing={false}
      />
    );
  }, [refetchMockInterviews, mockInterviewsResponse, seasonId]);

  const LeetcodeComponent = useMemo(() => {
    return (
      <>
        <ProblemAttemptsGraphContainer
          problemAttempts={problemAttemptsResponse?.responseBody?.problemAttempts}
          graphPreset={LeetcodeGraphPreset.ScatterChart}
        />
        <LeetcodeTable
          refetchProblemAttempts={refetchProblemAttempts}
          problemAttempts={problemAttemptsResponse?.responseBody?.problemAttempts}
          enrollmentId={enrollmentId || ''}
          enableEditing={false}
          showAuthor={false}
          showCategory={false}
        />
      </>
    );
  }, [refetchProblemAttempts, problemAttemptsResponse, enrollmentId]);

  const seasonRole = role !== null ? SeasonRoleReverseIndex[role] : '';

  if (isFetchingEnrollments || isLoadingEnrollments) {
    return null;
  }

  return (
    <Grid gutter={{ base: 'md', xs: 'md', md: 'xl', xl: 'xl' }}>
      <Grid.Col span={{ base: 12, sm: 12, md: 12, lg: 3, xl: 2 }}>
        <ProfileSummary
          problemsCount={problemAttemptsResponse?.responseBody?.problemAttempts.length || 0}
          mockInterviewsCount={mockInterviewsResponse?.responseBody?.mockInterviews.length || 0}
          name={user?.name || ''}
          seasonRole={seasonRole}
          studentRole={null}
        />
        <ProfileTimeline enrollments={enrollmentsResponse?.responseBody?.enrollments || []} />
      </Grid.Col>
      <Grid.Col span={{ base: 12, sm: 12, md: 12, lg: 9, xl: 10 }}>
        <SegmentedControl
          onChange={(value: any) =>
            setTimeout(() => {
              setSection(value);
            }, 150)
          }
          mb={10}
          className={classes.control}
          size="sm"
          data={['Leetcode', 'Mock Interviews']}
        />

        {section === 'Leetcode' ? LeetcodeComponent : null}

        {section === 'Mock Interviews' ? MockInterviewComponent : null}
      </Grid.Col>
    </Grid>
  );
}

type ProfileTimelineProps = {
  enrollments: EnrollmentResponseDto[];
};

export function ProfileTimeline({ enrollments }: ProfileTimelineProps) {
  const timelineItems = enrollments.map((e) => {
    const formattedDates = `${dayjs(e.seasonStartDate).format('D MMM YYYY')} - ${dayjs(e.seasonEndDate).format('D MMM YYYY')}`;

    let description: React.ReactNode = null;

    switch (e.role) {
      case SeasonRole.Student:
        description = (
          <>
            Achieved{' '}
            <Text span fw={600} c={SeasonStudentRolePromotionColors[e.studentRolePromotion]}>
              {SeasonStudentRolePromotionReverseIndex[e.studentRolePromotion]}
            </Text>{' '}
            status.
          </>
        );
        break;
      case SeasonRole.Mentor:
        description = (
          <>
            Mentored{' '}
            <Text span fw={600}>
              {e.numMenteesInSeason}
            </Text>{' '}
            students.
          </>
        );
        break;
      case SeasonRole.Coordinator:
        description = (
          <>
            Coordinated{' '}
            <Text span fw={600}>
              {e.numStudentsInSeason}
            </Text>{' '}
            users and{' '}
            <Text span fw={600}>
              {e.numMentorsInSeason}
            </Text>{' '}
            mentors.
          </>
        );
        break;
      default:
        description = null;
    }

    return (
      <Timeline.Item
        key={e.seasonName}
        title={<Text>{e.seasonName}</Text>}
        bullet={<IconCheck size={14} />}
      >
        <Text c="dimmed" size="xs">
          {SeasonRoleReverseIndex[e.role]}
        </Text>
        <Text size="sm" my={5}>
          {description}
        </Text>
        <Text c="dimmed" size="xs" mt={4}>
          {formattedDates}
        </Text>
      </Timeline.Item>
    );
  });

  return (
    <Card withBorder padding="xl" radius="md" mt="xl" className={classes.card}>
      <Timeline active={20} bulletSize={24} lineWidth={4}>
        {timelineItems}
      </Timeline>
    </Card>
  );
}

type ProfileSummaryProps = {
  problemsCount: number;
  mockInterviewsCount: number;
  name: string;
  seasonRole: string;
  studentRole: string | null;
};

export function ProfileSummary({
  problemsCount,
  mockInterviewsCount,
  name,
  seasonRole,
}: ProfileSummaryProps) {
  const stats = [
    { value: problemsCount, label: 'Problems' },
    { value: mockInterviewsCount, label: 'Mock Interviews' },
  ];

  const items = stats.map((stat) => (
    <div key={stat.label}>
      <Text ta="center" fz="lg" fw={500}>
        {stat.value}
      </Text>
      <Text ta="center" fz="sm" c="dimmed" lh={1}>
        {stat.label}
      </Text>
    </div>
  ));

  return (
    <Card withBorder padding="xl" radius="md" className={classes.card}>
      <Card.Section
        h={140}
        style={{
          backgroundImage:
            'url(https://images.pexels.com/photos/1249586/pexels-photo-1249586.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=2)',
          backgroundSize: 'cover',
          backgroundPosition: 'top',
          backgroundRepeat: 'no-repeat',
        }}
      />
      <Avatar
        size={80}
        radius={80}
        mx="auto"
        mt={-40}
        className={classes.avatar}
        opacity={1}
        name={name}
      />
      <Text ta="center" fz="lg" fw={500} mt="sm">
        {name}
      </Text>
      <Text ta="center" fz="sm" c="dimmed">
        {seasonRole}
      </Text>
      <Group mt="md" justify="center" gap={30}>
        {items}
      </Group>
    </Card>
  );
}
