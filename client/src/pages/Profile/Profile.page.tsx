import { Layout } from '@/components/Layout/Layout';
import { useGetMockInterviews, useGetProblemAttempts } from '@/generated/api/client';
import { SeasonRoleReverseIndex } from '@/shared/entities/reverseIndex';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { Avatar, Card, Grid, Group, SegmentedControl, Text } from '@mantine/core';
import { useMemo, useState } from 'react';
import { LeetcodeTable } from '../Leetcode/LeetcodeTable/LeetcodeTable';
import { MockInterviewTable } from '../MockInterviews/MockInterviewTable/MockInterviewTable';
import classes from './Profile.module.css';
import { useSearchParams } from 'react-router-dom';

export default function ProfilePage() {
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email');

  const { seasonSlug } = useSeasonSlug();
  const { enrollmentId, user, role } = useUserAndEnrollment(seasonSlug, email);
  const [section, setSection] = useState<'Leetcode' | 'Mock Interviews'>('Leetcode');

  const { data: problemAttemptsResponse, refetch: refetchProblemAttempts } = useGetProblemAttempts({
    enrollmentId: enrollmentId || undefined,
    includeCustom: false,
    includeLeetcode: true,
  });

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useGetMockInterviews({
    enrollmentId,
    includeCustom: true,
    includeLeetcode: true,
    includeBehavioural: true,
  });

  const MockInterviewComponent = useMemo(() => {
    return (
      <MockInterviewTable
        refetchMockInterviews={refetchMockInterviews}
        mockInterviews={mockInterviewsResponse?.responseBody?.mockInterviews}
        enrollmentId={enrollmentId || ''}
        enableEditing={false}
      />
    )
  }, [refetchMockInterviews, mockInterviewsResponse, enrollmentId]);

  const LeetcodeComponent = useMemo(() => {
    return (
      <LeetcodeTable
        refetchProblemAttempts={refetchProblemAttempts}
        problemAttempts={problemAttemptsResponse?.responseBody?.problemAttempts}
        enrollmentId={enrollmentId || ''}
        enableEditing={false}
      />
    )
  }, [refetchProblemAttempts, problemAttemptsResponse, enrollmentId]);

  const seasonRole = role !== null ? SeasonRoleReverseIndex[role] : '';

  return (
    <Layout>
      <Grid gutter={{ base: 'md', xs: 'md', md: 'xl', xl: 50 }}>
        <Grid.Col span={{ base: 12, sm: 12, md: 12, lg: 3 }}>
          <ProfileSummary
            problemsCount={problemAttemptsResponse?.responseBody?.problemAttempts.length || 0}
            mockInterviewsCount={mockInterviewsResponse?.responseBody?.mockInterviews.length || 0}
            name={user?.name || ''}
            seasonRole={seasonRole} studentRole={null} />
        </Grid.Col>
        <Grid.Col span={{ base: 12, sm: 12, md: 12, lg: 9 }}>

          <SegmentedControl
            onChange={(value: any) => setTimeout(() => {
              setSection(value)
            }, 150)}
            mb={10}
            className={classes.control}
            size="sm"
            color="blue"
            data={['Leetcode', 'Mock Interviews']} />

          {
            section === 'Leetcode' ? LeetcodeComponent : null
          }

          {
            section === 'Mock Interviews' ? MockInterviewComponent : null
          }
        </Grid.Col>
      </Grid>
    </Layout>
  );
}

type ProfileSummaryProps = {
  problemsCount: number,
  mockInterviewsCount: number,
  name: string,
  seasonRole: string,
  studentRole: string | null
}

export function ProfileSummary({ problemsCount, mockInterviewsCount, name, seasonRole }: ProfileSummaryProps) {
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
            'url(https://images.unsplash.com/photo-1488590528505-98d2b5aba04b?ixlib=rb-1.2.1&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=500&q=80)',
        }}
      />
      <Avatar
        size={80}
        radius={80}
        mx="auto"
        mt={-30}
        className={classes.avatar}
        name={name}
        color='initials'
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

