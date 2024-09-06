import { QueryClient } from 'react-query';
import { createRoutesFromElements, Route } from 'react-router-dom';
import {
  Anchor,
  AppShell,
  Breadcrumbs,
  Burger,
  Container,
  Group,
  Text,
  Title,
} from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import getSeasonRoleLoader from './actions/season/getSeasonRoleLoader';
import getSeasonsLoader from './actions/season/getSeasons';
import { Navbar } from './components/Navbar/Navbar';
import AdminRouteGuard from './shared/auth/AdminRouteGuard';
import AuthRouteGuard from './shared/auth/AuthRouteGuard';
import SeasonRouteGuard from './shared/auth/SeasonRouteGuard';
import classes from './Router.module.css';

const placeholderPage = (title: string) => {
  return (
    <Container p={0} size={600}>
      <Text size="lg" c="dimmed">
        {title}
      </Text>
    </Container>
  );
};

export function Test() {
  const [opened, { toggle }] = useDisclosure();

  const items = [
    { title: 'Seasons', href: '#' },
    { title: 'ADL-2023', href: '#' },
    { title: 'Overview', href: '#' },
  ].map((item, index) => (
    <Anchor href={item.href} key={index} className={classes.breadcrumb_links}>
      {item.title}
    </Anchor>
  ));

  return (
    <AppShell
      layout="alt"
      header={{ height: 65 }}
      navbar={{ width: 300, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="md"
    >
      <AppShell.Header>
        <Group h="100%" px="md">
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          <Breadcrumbs>{items}</Breadcrumbs>
        </Group>
      </AppShell.Header>
      <AppShell.Navbar p="md" className={classes.navbar}>
        <Group className={classes.mobile_nav_header}>
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          <Title className={classes.navbar_header_text} order={1} size="h3">
            Ravi Study Program
          </Title>
        </Group>
        <Navbar />
      </AppShell.Navbar>
      <AppShell.Main>
        Some Content Here
      </AppShell.Main>
    </AppShell>
  );
}

const routes = (queryClient: QueryClient) => {
  return createRoutesFromElements(
    <>
      {/* Authenticated Routes */}
      <Route element={<AuthRouteGuard />}>
        <Route path="/test-backend" element={placeholderPage('Test Backend')} />

        <Route path="/settings" element={placeholderPage('Settings Page')} />
        <Route path="/profile" element={placeholderPage('Profile Page')} />
        <Route path="/seasons" element={placeholderPage('General Season+')} />
        <Route path="/resources" element={placeholderPage('Resources Page')} />
        <Route path="/leetcode" element={placeholderPage('Leetcode Page')} />
        <Route path="/mocks" element={placeholderPage('Mock Interviews Page')} />

        {/* Admin Routes */}
        <Route path="admin" element={<AdminRouteGuard />}>
          <Route
            path="seasons"
            element={placeholderPage('Admin Seasons')}
            loader={getSeasonsLoader(queryClient)}
          />
          <Route path="users" element={placeholderPage('Admin: Users page')} />
        </Route>

        {/* Season Routes */}
        <Route
          path=":seasonSlug"
          element={<SeasonRouteGuard />}
          loader={getSeasonRoleLoader(queryClient)}
          id="season"
        >
          <Route path="students" element={placeholderPage('StudentList Page')} />
          <Route path="mentees" element={placeholderPage('MenteesList Page')} />
          <Route path="mentors" element={placeholderPage('MentorsList Page')} />
        </Route>
      </Route>

      {/* Unprotected Routes */}
      <Route path="/" element={<Test />} />
      <Route path="/login" element={placeholderPage('Login Page')} />
      <Route path="/not-found" element={placeholderPage('404 Page')} />
    </>
  );
};

export default routes;
