import { useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import {
  IconChevronRight,
  IconLogout,
  IconMoon,
  IconSettings,
  IconSun,
  IconUser,
} from '@tabler/icons-react';
import { useLocation } from 'react-router-dom';
import {
  Avatar,
  Box,
  Flex,
  Group,
  Menu,
  rem,
  ScrollArea,
  SegmentedControl,
  Skeleton,
  Text,
  Title,
  UnstyledButton,
  useComputedColorScheme,
  useMantineColorScheme,
} from '@mantine/core';
import { useGetCurrentUser, useGetCurrentUserEnrollments, User } from '@/generated/api/client';
import { LinksGroup } from '../NavbarLinksGroup/NavbarLinksGroup';
import { getTabs, Tabs } from './NavbarRoutes';
import classes from './Navbar.module.css';

export function Navbar() {
  const {
    data: userResponse,
    isError: isLoadingUserError,
    isFetching: isFetchingUser,
    isLoading: isLoadingUser,
  } = useGetCurrentUser();
  const {
    data: enrollmentsResponse,
    isError: isLoadingEnrollmentsError,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useGetCurrentUserEnrollments(); // TODO: create another backend route instead of listing all enrollments

  // Find season slug
  const location = useLocation();
  const pathSegments = location.pathname.split('/').filter(Boolean);
  const seasonIndex = pathSegments.findIndex((segment) => segment === 'season');
  const seasonSlug =
    seasonIndex !== -1 && seasonIndex + 1 < pathSegments.length
      ? pathSegments[seasonIndex + 1]
      : null;

  const user = userResponse?.responseBody?.user;
  const enrollments = enrollmentsResponse?.responseBody?.enrollments;
  const isAdmin = user?.isAdmin || false;
  const role =
    (seasonSlug && enrollments?.find((e) => e.season?.slug === seasonSlug)?.role) || null;
  const roleName = role?.name || '';

  const isLoading =
    isLoadingUser ||
    isFetchingUser ||
    isLoadingUserError ||
    isLoadingEnrollments ||
    isLoadingEnrollmentsError ||
    isFetchingEnrollments;

  return (
    <NavbarContent
      isLoading={isLoading}
      user={user}
      tabs={getTabs(seasonSlug, isAdmin, roleName)}
    />
  );
}

type NavbarContentProps = {
  isLoading: boolean;
  user: User | undefined;
  tabs: Tabs | null;
};

const NavbarContent = ({ isLoading, user, tabs }: NavbarContentProps) => {
  const { setColorScheme } = useMantineColorScheme();
  const [section, setSection] = useState<'general' | 'season'>('general');
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { logout } = useAuth0();

  const links = tabs?.[section]?.map((item) => <LinksGroup {...item} key={item.label} />) ?? [];

  if (isLoading) {
    return <NavbarSkeleton />;
  }

  return (
    <nav className={classes.navbar}>
      <div className={classes.header}>
        <Group justify="center">
          <Title order={1} size="h3" ta="center">
            Ravi Study Program
          </Title>
        </Group>
      </div>

      {tabs?.season != null ? (
        <SegmentedControl
          value={section}
          onChange={(value: any) => setSection(value)}
          transitionTimingFunction="ease"
          fullWidth
          data={[
            { label: 'General', value: 'general' },
            { label: 'Season', value: 'season' },
          ]}
        />
      ) : null}

      <ScrollArea className={classes.links}>
        <div className={classes.linksInner}>{links}</div>
      </ScrollArea>

      <div className={classes.footer}>
        <Menu shadow="md" width={200} position="bottom" withArrow openDelay={100} closeDelay={400}>
          <Menu.Target>
            <UnstyledButton className={classes.user}>
              <Group>
                <Avatar color="initials" name={user?.name || undefined} radius="xl" />
                <div style={{ flex: 1 }}>
                  <Text size="sm" fw={500}>
                    {user?.name || 'New User'}
                  </Text>

                  <Text c="dimmed" size="xs">
                    {user?.email || null}
                  </Text>
                </div>
                <IconChevronRight style={{ width: rem(20), height: rem(20) }} stroke={2} />
              </Group>
            </UnstyledButton>
          </Menu.Target>

          <Menu.Dropdown>
            <Menu.Label>Application</Menu.Label>
            <Menu.Item leftSection={<IconUser style={{ width: rem(14), height: rem(14) }} />}>
              Profile
            </Menu.Item>
            <Menu.Item
              leftSection={<IconSettings style={{ width: rem(14), height: rem(14) }} />}
              component="a"
              href="/settings"
            >
              Settings
            </Menu.Item>
            <Menu.Item
              onClick={() => setColorScheme(computedColorScheme === 'light' ? 'dark' : 'light')}
              leftSection={
                computedColorScheme === 'light' ? (
                  <IconMoon style={{ width: rem(14), height: rem(14) }} />
                ) : (
                  <IconSun style={{ width: rem(14), height: rem(14) }} />
                )
              }
            >
              Set To {computedColorScheme === 'light' ? 'Dark' : 'Light'} Mode
            </Menu.Item>

            <Menu.Divider />

            <Menu.Label>Authentication</Menu.Label>
            <Menu.Item
              onClick={() => logout()}
              leftSection={<IconLogout style={{ width: rem(14), height: rem(14) }} />}
            >
              Sign Out
            </Menu.Item>
          </Menu.Dropdown>
        </Menu>
      </div>
    </nav>
  );
};

const NavbarSkeleton = () => {
  return (
    <nav className={classes.navbar}>
      <div className={classes.header}>
        <Group justify="center">
          <Skeleton height={14} my={10} width="80%" radius="xl" />
        </Group>
      </div>

      <Flex className={classes.userSkeleton}>
        <Skeleton height={10} mt={6} radius="xl" />
      </Flex>
      <Flex className={classes.userSkeleton}>
        <Skeleton height={10} mt={6} radius="xl" />
      </Flex>
      <Flex className={classes.userSkeleton}>
        <Skeleton height={10} mt={6} radius="xl" />
      </Flex>
      <Flex className={classes.userSkeleton}>
        <Skeleton height={10} mt={6} radius="xl" />
      </Flex>

      <div className={classes.footer}>
        <Flex className={classes.userSkeleton}>
          <Skeleton height={40} width={40} circle />
          <Box ml="md" flex={1}>
            <Skeleton height={8} mt={10} width="70%" radius="xl" />
            <Skeleton height={8} mt={10} radius="xl" />
          </Box>
        </Flex>
      </div>
    </nav>
  );
};
