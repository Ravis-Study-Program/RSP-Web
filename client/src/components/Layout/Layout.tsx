import { ReactNode } from 'react';
import {
  Anchor,
  AppShell,
  Badge,
  Breadcrumbs,
  Burger,
  Flex,
  Group,
  Skeleton,
  Title,
  useComputedColorScheme,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { Navbar } from '../Navbar/Navbar';
import { getTabs, lookupTabByLink, TabItem } from '../Navbar/NavbarRoutes';
import classes from './Layout.module.css';

interface LayoutProps {
  children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
  const [opened, { toggle }] = useDisclosure();
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { seasonSlug, pathSegments } = useSeasonSlug();
  const { user, isAdmin, roleName, isLoading } = useUserAndEnrollment(seasonSlug);

  const getBreadcrumbLinks = () => {
    if (isLoading) {
      return null;
    }

    const breadcrumbs: TabItem[] = [];
    let currentPath = '';
    for (let i = 0; i < pathSegments.length; i++) {
      currentPath += `/${pathSegments[i]}`;
      const tab = lookupTabByLink(currentPath, seasonSlug, isAdmin, roleName);
      if (tab) {
        breadcrumbs.push(tab);
      }
    }

    const breadcrumbLinks = breadcrumbs.map((item, index) => (
      <Anchor underline="never" href={item.link} key={index} className={classes.breadcrumb_links}>
        <Flex justify="center" align="center" gap={8}>
          {item.label}
          {item.link === `/seasons/${seasonSlug}` ? (
            <Badge autoContrast color="yellow.5" className={classes.roleBadge}>
              {roleName}
            </Badge>
          ) : null}
        </Flex>
      </Anchor>
    ));

    return breadcrumbLinks;
  };

  return (
    <AppShell
      layout="alt"
      header={{ height: 65 }}
      navbar={{ width: 300, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="xl"
    >
      <AppShell.Header>
        <Group h="100%" px="md">
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          {!isLoading ? (
            <Breadcrumbs ml="md">{getBreadcrumbLinks()}</Breadcrumbs>
          ) : (
            <Skeleton ml="md" height={12} my={10} width="100px" radius="xl" />
          )}
        </Group>
      </AppShell.Header>
      <AppShell.Navbar className={classes.navbar}>
        <Group className={classes.mobile_nav_header}>
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          <Anchor
            href="/seasons"
            underline="never"
            c={computedColorScheme === 'light' ? 'dark' : 'white'}
          >
            <Title className={classes.navbar_header_text} order={1} size="h3">
              Ravi Study Program
            </Title>
          </Anchor>
        </Group>
        <Navbar isLoading={isLoading} user={user} tabs={getTabs(seasonSlug, isAdmin, roleName)} />
      </AppShell.Navbar>
      <AppShell.Main className={classes.main}>{children}</AppShell.Main>
    </AppShell>
  );
}
