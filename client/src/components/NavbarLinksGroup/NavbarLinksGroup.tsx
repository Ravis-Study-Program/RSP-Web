import { IconChevronRight } from '@tabler/icons-react';
import { Link } from 'react-router-dom';
import { Anchor, Box, Collapse, Group, Text, ThemeIcon, UnstyledButton } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { TabItem } from '../Navbar/NavbarRoutes';
import classes from './NavbarLinksGroup.module.css';

interface LinksGroupProps {
  icon: React.FC<any>;
  label: string;
  initiallyOpened: boolean;
  links?: TabItem[];
  link?: string;
  activeLink?: string;
}

export function LinksGroup({
  label,
  initiallyOpened = false,
  links,
  link,
  icon: Icon,
  activeLink,
}: LinksGroupProps) {
  const hasLinks = Array.isArray(links);

  const [opened, setOpened] = useLocalStorage({
    key: `navbar-group-${label}-opened`,
    defaultValue: initiallyOpened,
  });

  const items = (hasLinks ? links.filter((item) => !item.hidden) : []).map((innerLink) => (
    <Anchor
      component={Link}
      className={`${classes.innerLink} ${activeLink === innerLink.link ? classes.activeLink : ''}`}
      to={innerLink.link || ''}
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
  ));

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

  return (
    <Anchor
      component={Link}
      to={link}
      key={link}
      className={`${classes.link} ${activeLink === link ? classes.activeLink : ''}`}
    >
      {button}
    </Anchor>
  );
}
