import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, TextInput, Title } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  AdminListSeasonResponseApiResponse,
  AdminUpdateSeasonRequest,
  AdminUpdateSeasonResponseApiResponse,
  SeasonEntity,
} from '@/generated/api/client';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const locationImages: { [key: string]: string } = {
  'Adelaide, Australia':
    'https://images.pexels.com/photos/7146542/pexels-photo-7146542.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1',
  'Sydney, Australia':
    'https://images.pexels.com/photos/1086852/pexels-photo-1086852.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1',
};

const schema = z
  .object({
    name: z.string().min(1),
    slug: z
      .string()
      .min(1)
      .regex(/^[A-Za-z]{3}-\d{4}-\d{4}$/, {
        message:
          'Invalid format, expected XXX-YYYY-YYYY where X is letters and Y is numbers. Eg ADL-2023-2024.',
      }),
    startDateInclusiveUtc: z.date(),
    endDateInclusiveUtc: z.date(),
    location: z.string().min(1),
    imageUrl: z.string().min(1),
  })
  .refine((data) => data.endDateInclusiveUtc > data.startDateInclusiveUtc, {
    message: 'End date must be at least one day greater than start date',
    path: ['endDateInclusiveUtc'],
  });

export const AdminSeasonsUpdateModal = ({
  table,
  row: { original: season },
  updateSeason,
  refetchSeasons,
}: AdminSeasonsUpdateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: season.name,
      slug: season.slug,
      startDateInclusiveUtc: new Date(season.startDateInclusiveUtc),
      endDateInclusiveUtc: new Date(season.endDateInclusiveUtc),
      location: season.location,
      imageUrl: season.imageUrl,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    name: string;
    slug: string;
    startDateInclusiveUtc: Date;
    endDateInclusiveUtc: Date;
    location: string;
    imageUrl: string;
  }) => {
    try {
      const requestData: AdminUpdateSeasonRequest = {
        ...values,
        startDateInclusiveUtc: values.startDateInclusiveUtc.toISOString(),
        endDateInclusiveUtc: values.endDateInclusiveUtc.toISOString(),
        seasonId: season.seasonId,
      };

      await updateSeason({ data: requestData });
      await refetchSeasons();
      table.setEditingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Season updated successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as AdminUpdateSeasonResponseApiResponse;
      notifications.show({
        color: 'red',
        title: 'Error',
        autoClose: false,
        message: response.error?.message,
      });
    }
  };

  const handleLocationChange = (location: string | null) => {
    if (location == null) {
      return;
    }

    form.setFieldValue('location', location);
    const imageUrl = locationImages[location] || '';
    form.setFieldValue('imageUrl', imageUrl);
  };

  return (
    <Stack>
      <Title order={3} mt={15}>
        Update Season
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput
          {...form.getInputProps('name')}
          label="Name"
          placeholder="Enter season name"
          withAsterisk
        />
        <TextInput
          {...form.getInputProps('slug')}
          label="Slug"
          placeholder="Enter season slug"
          withAsterisk
        />
        <DatePickerInput
          {...form.getInputProps('startDateInclusiveUtc')}
          mt="sm"
          label="Start Date"
          placeholder="Pick a start date"
          valueFormat="YYYY-MM-DD"
          minDate={new Date()}
          error={form.errors.startDateInclusiveUtc}
          withAsterisk
          highlightToday
          clearable
        />
        <DatePickerInput
          {...form.getInputProps('endDateInclusiveUtc')}
          mt="sm"
          label="End Date"
          placeholder="Pick an end date"
          valueFormat="YYYY-MM-DD"
          minDate={new Date()}
          error={form.errors.endDateInclusiveUtc}
          withAsterisk
          highlightToday
          clearable
        />
        <Select
          {...form.getInputProps('location')}
          mt="sm"
          label="Location"
          placeholder="Pick location"
          withAsterisk
          data={Object.keys(locationImages)}
          filter={createOptionsFilter()}
          onChange={handleLocationChange}
        />
        <TextInput
          {...form.getInputProps('imageUrl')}
          mt="sm"
          label="Image URL"
          description="This field will be automatically populated by location"
          placeholder="Enter image"
          withAsterisk
          disabled
          hidden
        />
        <Flex justify="flex-end">
          <Button type="submit" mt="xl" mb="md">
            Submit
          </Button>
        </Flex>
      </form>
    </Stack>
  );
};

type AdminSeasonsUpdateModalProps = {
  table: MRT_TableInstance<SeasonEntity>;
  updateSeason: UseMutateAsyncFunction<
    AdminUpdateSeasonResponseApiResponse,
    unknown,
    {
      data: AdminUpdateSeasonRequest;
    },
    unknown
  >;
  row: MRT_Row<SeasonEntity>;
  refetchSeasons: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListSeasonResponseApiResponse, unknown>>;
};
