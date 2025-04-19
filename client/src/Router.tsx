import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { createRouteMeta } from './components/RouteMeta/createRouteMeta';
import AdminSeasonWeeksPage from './pages/Admin/SeasonWeeks/AdminSeasonWeeks.page';
import ProfilePage from './pages/Profile/Profile.page';
import ResourcesPage from './pages/Resources/Resources.page';
import SeasonUsersPage from './pages/SeasonUsers/SeasonUsers.page';
import AdminRouteGuard from './shared/auth/AdminRouteGuard';
import AuthRouteGuard from './shared/auth/AuthRouteGuard';
import SeasonRoleViewRouter from './shared/auth/SeasonRoleViewRouter';
import SeasonRouteGuard from './shared/auth/SeasonRouteGuard';

const AdminEnrollmentsPage = lazy(() => import('./pages/Admin/Enrollments/AdminEnrollments.page'));
const AdminMentorshipsPage = lazy(() => import('./pages/Admin/Mentorships/AdminMentorships.page'));
const AdminSeasonsPage = lazy(() => import('./pages/Admin/Seasons/AdminSeasons.page'));
const AdminUsersPage = lazy(() => import('./pages/Admin/Users/AdminUsers.page'));
const GraduatesPage = lazy(() => import('./pages/Graduates/Graduates.page'));
const LeetcodePage = lazy(() => import('./pages/Leetcode/Leetcode.page'));
const LoginPage = lazy(() => import('./pages/Login/Login.page'));
const MenteesPage = lazy(() => import('./pages/Mentees/Mentees.page'));
const MockInterviewPage = lazy(() => import('./pages/MockInterviews/MockInterview.page'));
const NotFoundPage = lazy(() => import('./pages/NotFound/NotFound.page'));
const SeasonsPage = lazy(() => import('./pages/Seasons/Seasons.page'));
const SeasonsOverviewPage = lazy(() => import('./pages/Seasons/SeasonsOverview.page'));
const SettingsPage = lazy(() => import('./pages/Settings/Settings.page'));

const routes = (
  <Routes>
    {/* Authenticated Routes */}
    <Route element={<AuthRouteGuard />}>
      {/* Admin Routes */}
      <Route path="admin" element={<AdminRouteGuard />}>
        {createRouteMeta({
          path: 'seasons',
          title: 'Admin Seasons | RSP',
          element: <AdminSeasonsPage />,
        })}
        {createRouteMeta({
          path: 'season-weeks',
          title: 'Admin Season Weeks | RSP',
          element: <AdminSeasonWeeksPage />,
        })}
        {createRouteMeta({
          path: 'users',
          title: 'Admin Users | RSP',
          element: <AdminUsersPage />,
        })}
        {createRouteMeta({
          path: 'enrollments',
          title: 'Admin Enrollments | RSP',
          element: <AdminEnrollmentsPage />,
        })}
        {createRouteMeta({
          path: 'mentorships',
          title: 'Admin Mentorships | RSP',
          element: <AdminMentorshipsPage />,
        })}
      </Route>

      {createRouteMeta({ path: 'profile', title: 'Profile | RSP', element: <ProfilePage /> })}
      {createRouteMeta({ path: 'seasons', title: 'Seasons | RSP', element: <SeasonsPage /> })}
      {createRouteMeta({ path: 'settings', title: 'Settings | RSP', element: <SettingsPage /> })}
      {createRouteMeta({ path: 'graduates', title: 'Graduates | RSP', element: <GraduatesPage /> })}
      {createRouteMeta({ path: 'leetcode', title: 'Leetcode | RSP', element: <LeetcodePage /> })}
      {createRouteMeta({
        path: 'mock-interviews',
        title: 'Mock Interviews | RSP',
        element: <MockInterviewPage />,
      })}

      <Route path="seasons/:seasonSlug" element={<SeasonRouteGuard />}>
        <Route index element={<Navigate to="overview" replace />} />
        {createRouteMeta({
          path: 'overview',
          title: 'Season Overview | RSP',
          element: <SeasonsOverviewPage />,
        })}
        {createRouteMeta({
          path: 'users',
          title: 'Season Users | RSP',
          element: <SeasonUsersPage />,
        })}
        {createRouteMeta({
          path: 'mentees',
          title: 'Season Mentees | RSP',
          element: <SeasonRoleViewRouter mentorView={<MenteesPage />} />,
        })}
        {createRouteMeta({
          path: 'mentors',
          title: 'Season Mentors | RSP',
          element: <MenteesPage />,
        })}
        {createRouteMeta({
          path: 'leetcode',
          title: 'Season Leetcode | RSP',
          element: <LeetcodePage />,
        })}
        {createRouteMeta({
          path: 'mock-interviews',
          title: 'Season Mock Interviews | RSP',
          element: <MockInterviewPage />,
        })}
        {createRouteMeta({
          path: 'resources',
          title: 'Season Resources | RSP',
          element: <ResourcesPage />,
        })}
        {createRouteMeta({ path: 'profile', title: 'Profile | RSP', element: <ProfilePage /> })}
      </Route>

      {createRouteMeta({ path: '*', title: '404 Not Found | RSP', element: <NotFoundPage /> })}
    </Route>

    {/* Unprotected Routes */}
    {createRouteMeta({ path: '/', title: 'Login | RSP', element: <LoginPage /> })}
    {createRouteMeta({ path: '*', title: '404 Not Found | RSP', element: <NotFoundPage /> })}
  </Routes>
);

const AppRoutes = () => <Suspense fallback={null}>{routes}</Suspense>;

export default AppRoutes;
