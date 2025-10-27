import { useState } from 'react';
import { IconChevronRight } from '@tabler/icons-react';
import { Link } from 'react-router-dom';
import { Anchor, Box, Collapse, Group, Text, ThemeIcon, UnstyledButton } from '@mantine/core';
import { TabItem } from '../Navbar/NavbarRoutes';
import classes from './NavbarLinksGroup.module.css';

const preloadRoute = (path: string) => {
  switch (path) {
    case '/seasons':
      import('../../pages/Seasons/Seasons.page');
      break;
    case '/profile':
      import('../../pages/Profile/Profile.page');
      break;
    case '/settings':
      import('../../pages/Settings/Settings.page');
      break;
    case '/graduates':
      import('../../pages/Graduates/Graduates.page');
      break;
    case '/leetcode':
      import('../../pages/Leetcode/Leetcode.page');
      break;
    case '/mock-interviews':
      import('../../pages/MockInterviews/MockInterview.page');
      break;
    default:
      if (path.includes('/seasons/')) {
        if (path.includes('/overview')) {
          import('../../pages/Seasons/SeasonsOverview.page');
        } else if (path.includes('/users')) {
          import('../../pages/SeasonUsers/SeasonUsers.page');
        } else if (path.includes('/mentees')) {
          import('../../pages/Mentees/Mentees.page');
        } else if (path.includes('/leetcode')) {
          import('../../pages/Leetcode/Leetcode.page');
        } else if (path.includes('/mock-interviews')) {
          import('../../pages/MockInterviews/MockInterview.page');
        }
      }
  }
};

interface LinksGroupProps {
  icon: React.FC<any>;
  label: string;
  links?: TabItem[];
  link?: string;
  activeLink?: string;
  isExternal?: boolean;
}

export function LinksGroup({
  label,
  links,
  link,
  icon: Icon,
  activeLink,
  isExternal,
}: LinksGroupProps) {
  const hasLinks = Array.isArray(links);

  const [opened, setOpened] = useState(false);

  const items = (hasLinks ? links.filter((item) => !item.hidden) : []).map((innerLink) =>
    innerLink.isExternal ? (
      <Anchor
        href={innerLink.link}
        target="_blank"
        rel="noopener noreferrer"
        className={`${classes.innerLink} ${activeLink === innerLink.link ? classes.activeLink : ''}`}
        key={innerLink.label}
      >
        <Box style={{ display: 'flex', alignItems: 'center' }}>
          <ThemeIcon variant="transparent" className={classes.icon} size={32}>
            <innerLink.icon size={22} />
          </ThemeIcon>
          <Text ml="sm" size="sm" fw={500}>
            {innerLink.label}
          </Text>
        </Box>
      </Anchor>
    ) : (
      <Anchor
        component={Link}
        to={innerLink.link || ''}
        className={`${classes.innerLink} ${activeLink === innerLink.link ? classes.activeLink : ''}`}
        key={innerLink.label}
        onMouseEnter={() => innerLink.link && preloadRoute(innerLink.link)}
      >
        <Box style={{ display: 'flex', alignItems: 'center' }}>
          <ThemeIcon variant="transparent" className={classes.icon} size={32}>
            <innerLink.icon size={22} />
          </ThemeIcon>
          <Text ml="sm" size="sm" fw={500}>
            {innerLink.label}
          </Text>
        </Box>
      </Anchor>
    )
  );

  const button = (
    <>
      <UnstyledButton
        onClick={() => setOpened((o) => !o)}
        className={`${classes.control} ${activeLink === link ? classes.activeLink : ''}`}
      >
        <Group justify="space-between" gap={0}>
          <Box style={{ display: 'flex', alignItems: 'center' }}>
            <ThemeIcon variant="transparent" className={classes.icon} size={32}>
              <Icon size={22} />
            </ThemeIcon>
            <Text ml="sm" size="sm" fw={500}>
              {label}
            </Text>
          </Box>
          {hasLinks && (
            <IconChevronRight
              className={classes.chevron}
              stroke={1.5}
              size={20}
              style={{ transform: opened ? 'rotate(-90deg)' : 'none' }}
            />
          )}
        </Group>
      </UnstyledButton>
      {hasLinks ? <Collapse in={opened}>{items}</Collapse> : null}
    </>
  );

  if (!link) {
    return button;
  }

  if (isExternal) {
    return (
      <Anchor
        href={link}
        target="_blank"
        rel="noopener noreferrer"
        key={link}
        className={`${classes.link} ${activeLink === link ? classes.activeLink : ''}`}
      >
        {button}
      </Anchor>
    );
  }

  return (
    <Anchor
      component={Link}
      to={link}
      key={link}
      className={`${classes.link} ${activeLink === link ? classes.activeLink : ''}`}
      onMouseEnter={() => link && preloadRoute(link)}
    >
      {button}
    </Anchor>
  );
}
