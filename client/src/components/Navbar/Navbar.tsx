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
import {
  Anchor,
  Avatar,
  Badge,
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
import { User } from '@/generated/api/client';
import { LinksGroup } from '../NavbarLinksGroup/NavbarLinksGroup';
import { Tabs } from './NavbarRoutes';
import classes from './Navbar.module.css';

export function Navbar({ isLoading, user, tabs }: NavbarProps) {
  const { setColorScheme } = useMantineColorScheme();
  const [section, setSection] = useState<'general' | 'season'>('general');
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { logout } = useAuth0();

  const links =
    tabs?.[section]
      ?.filter((item) => !item.hidden)
      ?.map((item) => <LinksGroup {...item} key={item.label} />) ?? [];

  if (isLoading) {
    return <NavbarSkeleton />;
  }

  return (
    <nav className={classes.navbar}>
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

      {tabs?.season != null && tabs.season.length > 0 ? (
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
  user: User | undefined;
  tabs: Tabs | null;
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
