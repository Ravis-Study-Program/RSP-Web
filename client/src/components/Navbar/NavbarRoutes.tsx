import {
  IconBadge,
  IconCalendarMonth,
  IconChalkboard,
  IconCode,
  IconFlag2,
  IconFolders,
  IconSchool,
  IconSeeding,
  IconStar,
  IconUsersGroup,
  TablerIcon,
} from '@tabler/icons-react';

export interface TabItem {
  label: string;
  icon: TablerIcon;
  link: string;
}

export interface Tabs {
  general: TabItem[];
  season?: TabItem[];
}

const noSeasonSelectedTabs: Tabs = {
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons' },
    { label: 'Graduates', icon: IconSchool, link: '/graduates' },
  ],
};

const adminNoSeasonSelectedTabs: Tabs = {
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/admin/seasons' },
    { label: 'Users', icon: IconUsersGroup, link: '/admin/users' },
    { label: 'Roles', icon: IconBadge, link: '/admin/roles' },
    { label: 'Enrollments', icon: IconSchool, link: '/admin/enrollments' },
  ],
};

const getStudentTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons' },
    { label: 'Graduates', icon: IconUsersGroup, link: '/graduates' },
    { label: 'All Leetcode', icon: IconCode, link: '/leetcode' },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews' },
  ],
  season: seasonSlug
    ? [
        { label: 'Leetcode', icon: IconCode, link: `/season/${seasonSlug}/leetcode` },
        {
          label: 'Mock Interviews',
          icon: IconChalkboard,
          link: `/season/${seasonSlug}/mock-interviews`,
        },
        { label: 'All Students', icon: IconSeeding, link: `/season/${seasonSlug}/students` },
        { label: 'Resources', icon: IconFolders, link: `/season/${seasonSlug}/resources` },
      ]
    : [],
});

const getMentorTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons' },
    { label: 'Graduates', icon: IconUsersGroup, link: '/graduates' },
    { label: 'All Leetcode', icon: IconCode, link: '/leetcode' },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews' },
  ],
  season: seasonSlug
    ? [
        { label: 'Students', icon: IconSeeding, link: `/season/${seasonSlug}/students` },
        { label: 'Mentees', icon: IconFlag2, link: `/season/${seasonSlug}/mentees` },
        { label: 'Resources', icon: IconFolders, link: `/season/${seasonSlug}/resources` },
      ]
    : [],
});

const getCoordinatorTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons' },
    { label: 'Graduates', icon: IconUsersGroup, link: '/graduates' },
    { label: 'All Leetcode', icon: IconCode, link: '/leetcode' },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews' },
  ],
  season: seasonSlug
    ? [
        { label: 'Students', icon: IconSeeding, link: `/season/${seasonSlug}/students` },
        { label: 'Mentors', icon: IconCode, link: `/season/${seasonSlug}/mentors` },
        { label: 'Resources', icon: IconFolders, link: `/season/${seasonSlug}/resources` },
      ]
    : [],
});

const getAdminTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/admin/seasons' },
    { label: 'Users', icon: IconUsersGroup, link: '/admin/users' },
    { label: 'Roles', icon: IconBadge, link: '/admin/roles' },
    { label: 'Enrollments', icon: IconSchool, link: '/admin/enrollments' },
  ],
  season: seasonSlug
    ? [
        { label: 'Students', icon: IconSeeding, link: `/season/${seasonSlug}/students` },
        { label: 'Mentors', icon: IconStar, link: `/season/${seasonSlug}/mentors` },
        { label: 'Resources', icon: IconFolders, link: `/season/${seasonSlug}/resources` },
      ]
    : [],
});

export const getTabs = (seasonSlug: string | null, isAdmin: boolean, roleName: string) => {
  let tabs: Tabs | null = null;
  if (isAdmin) {
    tabs = seasonSlug === null ? adminNoSeasonSelectedTabs : getAdminTabs(seasonSlug);
  } else {
    switch (roleName) {
      case 'Student':
        tabs = getStudentTabs(seasonSlug);
        break;
      case 'Mentor':
        tabs = getMentorTabs(seasonSlug);
        break;
      case 'Coordinator':
        tabs = getCoordinatorTabs(seasonSlug);
        break;
      default:
        tabs = noSeasonSelectedTabs;
    }
  }

  return tabs;
};
