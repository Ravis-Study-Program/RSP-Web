import { QueryClient } from 'react-query';
import { createRoutesFromElements, Route } from 'react-router-dom';
import { Text } from '@mantine/core';
import getSeasonRoleLoader from './actions/season/getSeasonRoleLoader';
import getSeasonsLoader from './actions/season/getSeasons';
import { Layout } from './components/Layout/Layout';
import { GraduatesPage } from './pages/Graduates/Graduates.page';
import { LoginPage } from './pages/Login/Login.page';
import { NotFoundPage } from './pages/NotFound/NotFound.page';
import { SeasonsPage } from './pages/Seasons/Seasons.page';
import { SettingsPage } from './pages/Settings/Settings.page';
import AdminRouteGuard from './shared/auth/AdminRouteGuard';
import AuthRouteGuard from './shared/auth/AuthRouteGuard';
import SeasonRouteGuard from './shared/auth/SeasonRouteGuard';

const placeholderPage = (title: string) => {
  return (
    <Layout>
      <Text size="lg" c="dimmed">
        {title}
      </Text>
    </Layout>
  );
};

const routes = (queryClient: QueryClient) => {
  return createRoutesFromElements(
    <>
      {/* Authenticated Routes */}
      <Route element={<AuthRouteGuard />}>
        <Route path="/test-backend" element={placeholderPage('Test Backend')} />

        <Route path="/profile" element={placeholderPage('Profile Page')} />
        <Route path="/seasons" element={<SeasonsPage />} />
        <Route path="/settings" element={<SettingsPage />} />
        <Route path="/graduates" element={<GraduatesPage />} />
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

        <Route path="*" element={<NotFoundPage />} />
      </Route>

      {/* Unprotected Routes */}
      <Route path="/" element={<LoginPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </>
  );
};

export default routes;
