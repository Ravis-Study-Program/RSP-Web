import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import { Button, Fieldset, Flex, NumberInput, Select, Stack, Title } from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  CreateMockInterviewRequest,
  CreateMockInterviewResponseApiResult,
  GetMockInterviewsResponseApiResult,
  LeetcodeProblemDto,
  MockInterviewEntity,
  MockInterviewRoundDto,
  UserEntity,
} from '@/generated/api/client';

const scoreSchema = z
  .number()
  .min(0, { message: 'The minimum score is 0' })
  .max(10, { message: 'The maximum score is 10' });

const schema = z.object({
  startDate: z.date(),
  timeTakenInMinutes: z
    .number()
    .min(1, { message: 'The minimum amount is 1 minute' })
    .max(120, { message: 'The maximum amount is 120 minute.' }),
  interviewer: z.string().min(1),
  behaviouralScore: scoreSchema,
  leetcodeProblem1: z.string().min(1),
  confirmQuestion1: scoreSchema,
  algorithmDesign1: scoreSchema,
  complexityAnalysis1: scoreSchema,
  code1: scoreSchema,
  test1: scoreSchema,
  leetcodeProblem2: z.string().min(1),
  confirmQuestion2: scoreSchema,
  algorithmDesign2: scoreSchema,
  complexityAnalysis2: scoreSchema,
  code2: scoreSchema,
  test2: scoreSchema,
});

