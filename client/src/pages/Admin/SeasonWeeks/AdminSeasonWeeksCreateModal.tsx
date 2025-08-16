import dayjs from 'dayjs';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, NumberInput, Select, Stack, Title } from '@mantine/core';
import { DatePickerInput } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  AdminCreateSeasonWeekRequest,
  AdminCreateSeasonWeekResponseApiResponse,
  AdminListSeasonWeekResponseApiResponse,
  SeasonEntity,
  SeasonWeekEntity,
} from '@/generated/api/client';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const schema = z
  .object({
    seasonId: z.string(),
    weekNumber: z.number().min(1),
    startDate: z.string().min(1),
    endDate: z.string().min(1),
  })
  .refine(
    (data) => {
      const startDate = dayjs(data.startDate).toDate();
      const endDate = dayjs(data.endDate).toDate();
      return endDate > startDate;
    },
    {
      message: 'End date must be at least one day greater than start date',
      path: ['endDate'],
    }
  );

export const AdminSeasonWeeksCreateModal = ({
  table,
  createSeasonWeek,
  refetchSeasonWeeks,
  seasons,
}: AdminSeasonWeeksCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      seasonId: '',
      weekNumber: 1,
      startDate: dayjs().format('YYYY-MM-DD'),
      endDate: dayjs().format('YYYY-MM-DD'),
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    seasonId: string;
    weekNumber: number;
    startDate: string;
    endDate: string;
  }) => {
    try {
      const requestData: AdminCreateSeasonWeekRequest = {
        ...values,
        startDate: dayjs(values.startDate).toISOString(),
        endDate: dayjs(values.endDate).toISOString(),
      };

      await createSeasonWeek({ data: requestData });
      await refetchSeasonWeeks();
      table.setCreatingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Season Week created successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as AdminCreateSeasonWeekResponseApiResponse;
      notifications.show({
        color: 'red',
        title: 'Error',
        autoClose: false,
        message: response.error?.message,
      });
    }
  };

  const seasonOptions =
    seasons?.map((season) => ({
      value: season.seasonId,
      label: season.name,
    })) || [];

  return (
    <Stack>
      <Title order={3} mt={15}>
        Create Season Week
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Select
          {...form.getInputProps('seasonId')}
          label="Select Season"
          placeholder="Pick a season"
          data={seasonOptions}
          filter={createOptionsFilter()}
          limit={5}
          withAsterisk
          searchable
          error={form.errors.seasonId}
        />
        <NumberInput
          {...form.getInputProps('weekNumber')}
          mt="sm"
          label="Week Number"
          placeholder="Enter week number"
          withAsterisk
          error={form.errors.weekNumber}
        />
        <DatePickerInput
          {...form.getInputProps('startDate')}
          mt="sm"
          label="Start Date"
          placeholder="Pick a start date"
          valueFormat="YYYY-MM-DD"
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
          error={form.errors.endDate}
          withAsterisk
          highlightToday
          clearable
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

type AdminSeasonWeeksCreateModalProps = {
  table: MRT_TableInstance<SeasonWeekEntity>;
  seasons: SeasonEntity[];
  createSeasonWeek: UseMutateAsyncFunction<
    AdminCreateSeasonWeekResponseApiResponse,
    unknown,
    {
      data: AdminCreateSeasonWeekRequest;
    },
    unknown
  >;
  refetchSeasonWeeks: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<AdminListSeasonWeekResponseApiResponse, unknown>>;
};
