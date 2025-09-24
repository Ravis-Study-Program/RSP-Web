import dayjs from 'dayjs';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, NumberInput, Select, Stack, Textarea, Title } from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  LeetcodeProblemDto,
  ListProblemAttemptResponseApiResponse,
  ProblemAttemptEntity,
  UpdateProblemAttemptRequest,
  UpdateProblemAttemptResponseApiResponse,
  useGetCurrentUser,
} from '@/generated/api/client';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const schema = z.object({
  leetcodeProblemId: z.string().min(1, {
    message: 'Leetcode must not be empty',
  }),
  attemptStartDateUtc: z.string().min(1),
  timeTakenInMinutes: z
    .number()
    .min(1, {
      message: 'The minimum amount is 1 minute.',
    })
    .max(120, {
      message: 'The maximum amount is 120 minute.',
    }),
  notes: z.string(),
});

export const LeetcodeProblemAttemptUpdateModal = ({
  table,
  row: { original: problemAttempt },
  updateProblemAttempt,
  refetchProblemAttempts,
  leetcodeProblems,
  enrollmentId,
}: LeetcodeProblemAttemptUpdateModalProps) => {
  const { data: userResponse } = useGetCurrentUser();
  const userId = userResponse?.responseBody?.user.userId ?? '';

  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      leetcodeProblemId: problemAttempt.leetcodeProblemId,
      attemptStartDateUtc: dayjs(problemAttempt.attemptStartDateUtc).format('YYYY-MM-DD HH:mm'),
      timeTakenInMinutes: problemAttempt.timeTakenInMinutes,
      notes: problemAttempt.notes,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    leetcodeProblemId: string | null | undefined;
    attemptStartDateUtc: string;
    timeTakenInMinutes: number;
    notes: string;
  }) => {
    try {
      const requestData: UpdateProblemAttemptRequest = {
        ...values,
        userId,
        attemptStartDateUtc: dayjs(values.attemptStartDateUtc).toISOString(),
        problemAttemptId: problemAttempt.problemAttemptId,
      };
      if (enrollmentId !== '') {
        requestData.enrollmentId = enrollmentId;
      }

      await updateProblemAttempt({ data: requestData });
      await refetchProblemAttempts();
      table.setEditingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Problem attempt updated successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as UpdateProblemAttemptResponseApiResponse;
      notifications.show({
        color: 'red',
        title: 'Error',
        autoClose: false,
        message: response.error?.message,
      });
    }
  };

  const leetcodeProblemOptions =
    leetcodeProblems?.map((leetcodeProblem) => ({
      value: leetcodeProblem.leetcodeProblemId,
      label: leetcodeProblem.title,
    })) || [];

  const daysBeforeToday = (days: number) => {
    const today = new Date();
    const resultDate = new Date();
    resultDate.setDate(today.getDate() - days);
    return dayjs(resultDate).format('YYYY-MM-DD HH:mm');
  };

  return (
    <Stack>
      <Title order={3} mt={15}>
        Update Problem Attempt
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Select
          {...form.getInputProps('leetcodeProblemId')}
          label="Select Leetcode Problem"
          placeholder="Pick a leetcode problem"
          data={leetcodeProblemOptions}
          filter={createOptionsFilter({ sort: false })}
          limit={5}
          withAsterisk
          searchable
          error={form.errors.leetcodeProblemId}
        />
        <DateTimePicker
          {...form.getInputProps('attemptStartDateUtc')}
          mt="sm"
          label="Attempt Start Date"
          placeholder="Pick a start date"
          valueFormat="YYYY-MM-DD HH:mm"
          minDate={daysBeforeToday(3)}
          maxDate={new Date()}
          withAsterisk
          highlightToday
          clearable
          error={form.errors.attemptStartDateUtc}
          timePickerProps={{
            withDropdown: true,
            popoverProps: { withinPortal: false },
          }}
        />
        <NumberInput
          {...form.getInputProps('timeTakenInMinutes')}
          mt="sm"
          label="Time Taken In Minutes"
          placeholder="Enter time taken in seconds to complete problem"
          withAsterisk
          error={form.errors.timeTakenInMinutes}
        />
        <Textarea
          {...form.getInputProps('notes')}
          mt="sm"
          label="Notes"
          autosize
          minRows={2}
          maxRows={6}
          placeholder="Enter notes"
          error={form.errors.notes}
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

type LeetcodeProblemAttemptUpdateModalProps = {
  table: MRT_TableInstance<ProblemAttemptEntity>;
  updateProblemAttempt: UseMutateAsyncFunction<
    UpdateProblemAttemptResponseApiResponse,
    unknown,
    {
      data: UpdateProblemAttemptRequest;
    },
    unknown
  >;
  row: MRT_Row<ProblemAttemptEntity>;
  refetchProblemAttempts: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<ListProblemAttemptResponseApiResponse, unknown>>;
  leetcodeProblems: LeetcodeProblemDto[] | null | undefined;
  enrollmentId: string;
};
