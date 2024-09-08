import { ReactNode } from 'react';
import { Anchor, AppShell, Breadcrumbs, Burger, Group, Title } from '@mantine/core';
import { useDisclosure } from '@mantine/hooks';
import { Navbar } from '../Navbar/Navbar';
import classes from './Layout.module.css';

interface LayoutProps {
  children: ReactNode;
}

export function Layout({ children }: LayoutProps) {
  const [opened, { toggle }] = useDisclosure();

  const items = [
    { title: 'Seasons', href: '#' },
    { title: 'ADL-2023', href: '#' },
    { title: 'Overview', href: '#' },
  ].map((item, index) => (
    <Anchor href={item.href} key={index} className={classes.breadcrumb_links}>
      {item.title}
    </Anchor>
  ));

  return (
    <AppShell
      layout="alt"
      header={{ height: 65 }}
      navbar={{ width: 300, breakpoint: 'sm', collapsed: { mobile: !opened } }}
      padding="xl"
    >
      <AppShell.Header>
        <Group h="100%" px="md">
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          <Breadcrumbs>{items}</Breadcrumbs>
        </Group>
      </AppShell.Header>
      <AppShell.Navbar p="md" className={classes.navbar}>
        <Group className={classes.mobile_nav_header}>
          <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
          <Title className={classes.navbar_header_text} order={1} size="h3">
            Ravi Study Program
          </Title>
        </Group>
        <Navbar />
      </AppShell.Navbar>
      <AppShell.Main className={classes.main}>{children}</AppShell.Main>
    </AppShell>
  );
}
