import { createRoutesFromElements, Route } from 'react-router-dom';
import { Text } from '@mantine/core';
import { Layout } from './components/Layout/Layout';
import { AdminEnrollmentsPage } from './pages/Admin/Enrollments/AdminEnrollments.page';
import { AdminRolesPage } from './pages/Admin/Roles/AdminRoles.page';
import { AdminSeasonsPage } from './pages/Admin/Seasons/AdminSeasons.page';
import { AdminUsersPage } from './pages/Admin/Users/AdminUsers.page';
import { GraduatesPage } from './pages/Graduates/Graduates.page';
import { LeetcodePage } from './pages/Leetcode/Leetcode.page';
import { LoginPage } from './pages/Login/Login.page';
import { NotFoundPage } from './pages/NotFound/NotFound.page';
import { SeasonsPage } from './pages/Seasons/Seasons.page';
import { SeasonsOverviewPage } from './pages/Seasons/SeasonsOverview.page';
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

const routes = createRoutesFromElements(
  <>
    {/* Authenticated Routes */}
    <Route element={<AuthRouteGuard />}>
      {/* Admin Routes */}
      <Route path="admin" element={<AdminRouteGuard />}>
        <Route path="seasons" element={<AdminSeasonsPage />} />
        <Route path="users" element={<AdminUsersPage />} />
        <Route path="roles" element={<AdminRolesPage />} />
        <Route path="enrollments" element={<AdminEnrollmentsPage />} />
      </Route>

      <Route path="profile" element={placeholderPage('Profile Page')} />
      <Route path="seasons" element={<SeasonsPage />} />
      <Route path="settings" element={<SettingsPage />} />
      <Route path="graduates" element={<GraduatesPage />} />
      <Route path="leetcode" element={<LeetcodePage />} />
      <Route path="mock-interviews" element={placeholderPage('All Mock Interviews Page')} />

      {/* Season Routes */}
      <Route path="seasons/:seasonSlug" element={<SeasonRouteGuard />}>
        <Route index element={<SeasonsOverviewPage />} />
        <Route path="students" element={placeholderPage('StudentList Page')} />
        <Route path="mentees" element={placeholderPage('MenteesList Page')} />
        <Route path="mentors" element={placeholderPage('MentorsList Page')} />
        <Route path="leetcode" element={placeholderPage('Leetcode Page')} />
        <Route path="mock-interviews" element={placeholderPage('Mock Interviews Page')} />
        <Route path="resources" element={placeholderPage('Resources Page')} />
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Route>

    {/* Unprotected Routes */}
    <Route path="/" element={<LoginPage />} />
    <Route path="*" element={<NotFoundPage />} />
  </>
);

export default routes;
