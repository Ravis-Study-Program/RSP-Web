import { Link, Outlet } from 'react-router-dom';
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
import { useGetUserEnrollments } from '@/generated/api/client';
import { SeasonRoleReverseIndex } from '@/shared/entities/reverseIndex';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import { Navbar } from '../Navbar/Navbar';
import { getTabs, lookupTabByLink, TabItem } from '../Navbar/NavbarRoutes';
import classes from './Layout.module.css';

export function Layout() {
  const [opened, { toggle }] = useDisclosure();
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { seasonSlug, pathSegments } = useSeasonSlug();
  const { user, isAdmin, role, isLoading } = useUserAndEnrollment(seasonSlug);
  const {
    data: enrollmentsResponse,
    isFetching: isFetchingEnrollments,
    isLoading: isLoadingEnrollments,
  } = useGetUserEnrollments();

  const resourcesUrl = enrollmentsResponse?.responseBody?.enrollments?.find(
    (enrollment) => enrollment.seasonSlug === seasonSlug
  )?.seasonResourcesUrl;

  const tabs = getTabs(seasonSlug, isAdmin, role, resourcesUrl);

  const getBreadcrumbLinks = () => {
    if (isLoading || isLoadingEnrollments || isFetchingEnrollments) {
      return null;
    }

    const breadcrumbs: TabItem[] = [];
    let currentPath = '';
    for (let i = 0; i < pathSegments.length; i++) {
      currentPath += `/${pathSegments[i]}`;
      const tab = lookupTabByLink(currentPath, seasonSlug, isAdmin, role, resourcesUrl);
      if (tab) {
        breadcrumbs.push(tab);
      }
    }

    const breadcrumbLinks = breadcrumbs.map((item, index) => (
      <Anchor
        underline="never"
        href={
          item.link === `/seasons/${seasonSlug}` ? `/seasons/${seasonSlug}/overview` : item.link
        }
        key={index}
        className={classes.breadcrumb_links}
      >
        <Flex justify="center" align="center" gap={8}>
          {item.label}
          {item.link === `/seasons/${seasonSlug}` && role != null ? (
            <Badge autoContrast color="gray" variant="light" className={classes.roleBadge}>
              {SeasonRoleReverseIndex[role]}
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
            component={Link}
            to="/seasons"
            underline="never"
            c={computedColorScheme === 'light' ? 'dark' : 'white'}
          >
            <Title className={classes.navbar_header_text} order={1} size="h3">
              Ravi's Study Program
            </Title>
          </Anchor>
        </Group>
        <Navbar
          isLoading={isLoading || isLoadingEnrollments}
          user={user}
          isAdmin={isAdmin}
          tabs={tabs}
          enrollments={enrollmentsResponse?.responseBody?.enrollments || []}
          isSeasonUrl={seasonSlug !== ''}
        />
      </AppShell.Navbar>
      <AppShell.Main className={classes.main}>
        <Outlet />
      </AppShell.Main>
    </AppShell>
  );
}
