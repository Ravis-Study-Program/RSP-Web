import dayjs from 'dayjs';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, TextInput, Title } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  AdminCreateSeasonRequest,
  AdminCreateSeasonResponseApiResponse,
  AdminListSeasonResponseApiResponse,
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
    startDateInclusiveUtc: z.string().min(1),
    endDateInclusiveUtc: z.string().min(1),
    location: z.string().min(1),
    imageUrl: z.string().min(1),
  })
  .refine(
    (data) => {
      const startDate = dayjs(data.startDateInclusiveUtc).toDate();
      const endDate = dayjs(data.endDateInclusiveUtc).toDate();
      return endDate > startDate;
    },
    {
      message: 'End date must be at least one day greater than start date',
      path: ['endDateInclusiveUtc'],
    }
  );

export const AdminSeasonsCreateModal = ({
  table,
  createSeason,
  refetchSeasons,
}: AdminSeasonsCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: '',
      slug: '',
      startDateInclusiveUtc: dayjs().format('YYYY-MM-DD HH:mm'),
      endDateInclusiveUtc: dayjs().format('YYYY-MM-DD HH:mm'),
      location: '',
      imageUrl: '',
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    name: string;
    slug: string;
    startDateInclusiveUtc: string;
    endDateInclusiveUtc: string;
    location: string;
    imageUrl: string;
  }) => {
    try {
      const requestData: AdminCreateSeasonRequest = {
        ...values,
        startDateInclusiveUtc: dayjs(values.startDateInclusiveUtc).toISOString(),
        endDateInclusiveUtc: dayjs(values.endDateInclusiveUtc).toISOString(),
      };

      await createSeason({ data: requestData });
      await refetchSeasons();
      table.setCreatingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Season created successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as AdminCreateSeasonResponseApiResponse;
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
        Create Season
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
          mt="sm"
          label="Slug"
          placeholder="Enter season slug"
          withAsterisk
        />
        <DatePickerInput
          {...form.getInputProps('startDateInclusiveUtc')}
          mt="sm"
          label="Start Date"
          placeholder="Pick a start date"
          valueFormat="YYYY-MM-DD HH:mm"
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
          valueFormat="YYYY-MM-DD HH:mm"
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
        <Flex justify="flex-end">
          <Button type="submit" mt="xl" mb="md">
            Submit
          </Button>
        </Flex>
      </form>
    </Stack>
  );
};

type AdminSeasonsCreateModalProps = {
  table: MRT_TableInstance<SeasonEntity>;
  createSeason: UseMutateAsyncFunction<
    AdminCreateSeasonResponseApiResponse,
    unknown,
    {
      data: AdminCreateSeasonRequest;
    },
    unknown
  >;
  refetchSeasons: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListSeasonResponseApiResponse, unknown>>;
};
