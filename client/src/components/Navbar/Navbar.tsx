import { useState } from 'react';
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
  Group,
  Menu,
  rem,
  ScrollArea,
  SegmentedControl,
  Text,
  Title,
  UnstyledButton,
  useComputedColorScheme,
  useMantineColorScheme,
} from '@mantine/core';
import { LinksGroup } from '../NavbarLinksGroup/NavbarLinksGroup';
import classes from './Navbar.module.css';

const tabs = {
  general: [
    { label: 'Seasons', icon: IconFlag, link: '/seasons' },
    { label: 'Graduates', icon: IconSchool, link: '/users' },
  ],
  season: [
    { label: 'Leetcode', icon: IconCode, link: '' },
    { label: 'Mocks', icon: IconBarbell, link: '' },
    { label: 'Students', icon: IconUsers, link: '' },
    { label: 'Resources', icon: IconFolder, link: '' },
  ],
};

export function Navbar() {
  const { setColorScheme } = useMantineColorScheme();
  const [section, setSection] = useState<'general' | 'season'>('general');
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });

  const links = tabs[section].map((item) => <LinksGroup {...item} key={item.label} />);

  return (
    <nav className={classes.navbar}>
      <div className={classes.header}>
        <Group justify="space-between">
          <Title order={1} size="h3">
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

      <div className={classes.footer}>
        <Menu shadow="md" width={200} position="bottom" withArrow openDelay={100} closeDelay={400}>
          <Menu.Target>
            <UnstyledButton className={classes.user}>
              <Group>
                <Avatar radius="xl" />
                <div style={{ flex: 1 }}>
                  <Text size="sm" fw={500}>
                    Ravi Hammond
                  </Text>

                  <Text c="dimmed" size="xs">
                    ravihammond@gmail.com
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
            <Menu.Item leftSection={<IconSettings style={{ width: rem(14), height: rem(14) }} />}>
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
            <Menu.Item leftSection={<IconLogout style={{ width: rem(14), height: rem(14) }} />}>
              Sign Out
            </Menu.Item>
          </Menu.Dropdown>
        </Menu>
      </div>
    </nav>
  );
}
