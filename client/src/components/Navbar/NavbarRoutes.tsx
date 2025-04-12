import {
  IconBrandLeetcode,
  IconCalendarMonth,
  IconChalkboard,
  IconChessKnight,
  IconFiles,
  IconLayoutDashboard,
  IconPlant,
  IconPlayCard7,
  IconSchool,
  IconSettings,
  IconTrophy,
  IconUser,
  IconUsers,
  IconUsersGroup,
  TablerIcon,
} from '@tabler/icons-react';
import { SpotlightActionData } from '@mantine/spotlight';
import { SeasonRole } from '@/generated/api/client';

export function createNonAdminSpotlightActions(
  navigate: (path: string) => void
): SpotlightActionData[] {
  return [
    {
      id: 'seasons',
      label: 'Seasons',
      description: 'Browse all program seasons',
      onClick: () => navigate('/seasons'),
      leftSection: <IconCalendarMonth size={24} stroke={1.5} />,
    },
    {
      id: 'graduates',
      label: 'Graduates',
      description: 'View graduate profiles',
      onClick: () => navigate('/graduates'),
      leftSection: <IconSchool size={24} stroke={1.5} />,
    },
    {
      id: 'leetcode',
      label: 'All Leetcode',
      description: 'Access all Leetcode practice sets',
      onClick: () => navigate('/leetcode'),
      leftSection: <IconBrandLeetcode size={24} stroke={1.5} />,
    },
    {
      id: 'mock-interviews',
      label: 'All Mock Interviews',
      description: 'Practice mock interviews',
      onClick: () => navigate('/mock-interviews'),
      leftSection: <IconChalkboard size={24} stroke={1.5} />,
    },
    {
      id: 'settings',
      label: 'Settings',
      description: 'Adjust your preferences',
      onClick: () => navigate('/settings'),
      leftSection: <IconSettings size={24} stroke={1.5} />,
    },
    {
      id: 'profile',
      label: 'Profile',
      description: 'View and edit your profile',
      onClick: () => navigate('/profile'),
      leftSection: <IconUser size={24} stroke={1.5} />,
    },
  ];
}

export interface TabItem {
  label: string;
  icon: TablerIcon;
  link?: string;
  links?: TabItem[];
  hidden?: boolean;
}

export interface Tabs {
  general: TabItem[];
  season?: TabItem[];
}

const noSeasonSelectedTabs: Tabs = {
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconSchool, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconBrandLeetcode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
};

const adminNoSeasonSelectedTabs: Tabs = {
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/admin/seasons', hidden: false },
    { label: 'Season Weeks', icon: IconPlayCard7, link: '/admin/season-weeks', hidden: false },
    { label: 'Users', icon: IconUsersGroup, link: '/admin/users', hidden: false },
    { label: 'Enrollments', icon: IconSchool, link: '/admin/enrollments', hidden: false },
    { label: 'Mentorships', icon: IconChessKnight, link: '/admin/mentorships', hidden: false },
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
};

export function createAdminSpotlightActions(
  navigate: (path: string) => void
): SpotlightActionData[] {
  return [
    {
      id: 'admin-seasons',
      label: 'Seasons',
      description: 'Manage all seasons',
      onClick: () => navigate('/admin/seasons'),
      leftSection: <IconCalendarMonth size={24} stroke={1.5} />,
    },
    {
      id: 'season-weeks',
      label: 'Season Weeks',
      description: 'View and edit season calendar weeks',
      onClick: () => navigate('/admin/season-weeks'),
      leftSection: <IconPlayCard7 size={24} stroke={1.5} />,
    },
    {
      id: 'users',
      label: 'Users',
      description: 'Browse and manage all users',
      onClick: () => navigate('/admin/users'),
      leftSection: <IconUsersGroup size={24} stroke={1.5} />,
    },
    {
      id: 'enrollments',
      label: 'Enrollments',
      description: 'Manage program enrollments',
      onClick: () => navigate('/admin/enrollments'),
      leftSection: <IconSchool size={24} stroke={1.5} />,
    },
    {
      id: 'mentorships',
      label: 'Mentorships',
      description: 'Oversee mentorship assignments',
      onClick: () => navigate('/admin/mentorships'),
      leftSection: <IconChessKnight size={24} stroke={1.5} />,
    },
    {
      id: 'settings',
      label: 'Settings',
      description: 'Adjust administrator settings',
      onClick: () => navigate('/settings'),
      leftSection: <IconSettings size={24} stroke={1.5} />,
    },
    {
      id: 'profile',
      label: 'Profile',
      description: 'Edit your administrator profile',
      onClick: () => navigate('/profile'),
      leftSection: <IconUser size={24} stroke={1.5} />,
    },
  ];
}

const getStudentTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconSchool, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconBrandLeetcode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
  season: seasonSlug
    ? [
        {
          label: seasonSlug,
          icon: IconTrophy,
          links: [
            {
              label: seasonSlug,
              icon: IconChessKnight,
              link: `/seasons/${seasonSlug}`,
              hidden: true,
            },
            {
              label: 'Overview',
              icon: IconLayoutDashboard,
              link: `/seasons/${seasonSlug}/overview`,
              hidden: false,
            },
            {
              label: 'Leetcode',
              icon: IconBrandLeetcode,
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
              label: 'Season Users',
              icon: IconUsers,
              link: `/seasons/${seasonSlug}/users`,
              hidden: false,
            },
            {
              label: 'Resources',
              icon: IconFiles,
              link: `/seasons/${seasonSlug}/resources`,
              hidden: false,
            },
          ],
          hidden: false,
        },
      ]
    : [],
});

const getMentorTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconSchool, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconBrandLeetcode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
  season: seasonSlug
    ? [
        {
          label: seasonSlug,
          icon: IconTrophy,
          links: [
            {
              label: seasonSlug,
              icon: IconChessKnight,
              link: `/seasons/${seasonSlug}`,
              hidden: true,
            },
            {
              label: 'Overview',
              icon: IconLayoutDashboard,
              link: `/seasons/${seasonSlug}/overview`,
              hidden: false,
            },
            {
              label: 'Season Users',
              icon: IconUsers,
              link: `/seasons/${seasonSlug}/users`,
              hidden: false,
            },
            {
              label: 'Mentees',
              icon: IconPlant,
              link: `/seasons/${seasonSlug}/mentees`,
              hidden: false,
            },
            {
              label: 'Resources',
              icon: IconFiles,
              link: `/seasons/${seasonSlug}/resources`,
              hidden: false,
            },
          ],
          hidden: false,
        },
      ]
    : [],
});

const getCoordinatorTabs = (seasonSlug: string | null): Tabs => ({
  general: [
    { label: 'Seasons', icon: IconCalendarMonth, link: '/seasons', hidden: false },
    { label: 'Graduates', icon: IconSchool, link: '/graduates', hidden: false },
    { label: 'All Leetcode', icon: IconBrandLeetcode, link: '/leetcode', hidden: false },
    { label: 'All Mock Interviews', icon: IconChalkboard, link: '/mock-interviews', hidden: false },
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
  season: seasonSlug
    ? [
        {
          label: seasonSlug,
          icon: IconTrophy,
          links: [
            {
              label: seasonSlug,
              icon: IconChessKnight,
              link: `/seasons/${seasonSlug}`,
              hidden: true,
            },
            {
              label: 'Overview',
              icon: IconLayoutDashboard,
              link: `/seasons/${seasonSlug}/overview`,
              hidden: false,
            },
            {
              label: 'Season Users',
              icon: IconUsers,
              link: `/seasons/${seasonSlug}/users`,
              hidden: false,
            },
            {
              label: 'Mentors',
              icon: IconBrandLeetcode,
              link: `/seasons/${seasonSlug}/mentors`,
              hidden: false,
            },
            {
              label: 'Resources',
              icon: IconFiles,
              link: `/seasons/${seasonSlug}/resources`,
              hidden: false,
            },
          ],
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
    { label: 'Settings', icon: IconSettings, link: '/settings', hidden: true },
    { label: 'Profile', icon: IconUser, link: '/profile', hidden: true },
  ],
  season: seasonSlug
    ? [
        {
          label: seasonSlug,
          icon: IconChessKnight,
          link: `/seasons/${seasonSlug}`,
          hidden: true,
        },
        {
          label: 'Overview',
          icon: IconLayoutDashboard,
          link: `/seasons/${seasonSlug}/overview`,
          hidden: false,
        },
        {
          label: 'Season Users',
          icon: IconUsersGroup,
          link: `/seasons/${seasonSlug}/users`,
          hidden: false,
        },
        {
          label: 'Mentors',
          icon: IconUsers,
          link: `/seasons/${seasonSlug}/mentors`,
          hidden: false,
        },
        {
          label: 'Resources',
          icon: IconFiles,
          link: `/seasons/${seasonSlug}/resources`,
          hidden: false,
        },
      ]
    : [],
});

export const getTabs = (seasonSlug: string | null, isAdmin: boolean, role: SeasonRole | null) => {
  let tabs: Tabs = noSeasonSelectedTabs;

  if (isAdmin) {
    tabs =
      seasonSlug === null || seasonSlug === ''
        ? adminNoSeasonSelectedTabs
        : getAdminTabs(seasonSlug);
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
  const allTabs = [...tabs.general, ...(tabs.season || [])];

  // Search through link and links if applicable
  const search = (items: TabItem[]): TabItem | undefined => {
    for (const item of items) {
      if (item.link === link) {
        return item;
      }
      if (item.links) {
        const found = search(item.links);
        if (found) {
          return found;
        }
      }
    }
    return undefined;
  };

  return search(allTabs);
};
