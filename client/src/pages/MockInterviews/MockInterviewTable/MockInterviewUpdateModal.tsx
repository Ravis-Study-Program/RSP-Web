import dayjs from 'dayjs';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import {
  Button,
  Fieldset,
  Flex,
  NumberInput,
  Select,
  Slider,
  Stack,
  Text,
  Title,
} from '@mantine/core';
import { DateTimePicker } from '@mantine/dates';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  EnrollmentUserDto,
  LeetcodeProblemDto,
  ListMockInterviewResponseApiResponse,
  MockInterviewEntity,
  MockInterviewRoundDto,
  UpdateMockInterviewRequest,
  UpdateMockInterviewResponseApiResponse,
  useGetCurrentUser,
} from '@/generated/api/client';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const SCORE_SLIDER_MARKS = Array.from({ length: 10 }, (_, i) => ({
  value: i + 1,
  label: String(i + 1),
}));

const scoreSchema = z
  .number()
  .min(0, { message: 'The minimum score is 0' })
  .max(10, { message: 'The maximum score is 10' });

const schema = z.object({
  startDate: z.string().min(1),
  timeTakenInMinutes: z
    .number()
    .min(1, { message: 'The minimum amount is 1 minute' })
    .max(120, { message: 'The maximum amount is 120 minute.' }),
  interviewee: z.string().min(1),
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

export const MockInterviewUpdateModal = ({
  table,
  row: { original: mockInterview },
  updateMockInterview,
  refetchMockInterviews,
  leetcodeProblems,
  seasonId,
  users,
}: MockInterviewUpdateModalProps) => {
  const { data: currentUserResponse } = useGetCurrentUser();
  const userId = currentUserResponse?.responseBody?.user.userId ?? '';

  const behavioural =
    mockInterview.mockInterviewRounds?.filter((m) => m.behaviouralMockInterviewRound != null) || [];
  const behaviouralRound = behavioural[0].behaviouralMockInterviewRound;
  const leetcodeRounds =
    mockInterview.mockInterviewRounds?.filter((m) => m.leetcodeMockInterviewRound != null) || [];
  const leetcodeRound1 = leetcodeRounds[0].leetcodeMockInterviewRound;
  const leetcodeRound2 = leetcodeRounds[1].leetcodeMockInterviewRound;

  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      mockInterviewId: mockInterview.mockInterviewId,
      startDate: dayjs(mockInterview.startDate).format('YYYY-MM-DD HH:mm'),
      timeTakenInMinutes: mockInterview.timeTakenInMinutes,
      interviewee: mockInterview.intervieweeUserId ?? '',
      behaviouralScore: behaviouralRound?.behavioralScore ?? 0,
      leetcodeProblem1: leetcodeRound1?.leetcodeProblemId ?? '',
      confirmQuestion1: leetcodeRound1?.confirmQuestionScore ?? 0,
      algorithmDesign1: leetcodeRound1?.algorithmDesignScore ?? 0,
      complexityAnalysis1: leetcodeRound1?.complexityAnalysisScore ?? 0,
      code1: leetcodeRound1?.codingScore ?? 0,
      test1: leetcodeRound1?.testingScore ?? 0,
      leetcodeProblem2: leetcodeRound2?.leetcodeProblemId ?? '',
      confirmQuestion2: leetcodeRound2?.confirmQuestionScore ?? 0,
      algorithmDesign2: leetcodeRound2?.algorithmDesignScore ?? 0,
      complexityAnalysis2: leetcodeRound2?.complexityAnalysisScore ?? 0,
      code2: leetcodeRound2?.codingScore ?? 0,
      test2: leetcodeRound2?.testingScore ?? 0,
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
    mockInterviewId: string;
    startDate: string;
    timeTakenInMinutes: number;
    interviewee: string;
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
      const mockInterviewRounds: MockInterviewRoundDto[] = [];
      mockInterviewRounds.push({
        mockInterviewRoundId: behavioural[0]?.mockInterviewRoundId,
        behaviouralMockInterviewRound: {
          behavioralScore: values.behaviouralScore,
        },
      });
      mockInterviewRounds.push({
        mockInterviewRoundId: leetcodeRounds[0]?.mockInterviewRoundId,
        leetcodeMockInterviewRound: {
          leetcodeProblemId: values.leetcodeProblem1,
          confirmQuestionScore: values.confirmQuestion1,
          algorithmDesignScore: values.algorithmDesign1,
          complexityAnalysisScore: values.complexityAnalysis1,
          codingScore: values.code1,
          testingScore: values.test1,
        },
      });
      mockInterviewRounds.push({
        mockInterviewRoundId: leetcodeRounds[1]?.mockInterviewRoundId,
        leetcodeMockInterviewRound: {
          leetcodeProblemId: values.leetcodeProblem2,
          confirmQuestionScore: values.confirmQuestion2,
          algorithmDesignScore: values.algorithmDesign2,
          complexityAnalysisScore: values.complexityAnalysis2,
          codingScore: values.code2,
          testingScore: values.test2,
        },
      });

      const requestData: UpdateMockInterviewRequest = {
        mockInterviewId: values.mockInterviewId,
        startDate: dayjs(values.startDate).toISOString(),
        timeTakenInMinutes: values.timeTakenInMinutes,
        intervieweeUserId: values.interviewee,
        mockInterviewRounds,
        interviewerUserId: userId,
      };
      if (seasonId !== '') {
        requestData.seasonId = seasonId;
      }

      await updateMockInterview({ data: requestData });
      await refetchMockInterviews();
      table.setEditingRow(null);
      notifications.show({
        color: 'green',
        title: 'Success',
        message: 'Mock interview updated successfully.',
      });
    } catch (err) {
      const response = (err as any)?.response.data as UpdateMockInterviewResponseApiResponse;
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
    return dayjs(resultDate).format('YYYY-MM-DD HH:mm');
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
          valueFormat="YYYY-MM-DD HH:mm"
          minDate={daysBeforeToday(3)}
          maxDate={new Date()}
          withAsterisk
          highlightToday
          clearable
          error={form.errors.startDate}
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
        <Select
          {...form.getInputProps('interviewee')}
          mt="sm"
          label="Interviewee"
          placeholder="Enter interviewee name"
          data={usersOptions}
          filter={createOptionsFilter()}
          limit={5}
          withAsterisk
          searchable
          error={form.errors.interviewee}
        />
        <Text size="sm" mt="sm">
          Behavioural Score
        </Text>
        <Slider
          {...form.getInputProps('behaviouralScore')}
          label={(value) => value}
          min={0}
          max={10}
          step={1}
          marks={SCORE_SLIDER_MARKS}
          mb="lg"
        />
        <Fieldset legend="Leetcode Problem 1" mt="sm">
          <Select
            {...form.getInputProps('leetcodeProblem1')}
            mt="sm"
            label="Leetcode Problem"
            placeholder="Pick a leetcode problem"
            data={leetcodeProblemOptions}
            filter={createOptionsFilter({ sort: false })}
            limit={5}
            withAsterisk
            searchable
            error={form.errors.leetcodeProblem1}
          />
          <Text size="sm" mt="sm">
            Confirm Question Score
          </Text>
          <Slider
            {...form.getInputProps('confirmQuestion1')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Algorithm Design Score
          </Text>
          <Slider
            {...form.getInputProps('algorithmDesign1')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Complexity Analysis Score
          </Text>
          <Slider
            {...form.getInputProps('complexityAnalysis1')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Coding Score
          </Text>
          <Slider
            {...form.getInputProps('code1')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Testing Score
          </Text>
          <Slider
            {...form.getInputProps('test1')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
        </Fieldset>

        <Fieldset legend="Leetcode Problem 2" mt="sm">
          <Select
            {...form.getInputProps('leetcodeProblem2')}
            mt="sm"
            label="Leetcode Problem"
            placeholder="Pick a leetcode problem"
            data={leetcodeProblemOptions}
            filter={createOptionsFilter({ sort: false })}
            limit={5}
            withAsterisk
            searchable
            error={form.errors.leetcodeProblem2}
          />
          <Text size="sm" mt="sm">
            Confirm Question Score
          </Text>
          <Slider
            {...form.getInputProps('confirmQuestion2')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Algorithm Design Score
          </Text>
          <Slider
            {...form.getInputProps('algorithmDesign2')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Complexity Analysis Score
          </Text>
          <Slider
            {...form.getInputProps('complexityAnalysis2')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Coding Score
          </Text>
          <Slider
            {...form.getInputProps('code2')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
          />
          <Text size="sm" mt="sm">
            Testing Score
          </Text>
          <Slider
            {...form.getInputProps('test2')}
            label={(value) => value}
            min={0}
            max={10}
            step={1}
            marks={[
              { value: 1, label: '1' },
              { value: 2, label: '2' },
              { value: 3, label: '3' },
              { value: 4, label: '4' },
              { value: 5, label: '5' },
              { value: 6, label: '6' },
              { value: 7, label: '7' },
              { value: 8, label: '8' },
              { value: 9, label: '9' },
              { value: 10, label: '10' },
            ]}
            mb="lg"
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

type MockInterviewUpdateModalProps = {
  table: MRT_TableInstance<MockInterviewEntity>;
  updateMockInterview: UseMutateAsyncFunction<
    UpdateMockInterviewResponseApiResponse,
    unknown,
    {
      data: UpdateMockInterviewRequest;
    },
    unknown
  >;
  refetchMockInterviews: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<ListMockInterviewResponseApiResponse, unknown>>;
  row: MRT_Row<MockInterviewEntity>;
  leetcodeProblems: LeetcodeProblemDto[] | null | undefined;
  users: EnrollmentUserDto[] | null | undefined;
  seasonId: string;
};
