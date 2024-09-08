import { IconPointFilled } from '@tabler/icons-react';
import { Badge, Button, Card, Center, Grid, Group, Image, Text } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import classes from './Seasons.module.css';

const seasons: SeasonCardProps[] = [
  {
    image:
      'https://images.pexels.com/photos/466685/pexels-photo-466685.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1',
    title: 'New York Summer 2023',
    role: 'Student',
    highlights: ['Advanced', '113 Leetcodes', '26 Mocks'],
  },
  {
    image:
      'https://images.pexels.com/photos/2193300/pexels-photo-2193300.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1',
    title: 'Sydney Summer 2024',
    role: 'Mentor',
    highlights: ['14 Mentees'],
  },
];

export function SeasonsPage() {
  return (
    <Layout>
      <Grid gutter={{ base: 5, xs: 'md', md: 'xl', xl: 50 }}>
        {seasons.map((season) => (
          <Grid.Col span={{ base: 12, sm: 6, md: 6, lg: 3 }}>
            <SeasonCard season={season} />
          </Grid.Col>
        ))}
      </Grid>
    </Layout>
  );
}

interface SeasonCardProps {
  image: string;
  title: string;
  role: string;
  highlights: string[];
}

export function SeasonCard({ season }: { season: SeasonCardProps }) {
  const highlights = season.highlights.map((highlight) => (
    <Center key={highlight}>
      <IconPointFilled size="1.05rem" className={classes.icon} stroke={1.5} />
      <Text size="xs">{highlight}</Text>
    </Center>
  ));

  return (
    <Card withBorder shadow="sm" radius="md" className={classes.card}>
      <Card.Section className={classes.imageSection}>
        <Image src={season.image} alt="Tesla Model S" className={classes.image} />
      </Card.Section>

      <Group justify="space-between" mt="md" mb="xs">
        <Text fw={500}>{season.title}</Text>
        <Badge color="lime.8">{season.role}</Badge>
      </Group>

      <Group gap={8} mb={-8}>
        <Text size="sm" c="dimmed">
          Lorem ipsum dolor sit amet consectetur adipisicing elit. Quas eveniet amet officia nam ab
          sint eos, velit.
        </Text>
        <Group my={10}>{highlights}</Group>
      </Group>

      <Button color="blue" fullWidth mt="md" radius="md">
        Select
      </Button>
    </Card>
  );
}
