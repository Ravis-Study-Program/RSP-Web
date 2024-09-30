import { Link } from 'react-router-dom';
import { Badge, Button, Card, Grid, Group, Image, Skeleton, Text } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import { Enrollment, useGetCurrentUserEnrollments } from '@/generated/api/client';
import classes from './Seasons.module.css';

export function SeasonsPage() {
  const {
    data: enrollmentsResponse,
    isError: isLoadingEnrollmentsError,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useGetCurrentUserEnrollments();

  return (
    <Layout>
      <Grid gutter={{ base: 5, xs: 'md', md: 'xl', xl: 50 }}>
        {!isLoadingEnrollments && !isFetchingEnrollments && !isLoadingEnrollmentsError ? (
          <SeasonsGridCards enrollments={enrollmentsResponse?.responseBody?.enrollments} />
        ) : (
          <SeasonsSkeletonCards />
        )}
      </Grid>
    </Layout>
  );
}

const SeasonsSkeletonCards = () => {
  const numCards = 3;

  return (
    <>
      {Array.from({ length: numCards }).map((_, index) => (
        <Grid.Col key={index} span={{ base: 12, sm: 6, md: 6, lg: 3 }}>
          <Card withBorder shadow="xs" radius="md">
            <Skeleton height={80} mb="xl" />
            <Skeleton height={10} radius="xl" />
            <Skeleton height={8} mt={8} radius="xl" />
            <Skeleton height={8} mt={8} radius="xl" />
            <Skeleton height={8} mt={8} width="70%" radius="xl" />
            <Skeleton height={40} mt={50} radius="xl" />
          </Card>
        </Grid.Col>
      ))}
    </>
  );
};

const SeasonsGridCards = ({ enrollments }: SeasonsListProps) => {
  const rolesPillColor: Record<string, string> = {
    Coordinator: 'red.8',
    Mentor: 'green.8',
    Student: 'gray.8',
  };

  const rolesSelectDestination = (seasonSlug: string): Record<string, string> => {
    return {
      Coordinator: `/${seasonSlug}/admin/students`,
      Mentor: `/${seasonSlug}/mentees`,
      Student: `/${seasonSlug}/leetcode`,
    };
  };

  return (
    <>
      {enrollments?.map((enrollment, key) => (
        <Grid.Col key={key} span={{ base: 12, sm: 6, md: 6, lg: 3 }}>
          <Card withBorder shadow="sm" radius="md" className={classes.card}>
            <Card.Section className={classes.imageSection}>
              <Image
                src={enrollment.season.imageUrl}
                alt="Tesla Model S"
                className={classes.image}
              />
            </Card.Section>

            <Group justify="space-between" mt="md" mb="xs">
              <Text fw={500}>{enrollment.season.name}</Text>
              <Badge color={rolesPillColor[enrollment.role.name] || 'gray'}>
                {enrollment.role.name}
              </Badge>
            </Group>

            <Button
              color="blue"
              fullWidth
              mt="md"
              radius="md"
              component={Link}
              to={rolesSelectDestination(enrollment.season.slug)[enrollment.role.name] || '#'}
            >
              Select
            </Button>
          </Card>
        </Grid.Col>
      ))}
    </>
  );
};

type SeasonsListProps = {
  enrollments: Enrollment[] | null | undefined;
};
