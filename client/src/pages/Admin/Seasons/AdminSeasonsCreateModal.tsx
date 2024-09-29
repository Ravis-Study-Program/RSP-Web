import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, Select, Stack, TextInput, Title } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import {
  AdminCreateSeasonRequest,
  AdminCreateSeasonResponseApiResult,
  AdminListSeasonResponseApiResult,
  Season,
} from '@/generated/api/client';

const locationImages: { [key: string]: string } = {
  'Adelaide, Australia':
    'https://images.pexels.com/photos/7146542/pexels-photo-7146542.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1',
  'Sydney, Australia':
    'https://images.pexels.com/photos/1086852/pexels-photo-1086852.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1',
};

const schema = z
  .object({
    name: z.string().min(1),
    startDate: z.date(),
    endDate: z.date(),
    location: z.string().min(1),
    imageUrl: z.string().min(1),
  })
  .refine((data) => data.endDate > data.startDate, {
    message: 'End date must be at least one day greater than start date',
    path: ['endDate'],
  });

export const AdminSeasonsCreateModal = ({
  table,
  createSeason,
  refetchSeasons,
}: AdminSeasonsCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      name: '',
      startDate: new Date(),
      endDate: new Date(),
      location: '',
      imageUrl: '',
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    name: string;
    startDate: Date;
    endDate: Date;
    location: string;
    imageUrl: string;
  }) => {
    try {
      const requestData: AdminCreateSeasonRequest = {
        ...values,
        startDate: values.startDate.toISOString(),
        endDate: values.endDate.toISOString(),
      };

      await createSeason({ data: requestData });
      await refetchSeasons();
      table.setCreatingRow(null);
    } catch (err: unknown) {
      // eslint-disable-next-line no-console
      console.log(err);
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
      <Title order={3}>Create Season</Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <TextInput
          {...form.getInputProps('name')}
          label="Name"
          placeholder="Enter season name"
          withAsterisk
        />
        <DatePickerInput
          {...form.getInputProps('startDate')}
          mt="sm"
          label="Start Date"
          placeholder="Pick a start date"
          valueFormat="YYYY-MM-DD"
          minDate={new Date()}
          error={form.errors.startDate}
          withAsterisk
          highlightToday
          clearable
        />
        <DatePickerInput
          {...form.getInputProps('endDate')}
          mt="sm"
          label="End Date"
          placeholder="Pick an end date"
          valueFormat="YYYY-MM-DD"
          minDate={new Date()}
          error={form.errors.endDate}
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
          onChange={handleLocationChange}
        />
        <TextInput
          {...form.getInputProps('imageUrl')}
          mt="sm"
          label="Image URL"
          description="This field will be automatically populated by location"
          placeholder="Enter Image"
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

type AdminSeasonsCreateModalProps = {
  table: MRT_TableInstance<Season>;
  createSeason: UseMutateAsyncFunction<
    AdminCreateSeasonResponseApiResult,
    AdminCreateSeasonResponseApiResult,
    {
      data: AdminCreateSeasonRequest;
    },
    unknown
  >;
  refetchSeasons: (
    options?: RefetchOptions
  ) => Promise<
    QueryObserverResult<AdminListSeasonResponseApiResult, AdminListSeasonResponseApiResult>
  >;
};
