import { IconBrandGithub, IconBrandInstagram, IconBrandLinkedin } from '@tabler/icons-react';
import {
  ActionIcon,
  Avatar,
  Badge,
  Card,
  Grid,
  Group,
  rem,
  Text,
  useMantineTheme,
} from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import classes from './Graduates.module.css';

const graduates: GraduateCardProps[] = [
  {
    image:
      'https://images.pexels.com/photos/1490908/pexels-photo-1490908.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1',
    name: 'John Doe',
    season: 'New York 2023',
    company: 'Google',
  },
];

export function GraduatesPage() {
  return (
    <Layout>
      <Grid gutter={{ base: 'md', xs: 'md', md: 'xl', xl: 50 }}>
        {Array(30)
          .fill(null)
          .map((_) => (
            <Grid.Col span={{ base: 12, xs: 6, sm: 6, md: 4, lg: 3, xl: 2 }}>
              <GraduateCard graduate={graduates[0]} />
            </Grid.Col>
          ))}
      </Grid>
    </Layout>
  );
}

interface GraduateCardProps {
  image: string;
  name: string;
  season: string;
  company: string;
}

export function GraduateCard({ graduate }: { graduate: GraduateCardProps }) {
  const theme = useMantineTheme();

  return (
    <Card withBorder shadow="sm" radius="md" className={classes.card}>
      <Avatar src={graduate.image} size={120} radius={120} mx="auto" />
      <Text ta="center" fz="lg" fw={600} mt="md">
        {graduate.name}
      </Text>
      <Text ta="center" c="dimmed" fz="sm">
        {graduate.company}
      </Text>
      <Badge color="lime.8" mt="sm">
        {graduate.season}
      </Badge>

      <Group gap={0} mt="md">
        <ActionIcon variant="subtle" color="gray">
          <IconBrandLinkedin
            style={{ width: rem(20), height: rem(20) }}
            color={theme.colors.blue[6]}
            stroke={1.5}
          />
        </ActionIcon>
        <ActionIcon variant="subtle" color="gray">
          <IconBrandGithub
            style={{ width: rem(20), height: rem(20) }}
            color={theme.colors.gray[6]}
            stroke={1.5}
          />
        </ActionIcon>
        <ActionIcon variant="subtle" color="gray">
          <IconBrandInstagram
            style={{ width: rem(20), height: rem(20) }}
            color={theme.colors.pink[6]}
            stroke={1.5}
          />
        </ActionIcon>
      </Group>
    </Card>
  );
}
