import { Layout } from '@/components/Layout/Layout';
import { SeasonRole } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { Button, Card, Grid, Group, Image, Skeleton, Text } from '@mantine/core';
import classes from './Resources.module.css';

type Resource = {
  link: string;
  title: string;
  imageUrl: string;
}

export default function ResourcesPage() {
  const { seasonSlug } = useSeasonSlug();
  const { role, isLoading } = useUserAndEnrollment(seasonSlug);

  const coordinatorResources: Resource[] = [
    {
      link: 'https://google.com',
      title: 'Coordinator Resource',
      imageUrl: 'https://images.pexels.com/photos/66100/pexels-photo-66100.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1'
    }
  ];
  const mentorResources: Resource[] = [
    {
      link: 'https://google.com',
      title: 'Mentor Resource',
      imageUrl: 'https://images.pexels.com/photos/1250452/pexels-photo-1250452.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1'
    }
  ];
  const studentResources: Resource[] = [
    {
      link: 'https://google.com',
      title: 'Student Resource',
      imageUrl: 'https://images.pexels.com/photos/1516440/pexels-photo-1516440.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1'
    }
  ]

  let resources;

  if (role === SeasonRole.Coordinator) {
    resources = coordinatorResources;
  } else if (role === SeasonRole.Mentor) {
    resources = mentorResources;
  } else if (role === SeasonRole.Student) {
    resources = studentResources;
  }

  return (
    <Layout>
      <Grid gutter={{ base: 5, xs: 'md', md: 'xl', xl: 'xl' }}>
        {!isLoading ? (
          <ResourcesGridCards resources={resources} />
        ) : (
          <ResourcesSkeletonCards />
        )}
      </Grid>
    </Layout>
  );
}

const ResourcesSkeletonCards = () => {
  const numCards = 3;

  return (
    <>
      {Array.from({ length: numCards }).map((_, index) => (
        <Grid.Col key={index} span={{ base: 12, sm: 6, md: 6, lg: 4 }}>
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

const ResourcesGridCards = ({ resources }: ResourcesListProps) => {
  return (
    <>
      {resources?.map((resource, key) => (
        <Grid.Col key={key} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
          <Card withBorder shadow="sm" radius="md" className={classes.card}>
            <Card.Section className={classes.imageSection}>
              <Image
                src={resource.imageUrl}
                alt="Resource image"
                className={classes.image}
              />
            </Card.Section>

            <Group justify="space-between" mt="md" mb="xs">
              <Text fw={500}>{resource.title}</Text>
            </Group>

            <Button
              color="blue"
              fullWidth
              mt="md"
              radius="md"
              component="a"
              target='_blank'
              href={resource.link}
            >
              View
            </Button>
          </Card>
        </Grid.Col>
      ))}
    </>
  );
};

type ResourcesListProps = {
  resources: Resource[] | undefined;
};
