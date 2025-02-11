import React, { lazy, Suspense } from 'react';
import { Route, Routes } from 'react-router-dom';
import AdminSeasonWeeksPage from './pages/Admin/SeasonWeeks/AdminSeasonWeeks.page';
import ProfilePage from './pages/Profile/Profile.page';
import ResourcesPage from './pages/Resources/Resources.page';
import SeasonUsersPage from './pages/SeasonUsers/SeasonUsers.page';
import AdminRouteGuard from './shared/auth/AdminRouteGuard';
import AuthRouteGuard from './shared/auth/AuthRouteGuard';
import SeasonRoleViewRouter from './shared/auth/SeasonRoleViewRouter';
import SeasonRouteGuard from './shared/auth/SeasonRouteGuard';

// Lazy load the page components
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
        <Route path="seasons" element={<AdminSeasonsPage />} />
        <Route path="season-weeks" element={<AdminSeasonWeeksPage />} />
        <Route path="users" element={<AdminUsersPage />} />
        <Route path="enrollments" element={<AdminEnrollmentsPage />} />
        <Route path="mentorships" element={<AdminMentorshipsPage />} />
      </Route>

      <Route path="profile" element={<ProfilePage />} />
      <Route path="seasons" element={<SeasonsPage />} />
      <Route path="settings" element={<SettingsPage />} />
      <Route path="graduates" element={<GraduatesPage />} />
      <Route path="leetcode" element={<LeetcodePage />} />
      <Route path="mock-interviews" element={<MockInterviewPage />} />

      {/* Season Routes */}
      <Route path="seasons/:seasonSlug" element={<SeasonRouteGuard />}>
        <Route index element={<SeasonsOverviewPage />} />
        <Route path="users" element={<SeasonUsersPage />} />
        <Route path="mentees" element={<SeasonRoleViewRouter mentorView={<MenteesPage />} />} />
        <Route path="mentors" element={<MenteesPage />} />
        <Route path="leetcode" element={<LeetcodePage />} />
        <Route path="mock-interviews" element={<MockInterviewPage />} />
        <Route path="resources" element={<ResourcesPage />} />
        <Route path="profile" element={<ProfilePage />} />
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Route>

    {/* Unprotected Routes */}
    <Route path="/" element={<LoginPage />} />
    <Route path="*" element={<NotFoundPage />} />
  </Routes>
);

const AppRoutes = () => <Suspense fallback={null}>{routes}</Suspense>;

export default AppRoutes;
