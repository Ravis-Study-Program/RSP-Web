import { Anchor, Flex, SimpleGrid, Text, useComputedColorScheme } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import { getTabs } from '@/components/Navbar/NavbarRoutes';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import classes from './SeasonsOverview.module.css';

export function SeasonsOverviewPage() {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { seasonSlug } = useSeasonSlug();
  const { isAdmin, roleName, isLoading } = useUserAndEnrollment(seasonSlug);
  const tabs = getTabs(seasonSlug, isAdmin, roleName);
  const tabItems = tabs.season?.filter((item) => !item.hidden) || [];

  if (isLoading) {
    return null;
  }

  const items = tabItems.map((item) => (
    <Anchor underline="never" key={item.label} className={classes.item} href={item.link}>
      <item.icon color={computedColorScheme === 'light' ? 'darkslategray' : 'white'} size="2rem" />
      <Text size="sm" mt={7} fw={500} c={computedColorScheme === 'light' ? 'dark' : 'white'}>
        {item.label}
      </Text>
    </Anchor>
  ));

  return (
    <Layout>
      <Flex justify="center">
        <SimpleGrid
          className={classes.grid}
          cols={2}
          w={{ base: '100%', sm: '90%', md: '80%', lg: '60%', xl: '50%' }}
          spacing="xl"
          verticalSpacing="xl"
          mt="xl"
        >
          {items}
        </SimpleGrid>
      </Flex>
    </Layout>
  );
}
