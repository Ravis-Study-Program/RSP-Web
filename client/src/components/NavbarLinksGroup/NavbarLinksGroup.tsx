import { Box, Group, rem, Text, ThemeIcon, UnstyledButton } from '@mantine/core';
import classes from './NavbarLinksGroup.module.css';

interface LinksGroupProps {
  icon: React.FC<any>;
  label: string;
  link: string;
}

export function LinksGroup({ icon: Icon, label, link }: LinksGroupProps) {
  return (
    <Text component="a" className={classes.link} href={link} key={link}>
      <UnstyledButton className={classes.control}>
        <Group justify="space-between" gap={0}>
          <Box style={{ display: 'flex', alignItems: 'center' }}>
            <ThemeIcon variant="light" size={32}>
              <Icon style={{ width: rem(22), height: rem(22) }} />
            </ThemeIcon>
            <Box ml="md">{label}</Box>
          </Box>
        </Group>
      </UnstyledButton>
    </Text>
  );
}