export const MockInterviewCreateModal = ({
  table,
  createMockInterview,
  refetchMockInterviews,
  leetcodeProblems,
  enrollmentId,
  users,
}: MockInterviewCreateModalProps) => {
  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      startDate: new Date(),
      timeTakenInMinutes: 0,
      interviewer: '',
      behaviouralScore: 0,
      leetcodeProblem1: '',
      confirmQuestion1: 0,
      algorithmDesign1: 0,
      complexityAnalysis1: 0,
      code1: 0,
      test1: 0,
      leetcodeProblem2: '',
      confirmQuestion2: 0,
      algorithmDesign2: 0,
      complexityAnalysis2: 0,
      code2: 0,
      test2: 0,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    startDate: Date;
    timeTakenInMinutes: number;
    interviewer: string;
    behaviouralScore: number;
    leetcodeProblem1: string;
    confirmQuestion1: number;
    algorithmDesign1: number;
    complexityAnalysis1: number;
    code1: number;
    test1: number;
    leetcodeProblem2: string;
    confirmQuestion2: number;
    algorithmDesign2: number;
    complexityAnalysis2: number;
    code2: number;
    test2: number;
  }) => {
    try {
      const mockInterviewRoundDtos: MockInterviewRoundDto[] = [];
      // Behavioural
      mockInterviewRoundDtos.push({
        behaviouralMockInterviewRound: {
          behavioralScore: values.behaviouralScore,
        },
      });
      // Leetcode Problem 1
      mockInterviewRoundDtos.push({
        leetcodeMockInterviewRound: {
          leetcodeProblemId: values.leetcodeProblem1,
          confirmQuestionScore: values.confirmQuestion1,
          algorithmDesignScore: values.algorithmDesign1,
          complexityAnalysisScore: values.complexityAnalysis1,
          codingScore: values.code1,
          testingScore: values.test1,
        },
      });
      // Leetcode Problem 2
      mockInterviewRoundDtos.push({
        leetcodeMockInterviewRound: {
          leetcodeProblemId: values.leetcodeProblem2,
          confirmQuestionScore: values.confirmQuestion2,
          algorithmDesignScore: values.algorithmDesign2,
          complexityAnalysisScore: values.complexityAnalysis2,
          codingScore: values.code2,
          testingScore: values.test2,
        },
      });

      const requestData: CreateMockInterviewRequest = {
        startDate: values.startDate.toISOString(),
        timeTakenInMinutes: values.timeTakenInMinutes,
        interviewerUserId: values.interviewer,
        mockInterviewRoundDtos,
      };
      if (enrollmentId !== '') {
        requestData.enrollmentId = enrollmentId;
      }

      await createMockInterview({ data: requestData });
      await refetchMockInterviews();
      table.setCreatingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Mock interview created successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as CreateMockInterviewResponseApiResult;
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

  const usersOptions =
    users?.map((user) => ({
      value: user.userId || '',
      label: user.name,
    })) || [];

  const daysBeforeToday = (days: number) => {
    const today = new Date();
    const resultDate = new Date();
    resultDate.setDate(today.getDate() - days);
    return resultDate;
  };

  return (
    <Stack>
      <Title order={3} mt={15}>
        Add Mock Interview
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <DateTimePicker
          {...form.getInputProps('startDate')}
          mt="sm"
          label="Date"
          placeholder="Pick a start date"
          valueFormat="YYYY-MM-DD HH:MM"
          minDate={daysBeforeToday(3)}
          maxDate={new Date()}
          withAsterisk
          highlightToday
          clearable
          error={form.errors.startDate}
        />
        <NumberInput
          {...form.getInputProps('timeTakenInMinutes')}
          mt="sm"
          label="Time Taken In Minutes"
          placeholder="Enter time taken in seconds to complete problem"
          withAsterisk
          error={form.errors.timeTakenInMinutes}
        />
        <Select
          {...form.getInputProps('interviewer')}
          mt="sm"
          label="Interviewer"
          placeholder="Enter interviewer name"
          data={usersOptions}
          limit={5}
          withAsterisk
          searchable
          error={form.errors.interviewer}
        />
        <NumberInput
          {...form.getInputProps('behaviouralScore')}
          mt="sm"
          label="Behavioural Score"
          placeholder="Enter behavioural score"
          withAsterisk
          error={form.errors.behaviouralScore}
        />
        <Fieldset legend="Leetcode Problem 1" mt="sm">
          <Select
            {...form.getInputProps('leetcodeProblem1')}
            mt="sm"
            label="Leetcode Problem"
            placeholder="Pick a leetcode problem"
            data={leetcodeProblemOptions}
            limit={5}
            withAsterisk
            searchable
            error={form.errors.leetcodeProblem1}
          />
          <NumberInput
            {...form.getInputProps('confirmQuestion1')}
            mt="sm"
            label="Confirm Question Score"
            placeholder="Enter confirm question score"
            withAsterisk
            error={form.errors.confirmQuestion1}
          />
          <NumberInput
            {...form.getInputProps('algorithmDesign1')}
            mt="sm"
            label="Algorithm Design Score"
            placeholder="Enter algorithm design score"
            withAsterisk
            error={form.errors.algorithmDesign1}
          />
          <NumberInput
            {...form.getInputProps('complexityAnalysis1')}
            mt="sm"
            label="Complexity Analysis Score"
            placeholder="Enter complexity analysis score"
            withAsterisk
            error={form.errors.complexityAnalysis1}
          />
          <NumberInput
            {...form.getInputProps('code1')}
            mt="sm"
            label="Coding Score"
            placeholder="Enter coding score"
            withAsterisk
            error={form.errors.code1}
          />
          <NumberInput
            {...form.getInputProps('test1')}
            mt="sm"
            label="Testing Score"
            placeholder="Enter testing score"
            withAsterisk
            error={form.errors.test1}
          />
        </Fieldset>

        <Fieldset legend="Leetcode Problem 2" mt="sm">
          <Select
            {...form.getInputProps('leetcodeProblem2')}
            mt="sm"
            label="Leetcode Problem"
            placeholder="Pick a leetcode problem"
            data={leetcodeProblemOptions}
            limit={5}
            withAsterisk
            searchable
            error={form.errors.leetcodeProblem2}
          />
          <NumberInput
            {...form.getInputProps('confirmQuestion2')}
            mt="sm"
            label="Confirm Question Score"
            placeholder="Enter confirm question score"
            withAsterisk
            error={form.errors.confirmQuestion2}
          />
          <NumberInput
            {...form.getInputProps('algorithmDesign2')}
            mt="sm"
            label="Algorithm Design Score"
            placeholder="Enter algorithm design score"
            withAsterisk
            error={form.errors.algorithmDesign2}
          />
          <NumberInput
            {...form.getInputProps('complexityAnalysis2')}
            mt="sm"
            label="Complexity Analysis Score"
            placeholder="Enter complexity analysis score"
            withAsterisk
            error={form.errors.complexityAnalysis2}
          />
          <NumberInput
            {...form.getInputProps('code2')}
            mt="sm"
            label="Coding Score"
            placeholder="Enter coding score"
            withAsterisk
            error={form.errors.code2}
          />
          <NumberInput
            {...form.getInputProps('test2')}
            mt="sm"
            label="Testing Score"
            placeholder="Enter testing score"
            withAsterisk
            error={form.errors.test2}
          />
        </Fieldset>

        <Flex justify="flex-end">
          <Button type="submit" mt="xl" mb="md">
            Submit
          </Button>
        </Flex>
      </form>
    </Stack>
  );
};

type MockInterviewCreateModalProps = {
  table: MRT_TableInstance<MockInterviewEntity>;
  createMockInterview: UseMutateAsyncFunction<
    CreateMockInterviewResponseApiResult,
    CreateMockInterviewResponseApiResult,
    {
      data: CreateMockInterviewRequest;
    },
    unknown
  >;
  refetchMockInterviews: (
    options?: RefetchOptions
  ) => Promise<
    QueryObserverResult<GetMockInterviewsResponseApiResult, GetMockInterviewsResponseApiResult>
  >;
  leetcodeProblems: LeetcodeProblemDto[] | null | undefined;
  users: UserEntity[] | null | undefined;
  enrollmentId: string;
};
