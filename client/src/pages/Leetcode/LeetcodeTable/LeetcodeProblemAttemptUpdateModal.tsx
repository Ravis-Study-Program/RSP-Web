import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Flex, NumberInput, Select, Stack, Textarea, Title } from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { useForm } from '@mantine/form';
import {
  GetProblemAttemptsResponseApiResult,
  LeetcodeProblem,
  ProblemAttempt,
  UpdateProblemAttemptRequest,
  UpdateProblemAttemptResponseApiResult,
} from '@/generated/api/client';

const schema = z.object({
  leetcodeProblemId: z.string().uuid().min(1),
  attemptStartDate: z.date(),
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
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      leetcodeProblemId: problemAttempt.leetcodeProblemId || '',
      attemptStartDate: new Date(problemAttempt.attemptStartDate),
      timeTakenInMinutes: problemAttempt.timeTakenInMinutes,
      notes: problemAttempt.notes,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    leetcodeProblemId: string;
    attemptStartDate: Date;
    timeTakenInMinutes: number;
    notes: string;
  }) => {
    try {
      const requestData: UpdateProblemAttemptRequest = {
        ...values,
        attemptStartDate: values.attemptStartDate.toISOString(),
        problemAttemptId: problemAttempt.problemAttemptId,
      };
      if (enrollmentId !== '') {
        requestData.enrollmentId = enrollmentId;
      }

      await updateProblemAttempt({ data: requestData });
      await refetchProblemAttempts();
      table.setEditingRow(null);
    } catch (err: unknown) {
      // eslint-disable-next-line no-console
      console.log(err);
    }
  };

  const leetcodeProblemOptions =
    leetcodeProblems?.map((leetcodeProblem) => ({
      value: leetcodeProblem.leetcodeProblemId || '',
      label: leetcodeProblem.problem.title,
    })) || [];

  const daysBeforeToday = (days: number) => {
    const today = new Date();
    const resultDate = new Date();
    resultDate.setDate(today.getDate() - days);
    return resultDate;
  };

  return (
    <Stack>
      <Title order={3}>Update Problem Attempt</Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Select
          {...form.getInputProps('leetcodeProblemId')}
          label="Select Leetcode Problem"
          placeholder="Pick a leetcode problem"
          data={leetcodeProblemOptions}
          limit={5}
          withAsterisk
          searchable
          error={form.errors.leetcodeProblemId}
        />
        <DateTimePicker
          {...form.getInputProps('attemptStartDate')}
          mt="sm"
          label="Attempt Start Date"
          placeholder="Pick a start date"
          valueFormat="YYYY-MM-DD"
          minDate={daysBeforeToday(3)}
          maxDate={new Date()}
          withAsterisk
          highlightToday
          clearable
          error={form.errors.attemptStartDate}
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
          label="Notes (Optional)"
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
  table: MRT_TableInstance<ProblemAttempt>;
  updateProblemAttempt: UseMutateAsyncFunction<
    UpdateProblemAttemptResponseApiResult,
    UpdateProblemAttemptResponseApiResult,
    {
      data: UpdateProblemAttemptRequest;
    },
    unknown
  >;
  row: MRT_Row<ProblemAttempt>;
  refetchProblemAttempts: (
    options?: RefetchOptions
  ) => Promise<
    QueryObserverResult<GetProblemAttemptsResponseApiResult, GetProblemAttemptsResponseApiResult>
  >;
  leetcodeProblems: LeetcodeProblem[] | null | undefined;
  enrollmentId: string;
};
