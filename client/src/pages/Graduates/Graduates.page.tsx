import { useMemo, useState } from 'react';
import { Avatar, Button, Card, Flex, Grid, MultiSelect, Skeleton, Text } from '@mantine/core';
import { Layout } from '@/components/Layout/Layout';
import { GraduateDto, useGetGraduates } from '@/generated/api/client';
import { createOptionsFilter, getGraduates } from '@/shared/table/globalFilters';
import classes from './Graduates.module.css';

export default function GraduatesPage() {
  const {
    data: graduatesResponse,
    isError: isLoadingGraduatesError,
    isFetching: isFetchingGraduates,
    isLoading: isLoadingGraduates,
  } = useGetGraduates();
  const [selectedGraduates, setSelectedGraduates] = useState<string[]>([]);
  const graduatesOptions = getGraduates(graduatesResponse?.responseBody?.graduates || []);

  const filteredGraduates = useMemo(() => {
    const graduates = graduatesResponse?.responseBody?.graduates || [];

    if (selectedGraduates.length === 0) {
      return graduates;
    }

    return graduates.filter(
      (graduate) => graduate.name != null && selectedGraduates.includes(graduate.name)
    );
  }, [graduatesResponse, selectedGraduates]);

  return (
    <Layout>
      <Flex mb="xl" pb="xl" justify="space-between" align="flex-end">
        <MultiSelect
          classNames={{ inputField: classes.inputField }}
          label="Name"
          placeholder="Pick value(s)"
          data={graduatesOptions}
          filter={createOptionsFilter()}
          miw={150}
          searchable
          nothingFoundMessage="Nothing found..."
          value={selectedGraduates}
          onChange={(values) => {
            setSelectedGraduates(values as string[]);
          }}
        />
        <Text size="sm">{filteredGraduates.length} found</Text>
      </Flex>

      <Grid mt="lg" gutter={{ base: 'md', xs: 'md', md: 'xl', xl: 50 }}>
        {!isLoadingGraduates && !isFetchingGraduates && !isLoadingGraduatesError ? (
          <GraduateCards graduates={filteredGraduates} />
        ) : (
          <GraduateSkeletonCards />
        )}
      </Grid>
    </Layout>
  );
}

const GraduateSkeletonCards = () => {
  const numCards = 8;

  return (
    <>
      {Array.from({ length: numCards }).map((_, index) => (
        <Grid.Col key={index} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
          <Card withBorder shadow="xs" radius="md">
            <Skeleton height={80} mb="xl" />
            <Skeleton height={10} radius="xl" />
            <Skeleton height={8} mt={8} radius="xl" />
            <Skeleton height={8} mt={8} radius="xl" />
            <Skeleton height={8} mt={8} width="70%" radius="xl" />
            <Skeleton height={40} mt={50} radius="xl" />
          </Card>
        </Grid.Col>
      ))}
    </>
  );
};

type GraduateCardsProps = {
  graduates: GraduateDto[] | undefined;
};

export function GraduateCards({ graduates }: GraduateCardsProps) {
  return (
    <>
      {graduates?.map((graduate, key) => (
        <Grid.Col key={key} span={{ base: 12, sm: 6, md: 6, lg: 2 }}>
          <Card key={key} withBorder shadow="sm" radius="md" py="xl" className={classes.card}>
            <Avatar
              color="initials"
              name={graduate?.name || undefined}
              size={60}
              radius={60}
              mx="auto"
            />
            <Text ta="center" fz="lg" fw={600} size="sm" mt="md">
              {graduate.name}
            </Text>

            <Button
              component="a"
              href={`/profile?email=${graduate.email}`}
              radius="md"
              mt="sm"
              size="xs"
              variant="primary"
            >
              Profile
            </Button>
          </Card>
        </Grid.Col>
      ))}
    </>
  );
}
