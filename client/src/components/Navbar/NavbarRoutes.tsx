import {
  IconCalendarMonth,
  IconChalkboard,
  IconChessKnight,
  IconCode,
  IconFlag2,
  IconFolders,
  IconLink,
  IconSchool,
  IconSeeding,
  IconSettings,
  IconStar,
  IconUser,
  IconUsersGroup,
  TablerIcon,
} from '@tabler/icons-react';
import { SeasonRole } from '@/generated/api/client';

export interface TabItem {
  label: string;
  icon: TablerIcon;
  link: string;
  hidden: boolean;
}

export interface Tabs {
  general: TabItem[];
  season?: TabItem[];
}

const noSeasonSelectedTabs: Tabs = {
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconSchool, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconCode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
};

const adminNoSeasonSelectedTabs: Tabs = {
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/admin/seasons', hidden: false },
    { label: 'Users', icon: IconUsersGroup, link: '/admin/users', hidden: false },
    { label: 'Enrollments', icon: IconSchool, link: '/admin/enrollments', hidden: false },
    { label: 'Mentorships', icon: IconChessKnight, link: '/admin/mentorships', hidden: false },
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
};

const getStudentTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconUsersGroup, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconCode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
  ],
  season: seasonSlug
    ? [
        { label: seasonSlug, icon: IconLink, link: `/seasons/${seasonSlug}`, hidden: true },
        {
          label: 'Leetcode',
          icon: IconCode,
          link: `/seasons/${seasonSlug}/leetcode`,
          hidden: false,
        },
        {
          label: 'Mock Interviews',
          icon: IconChalkboard,
          link: `/seasons/${seasonSlug}/mock-interviews`,
          hidden: false,
        },
        {
          label: 'Students',
          icon: IconSeeding,
          link: `/seasons/${seasonSlug}/students`,
          hidden: false,
        },
        {
          label: 'Resources',
          icon: IconFolders,
          link: `/seasons/${seasonSlug}/resources`,
          hidden: false,
        },
      ]
    : [],
});

const getMentorTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconUsersGroup, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconCode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
  ],
  season: seasonSlug
    ? [
        { label: seasonSlug, icon: IconLink, link: `/seasons/${seasonSlug}`, hidden: true },
        {
          label: 'Students',
          icon: IconSeeding,
          link: `/seasons/${seasonSlug}/students`,
          hidden: false,
        },
        {
          label: 'Mentees',
          icon: IconFlag2,
          link: `/seasons/${seasonSlug}/mentees`,
          hidden: false,
        },
        {
          label: 'Resources',
          icon: IconFolders,
          link: `/seasons/${seasonSlug}/resources`,
          hidden: false,
        },
      ]
    : [],
});

const getCoordinatorTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconUsersGroup, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconCode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
  ],
  season: seasonSlug
    ? [
        { label: seasonSlug, icon: IconLink, link: `/seasons/${seasonSlug}`, hidden: true },
        {
          label: 'Students',
          icon: IconSeeding,
          link: `/seasons/${seasonSlug}/students`,
          hidden: false,
        },
        { label: 'Mentors', icon: IconCode, link: `/seasons/${seasonSlug}/mentors`, hidden: false },
        {
          label: 'Resources',
          icon: IconFolders,
          link: `/seasons/${seasonSlug}/resources`,
          hidden: false,
        },
      ]
    : [],
});

const getAdminTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/admin/seasons', hidden: false },
    { label: 'Users', icon: IconUsersGroup, link: '/admin/users', hidden: false },
    { label: 'Enrollments', icon: IconSchool, link: '/admin/enrollments', hidden: false },
    { label: 'Mentorships', icon: IconChessKnight, link: '/admin/mentorships', hidden: false },
  ],
  season: seasonSlug
    ? [
        { label: seasonSlug, icon: IconLink, link: `/seasons/${seasonSlug}`, hidden: true },
        {
          label: 'Students',
          icon: IconSeeding,
          link: `/seasons/${seasonSlug}/students`,
          hidden: false,
        },
        { label: 'Mentors', icon: IconStar, link: `/seasons/${seasonSlug}/mentors`, hidden: false },
        {
          label: 'Resources',
          icon: IconFolders,
          link: `/seasons/${seasonSlug}/resources`,
          hidden: false,
        },
      ]
    : [],
});

export const getTabs = (seasonSlug: string | null, isAdmin: boolean, role: SeasonRole | null) => {
  let tabs: Tabs = noSeasonSelectedTabs;
  
  if (isAdmin) {    
    tabs = (seasonSlug === null || seasonSlug === '') ? adminNoSeasonSelectedTabs : getAdminTabs(seasonSlug);    
    return tabs;
  } 
  
  if (role == null) {
    return tabs;
  }

  switch (role) {
    case SeasonRole.Student:
      tabs = getStudentTabs(seasonSlug);
      break;
    case SeasonRole.Mentor:
      tabs = getMentorTabs(seasonSlug);
      break;
    case SeasonRole.Coordinator:
      tabs = getCoordinatorTabs(seasonSlug);
      break;
  }

  return tabs;
};

export const lookupTabByLink = (
  link: string,
  seasonSlug: string | null,
  isAdmin: boolean,
  role: SeasonRole | null
): TabItem | undefined => {
  const tabs = getTabs(seasonSlug, isAdmin, role);
  const tabsLink = [...tabs.general, ...(tabs.season || [])];
  return tabsLink.find((tab) => tab.link === link);
};
