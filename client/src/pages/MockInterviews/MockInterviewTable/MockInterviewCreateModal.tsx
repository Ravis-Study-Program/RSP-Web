import dayjs from 'dayjs';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_TableInstance } from 'mantine-react-table';
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
  CreateMockInterviewRequest,
  CreateMockInterviewResponseApiResponse,
  EnrollmentUserDto,
  LeetcodeProblemDto,
  ListMockInterviewResponseApiResponse,
  MockInterviewEntity,
  MockInterviewRoundDto,
  useGetCurrentUser,
} from '@/generated/api/client';
import { CustomRichTextEditor } from '@/shared/components/RichTextEditor';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const scoreSliderMarks = Array.from({ length: 10 }, (_, i) => ({
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
  notes: z.string().optional(),
});

export const MockInterviewCreateModal = ({
  table,
  createMockInterview,
  refetchMockInterviews,
  leetcodeProblems,
  seasonId,
  users,
}: MockInterviewCreateModalProps) => {
  const { data: currentUserResponse } = useGetCurrentUser();
  const userId = currentUserResponse?.responseBody?.user.userId ?? '';

  const form = useForm({
    mode: 'uncontrolled',
    initialValues: {
      startDate: dayjs().format('YYYY-MM-DD HH:mm'),
      timeTakenInMinutes: 0,
      interviewee: '',
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
      notes: '',
    },
    validate: zodResolver(schema),
  });

  const handleSubmit = async (values: {
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
    notes?: string;
  }) => {
    try {
      const mockInterviewRounds: MockInterviewRoundDto[] = [];
      // Behavioural
      mockInterviewRounds.push({
        behaviouralMockInterviewRound: {
          behavioralScore: values.behaviouralScore,
        },
      });
      // Leetcode Problem 1
      mockInterviewRounds.push({
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
      mockInterviewRounds.push({
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
        startDate: dayjs(values.startDate).toISOString(),
        timeTakenInMinutes: values.timeTakenInMinutes,
        intervieweeUserId: values.interviewee,
        mockInterviewRounds,
        interviewerUserId: userId,
        notes: values.notes,
      };
      if (seasonId != null) {
        requestData.seasonId = seasonId;
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
      const response = (err as any)?.response.data as CreateMockInterviewResponseApiResponse;
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
          marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
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
            marks={scoreSliderMarks}
            mb="lg"
          />
        </Fieldset>

        <CustomRichTextEditor
          content={form.values.notes}
          onChange={(value) => form.setFieldValue('notes', value || '')}
          label="Interviewer Notes"
          error={form.errors.notes?.toString()}
          maxLength={5000}
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

type MockInterviewCreateModalProps = {
  table: MRT_TableInstance<MockInterviewEntity>;
  createMockInterview: UseMutateAsyncFunction<
    CreateMockInterviewResponseApiResponse,
    unknown,
    {
      data: CreateMockInterviewRequest;
    },
    unknown
  >;
  refetchMockInterviews: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<ListMockInterviewResponseApiResponse, unknown>>;
  leetcodeProblems: LeetcodeProblemDto[] | null | undefined;
  users: EnrollmentUserDto[] | null | undefined;
  seasonId: string | null;
};
