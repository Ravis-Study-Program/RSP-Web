import { Card, Flex, SimpleGrid, Text, useComputedColorScheme } from '@mantine/core';
import { getTabs } from '@/components/Navbar/NavbarRoutes';
import { useGetUserEnrollments } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { useUserAndEnrollment } from '@/shared/hooks/useUserAndEnrollment';
import classes from './SeasonsOverview.module.css';

export default function SeasonsOverviewPage() {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });
  const { seasonSlug } = useSeasonSlug();
  const { isAdmin, role, isLoading } = useUserAndEnrollment(seasonSlug);
  const { data: enrollmentsResponse } = useGetUserEnrollments();

  const resourcesUrl = enrollmentsResponse?.responseBody?.enrollments?.find(
    (enrollment) => enrollment.seasonSlug === seasonSlug
  )?.seasonResourcesUrl;

  const tabs = getTabs(seasonSlug, isAdmin, role, resourcesUrl);
  const tabItems = tabs.season?.[0]?.links?.filter((item) => !item.hidden) ?? [];

  if (isLoading) {
    return null;
  }

  const items = tabItems.map((item) => (
    <Card
      withBorder
      shadow="sm"
      key={item.label}
      className={classes.item}
      component="a"
      href={item.link}
      target={item.isExternal ? '_blank' : undefined}
      rel={item.isExternal ? 'noopener noreferrer' : undefined}
    >
      <item.icon color={computedColorScheme === 'light' ? 'darkslategray' : 'white'} size="2rem" />
      <Text size="sm" mt={7} fw={500} c={computedColorScheme === 'light' ? 'dark' : 'white'}>
        {item.label}
      </Text>
    </Card>
  ));

  return (
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
  );
}
