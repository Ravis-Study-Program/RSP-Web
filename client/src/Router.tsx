import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from './components/Layout/Layout';
import { createRouteMeta } from './components/RouteMeta/createRouteMeta';
import AdminRouteGuard from './shared/auth/AdminRouteGuard';
import SeasonRoleViewRouter from './shared/auth/SeasonRoleViewRouter';
import SeasonRouteGuard from './shared/auth/SeasonRouteGuard';
import UnverifiedUserAuthGuard from './shared/auth/UnverifiedUserAuthGuard';
import VerifiedUserAuthGuard from './shared/auth/VerifiedUserAuthGuard';

const AdminEnrollmentsPage = lazy(() => import('./pages/Admin/Enrollments/AdminEnrollments.page'));
const AdminMentorshipsPage = lazy(() => import('./pages/Admin/Mentorships/AdminMentorships.page'));
const AdminSeasonsPage = lazy(() => import('./pages/Admin/Seasons/AdminSeasons.page'));
const AdminSeasonWeeksPage = lazy(() => import('./pages/Admin/SeasonWeeks/AdminSeasonWeeks.page'));
const AdminUsersPage = lazy(() => import('./pages/Admin/Users/AdminUsers.page'));
const EmailVerificiationPage = lazy(
  () => import('./pages/EmailVerification/EmailVerification.page')
);
const GraduatesPage = lazy(() => import('./pages/Graduates/Graduates.page'));
const LeetcodePage = lazy(() => import('./pages/Leetcode/Leetcode.page'));
const LoginPage = lazy(() => import('./pages/Login/Login.page'));
const MenteesPage = lazy(() => import('./pages/Mentees/Mentees.page'));
const MockInterviewPage = lazy(() => import('./pages/MockInterviews/MockInterview.page'));
const NotFoundPage = lazy(() => import('./pages/NotFound/NotFound.page'));
const ProfilePage = lazy(() => import('./pages/Profile/Profile.page'));
const ResourcesPage = lazy(() => import('./pages/Resources/Resources.page'));
const SeasonUsersPage = lazy(() => import('./pages/SeasonUsers/SeasonUsers.page'));
const SeasonsPage = lazy(() => import('./pages/Seasons/Seasons.page'));
const SeasonsOverviewPage = lazy(() => import('./pages/Seasons/SeasonsOverview.page'));

const routes = (
  <Routes>
    {/* Authenticated & Unverified User Routes */}
    <Route element={<UnverifiedUserAuthGuard />}>
      {createRouteMeta({
        path: 'verify-email',
        title: 'Email Verification | RSP',
        element: <EmailVerificiationPage />,
      })}
    </Route>

    {/* Authenticated & Verified User Routes */}
    <Route element={<VerifiedUserAuthGuard />}>
      {/* Wrap all authenticated routes with Layout */}
      <Route element={<Layout />}>
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
        {createRouteMeta({
          path: 'graduates',
          title: 'Graduates | RSP',
          element: <GraduatesPage />,
        })}
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

      {createRouteMeta({ path: '*', title: '404 Not Found | RSP', element: <NotFoundPage /> })}
    </Route>

    {/* Unprotected Routes */}
    {createRouteMeta({ path: '/', title: 'Login | RSP', element: <LoginPage /> })}
    {createRouteMeta({ path: '*', title: '404 Not Found | RSP', element: <NotFoundPage /> })}
  </Routes>
);

const AppRoutes = () => <Suspense fallback={null}>{routes}</Suspense>;

export default AppRoutes;
