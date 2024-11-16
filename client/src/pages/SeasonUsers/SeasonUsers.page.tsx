import { Layout } from '@/components/Layout/Layout';
import { SeasonUserDto, useGetSeasonUsers } from '@/generated/api/client';
import {
  ActionIcon,
  Anchor,
  Avatar,
  Badge,
  Card,
  Grid,
  Group,
  rem,
  Skeleton,
  Text
} from '@mantine/core';
import { IconBrandDiscordFilled } from '@tabler/icons-react';
import classes from './SeasonUsers.module.css';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { SeasonRoleReverseIndex } from '@/shared/entities/reverseIndex';

export default function SeasonUsersPage() {
  const { seasonSlug } = useSeasonSlug();
  const {
    data: seasonUsersResponse,
    isError: isLoadingSeasonUsersError,
    isFetching: isFetchingSeasonUsers,
    isLoading: isLoadingSeasonUsers,
  } = useGetSeasonUsers(seasonSlug);

  return (
    <Layout>
      <Grid gutter={{ base: 'md', xs: 'md', md: 'xl', xl: 50 }}>
        {!isLoadingSeasonUsers && !isFetchingSeasonUsers && !isLoadingSeasonUsersError ? (
          <SeasonUserCards seasonUsers={seasonUsersResponse?.responseBody?.seasonUsers} />
        ) : (
          <SeasonUserSkeletonCards />
        )}
      </Grid>
    </Layout>
  );
}

const SeasonUserSkeletonCards = () => {
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


type SeasonUserCardsProps = {
  seasonUsers: SeasonUserDto[] | undefined
}

export function SeasonUserCards({ seasonUsers }: SeasonUserCardsProps) {
  return (
    <>
      {
        seasonUsers?.map((seasonUser, key) => (
          <Grid.Col key={key} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
          <Card key={key} withBorder shadow="sm" radius="md" className={classes.card}>
            <Avatar src={seasonUser.profileImage} size={70} radius={70} mx="auto" />
            <Text ta="center" fz="lg" fw={600} mt="md">
              {seasonUser.name}
            </Text>
            <Badge mt={10} autoContrast color="yellow.5">{SeasonRoleReverseIndex[seasonUser.role]}</Badge>

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
