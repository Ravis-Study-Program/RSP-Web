import { Layout } from '@/components/Layout/Layout';
import { GraduateDto, useGetGraduates } from '@/generated/api/client';
import {
  ActionIcon,
  Anchor,
  Avatar,
  Button,
  Card,
  Grid,
  Group,
  rem,
  Skeleton,
  Text
} from '@mantine/core';
import { IconBrandDiscordFilled } from '@tabler/icons-react';
import classes from './Graduates.module.css';

export default function GraduatesPage() {
  const {
    data: graduatesResponse,
    isError: isLoadingGraduatesError,
    isFetching: isFetchingGraduates,
    isLoading: isLoadingGraduates,
  } = useGetGraduates();

  return (
    <Layout>
      <Grid gutter={{ base: 'md', xs: 'md', md: 'xl', xl: 50 }}>
        {!isLoadingGraduates && !isFetchingGraduates && !isLoadingGraduatesError ? (
          <GraduateCards graduates={graduatesResponse?.responseBody?.graduates} />
        ) : (
          <GraduateSkeletonCards />
        )}
      </Grid>
    </Layout>
  );
}

const GraduateSkeletonCards = () => {
  const numCards = 8;

  return (
    <>
      {Array.from({ length: numCards }).map((_, index) => (
        <Grid.Col key={index} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
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


type GraduateCardsProps = {
  graduates: GraduateDto[] | undefined
}

export function GraduateCards({ graduates }: GraduateCardsProps) {
  return (
    <>
      {
        graduates?.map((graduate, key) => (
          <Grid.Col key={key} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
          <Card key={key} withBorder shadow="sm" radius="md" className={classes.card}>
            <Avatar src={graduate.profileImage} size={70} radius={70} mx="auto" />
            <Text ta="center" fz="lg" fw={600} mt="md">
              {graduate.name}
            </Text>

            <Button component="a" href={`/profile?email=${graduate.email}`} radius="md" mt="sm" size="sm" variant="primary">
              View Profile
            </Button>

            <Group gap={0} mt="md">
                <Anchor c="gray" target='_blank' href='https://www.discord.com'>
                  <ActionIcon variant="subtle" color="gray">
                    <IconBrandDiscordFilled
                      style={{ width: rem(20), height: rem(20) }}
                      color="gray"
                      stroke={1.5}
                    />
                  </ActionIcon>
                </Anchor>
              </Group>
          </Card>
          </Grid.Col>
        ))
      }
    </>
  );
}
