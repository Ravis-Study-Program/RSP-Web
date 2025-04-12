import { useAuth0 } from '@auth0/auth0-react';
import {
  IconChevronRight,
  IconLogout,
  IconMoon,
  IconSearch,
  IconSettings,
  IconSun,
  IconTrophy,
  IconUser,
} from '@tabler/icons-react';
import { useNavigate } from 'react-router-dom';
import {
  Anchor,
  Avatar,
  Badge,
  Box,
  Code,
  Flex,
  Group,
  Menu,
  rem,
  ScrollArea,
  Skeleton,
  Text,
  TextInput,
  Title,
  UnstyledButton,
  useComputedColorScheme,
  useMantineColorScheme,
} from '@mantine/core';
import { spotlight, Spotlight } from '@mantine/spotlight';
import { EnrollmentResponseDto, UserEntity } from '@/generated/api/client';
import { LinksGroup } from '../NavbarLinksGroup/NavbarLinksGroup';
import { createAdminSpotlightActions, createNonAdminSpotlightActions, Tabs } from './NavbarRoutes';
import classes from './Navbar.module.css';

export function Navbar({ isLoading, user, tabs, isSeasonUrl, enrollments = [] }: NavbarProps) {
  const { setColorScheme } = useMantineColorScheme();
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { logout } = useAuth0();
  const navigate = useNavigate();
  const pathname = typeof window !== 'undefined' ? window.location.pathname : '';

  const generalLinks =
    tabs?.general
      ?.filter((item) => !item.hidden)
      ?.map((item) => {
        // Inject the enrolled seasons as links
        if (item.label === 'Seasons' && !user?.isAdmin) {
          item.links = enrollments.map((e) => ({
            label: e.seasonName,
            icon: IconTrophy,
            link: `/seasons/${e.seasonSlug}/overview`,
            links: [],
            hidden: false,
          }));

          // Add view all seasons
          item.links.push({
            label: 'All Seasons',
            icon: IconTrophy,
            link: '/seasons',
            links: [],
            hidden: false,
          });

          // Explicitly make the season group tab unreachable via navigation
          item.link = undefined;

          // Explicitly disable active link
          return (
            <LinksGroup initiallyOpened={false} activeLink={pathname} {...item} key={item.label} />
          );
        }

        return (
          <LinksGroup initiallyOpened={false} activeLink={pathname} {...item} key={item.label} />
        );
      }) ?? [];

  const seasonLinks =
    tabs?.season
      ?.filter((item) => !item.hidden)
      ?.map((item) => (
        <LinksGroup initiallyOpened activeLink={pathname} {...item} key={item.label} />
      )) ?? [];

  const adminSpotlightActions = createAdminSpotlightActions(navigate);
  const nonAdminSpotlightActions = createNonAdminSpotlightActions(navigate);

  if (isLoading) {
    return <NavbarSkeleton />;
  }

  return (
    <nav className={classes.navbar}>
      <Spotlight
        actions={user?.isAdmin ? adminSpotlightActions : nonAdminSpotlightActions}
        nothingFound="Nothing found..."
        highlightQuery
        searchProps={{
          leftSection: <IconSearch size={20} stroke={1.5} />,
          placeholder: 'Search...',
        }}
      />
      <div className={classes.header}>
        <Flex justify="center" align="center">
          <Anchor
            href="/seasons"
            underline="never"
            c={computedColorScheme === 'light' ? 'dark' : 'white'}
          >
            <Title order={1} size="h3" ta="center">
              Ravi Study Program
            </Title>
          </Anchor>
          {user?.isAdmin ? (
            <Badge color="red" size="xs" ml={8} mt={3}>
              Admin
            </Badge>
          ) : null}
        </Flex>
      </div>

      <TextInput
        placeholder="Search"
        onClick={spotlight.open}
        size="sm"
        leftSection={<IconSearch onClick={spotlight.open} size={12} stroke={1.5} />}
        rightSectionWidth={80}
        rightSection={
          <Flex
            onClick={spotlight.open}
            align="center"
            mr="xs"
            className={classes.searchCodeContainer}
          >
            <Code className={classes.searchCode}>Ctrl + K</Code>
          </Flex>
        }
        className={classes.searchBox}
      />

      <ScrollArea className={classes.links}>
        <div className={classes.linksInner}>{generalLinks}</div>
        {isSeasonUrl ? (
          <>
            <Group className={classes.collectionsHeader} justify="space-between">
              <Text
                size="sm"
                fw={600}
                c="dimmed"
                px="lg"
                pt="md"
                style={{
                  paddingLeft: 'calc(var(--mantine-spacing-lg) + 5px)',
                }}
              >
                Active Season
              </Text>
            </Group>
            <Box className={classes.linksInner} pt="xs">
              {seasonLinks}
            </Box>
          </>
        ) : null}
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
            <Menu.Item
              leftSection={<IconUser style={{ width: rem(14), height: rem(14) }} />}
              component="a"
              href="/profile"
            >
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
}

type NavbarProps = {
  isLoading: boolean;
  user: UserEntity | undefined;
  tabs: Tabs | null;
  isSeasonUrl: boolean;
  enrollments: EnrollmentResponseDto[];
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
