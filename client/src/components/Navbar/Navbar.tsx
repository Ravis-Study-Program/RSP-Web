import { useState } from 'react';
import { useAuth0 } from '@auth0/auth0-react';
import {
  IconBarbell,
  IconChevronRight,
  IconCode,
  IconFlag,
  IconFolder,
  IconLogout,
  IconMoon,
  IconSchool,
  IconSettings,
  IconSun,
  IconUser,
  IconUsers,
} from '@tabler/icons-react';
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
import { useGetCurrentUser } from '@/generated/api/client';
import { LinksGroup } from '../NavbarLinksGroup/NavbarLinksGroup';
import classes from './Navbar.module.css';

const tabs = {
  general: [
    { label: 'Seasons', icon: IconFlag, link: '/seasons' },
    { label: 'Graduates', icon: IconSchool, link: '/graduates' },
  ],
  season: [
    { label: 'Leetcode', icon: IconCode, link: '/leetcode' },
    { label: 'Mocks', icon: IconBarbell, link: '' },
    { label: 'Students', icon: IconUsers, link: '' },
    { label: 'Resources', icon: IconFolder, link: '' },
  ],
};

export function Navbar() {
  const { setColorScheme } = useMantineColorScheme();
  const [section, setSection] = useState<'general' | 'season'>('general');
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { logout } = useAuth0();
  const {
    data: userResponse,
    isError: isLoadingUserError,
    isFetching: isFetchingUser,
    isLoading: isLoadingUser,
  } = useGetCurrentUser();

  const links = tabs[section].map((item) => <LinksGroup {...item} key={item.label} />);

  return (
    <nav className={classes.navbar}>
      <div className={classes.header}>
        <Group justify="center">
          <Title order={1} size="h3" ta="center">
            Ravi Study Program
          </Title>
        </Group>
      </div>

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

      <ScrollArea className={classes.links}>
        <div className={classes.linksInner}>{links}</div>
      </ScrollArea>

      {!isLoadingUser && !isFetchingUser && !isLoadingUserError ? (
        <div className={classes.footer}>
          <Menu
            shadow="md"
            width={200}
            position="bottom"
            withArrow
            openDelay={100}
            closeDelay={400}
          >
            <Menu.Target>
              <UnstyledButton className={classes.user}>
                <Group>
                  <Avatar
                    color="initials"
                    name={userResponse?.responseBody?.user?.name || undefined}
                    radius="xl"
                  />
                  <div style={{ flex: 1 }}>
                    <Text size="sm" fw={500}>
                      {userResponse?.responseBody?.user?.name || 'New User'}
                    </Text>

                    <Text c="dimmed" size="xs">
                      {userResponse?.responseBody?.user?.email || null}
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
      ) : (
        <NavigationProfileSkeleton />
      )}
    </nav>
  );
}

const NavigationProfileSkeleton = () => {
  return (
    <>
      <Flex className={classes.userSkeleton}>
        <Skeleton height={40} width={40} circle />
        <Box ml="md" flex={1}>
          <Skeleton height={8} mt={10} width="70%" radius="xl" />
          <Skeleton height={8} mt={6} radius="xl" />
        </Box>
      </Flex>
    </>
  );
};
