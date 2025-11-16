import { useEffect, useState } from 'react';
import { QueryObserverResult, RefetchOptions, UseMutateAsyncFunction } from '@tanstack/react-query';
import { zodResolver } from 'mantine-form-zod-resolver';
import { MRT_Row, MRT_TableInstance } from 'mantine-react-table';
import { z } from 'zod';
import {
  Button,
  Fieldset,
  Flex,
  SegmentedControl,
  Select,
  Slider,
  Stack,
  Text,
  TextInput,
  Title,
} from '@mantine/core';
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
import { CustomRichTextEditor } from '@/shared/components/RichTextEditor';
import { createOptionsFilter } from '@/shared/table/globalFilters';

const scoreSliderMarks = Array.from({ length: 11 }, (_, i) => ({
  value: i,
  label: String(i),
}));

const scoreSchema = z
  .number()
  .min(0, { message: 'The minimum score is 0' })
  .max(10, { message: 'The maximum score is 10' });

const baseSchema = z.object({
  mockInterviewId: z.string(),
  interviewee: z.string().min(1),
  behaviouralScore: scoreSchema,
  notes: z.string().optional(),
});

const leetcodeSchema = baseSchema.extend({
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

const customSchema = baseSchema.extend({
  customContent: z.string().optional(),
  customLink: z.string().url({ message: 'Please enter a valid URL' }).optional().or(z.literal('')),
  customScore: scoreSchema,
});

export const MockInterviewUpdateModal = ({
  table,
  row: { original: mockInterview },
  updateMockInterview,
  refetchMockInterviews,
  leetcodeProblems,
  users,
}: MockInterviewUpdateModalProps) => {
  const { data: currentUserResponse } = useGetCurrentUser();
  const userId = currentUserResponse?.responseBody?.user.userId ?? '';

  // Determine interview type based on existing rounds
  const rounds = mockInterview.mockInterviewRounds || [];
  const behavioural = rounds.filter((m) => m.behaviouralMockInterviewRound != null);
  const leetcodeRounds = rounds.filter((m) => m.leetcodeMockInterviewRound != null);
  const customRounds = rounds.filter((m) => m.customMockInterviewRound != null);

  const initialInterviewType: 'leetcode' | 'custom' =
    customRounds.length > 0 ? 'custom' : 'leetcode';
  const [interviewType, setInterviewType] = useState<'leetcode' | 'custom'>(initialInterviewType);

  const behaviouralRound = behavioural[0]?.behaviouralMockInterviewRound;
  const leetcodeRound1 = leetcodeRounds[0]?.leetcodeMockInterviewRound;
  const leetcodeRound2 = leetcodeRounds[1]?.leetcodeMockInterviewRound;
  const customRound = customRounds[0]?.customMockInterviewRound;

  const getInitialValues = () => {
    const baseValues = {
      mockInterviewId: mockInterview.mockInterviewId,
      interviewee: mockInterview.intervieweeUserId ?? '',
      behaviouralScore: behaviouralRound?.behavioralScore ?? 0,
      notes: mockInterview.notes ?? '',
    };

    if (interviewType === 'leetcode') {
      return {
        ...baseValues,
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
      };
    }

    return {
      ...baseValues,
      customContent: customRound?.content ?? '',
      customLink: customRound?.link ?? '',
      customScore: customRound?.score ?? 0,
    };
  };

  const form = useForm({
    mode: 'uncontrolled',
    initialValues: getInitialValues(),
    validate: zodResolver(interviewType === 'leetcode' ? leetcodeSchema : customSchema),
  });

  useEffect(() => {
    form.clearErrors();
    form.setValues(getInitialValues());
  }, [interviewType]);

  const handleSubmit = async (values: any) => {
    try {
      const mockInterviewRounds: MockInterviewRoundDto[] = [];

      // Behavioural round (always included)
      mockInterviewRounds.push({
        mockInterviewRoundId: behavioural[0]?.mockInterviewRoundId,
        behaviouralMockInterviewRound: {
          behavioralScore: values.behaviouralScore,
        },
      });

      if (interviewType === 'leetcode') {
        // Leetcode Problem 1
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
        // Leetcode Problem 2
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
      } else {
        // Custom problem
        const customRound: MockInterviewRoundDto = {
          customMockInterviewRound: {
            content: values.customContent,
            link: values.customLink,
            score: values.customScore,
          },
        };

        // Only include ID if there's an existing custom round
        if (customRounds[0]?.mockInterviewRoundId) {
          customRound.mockInterviewRoundId = customRounds[0].mockInterviewRoundId;
        }

        mockInterviewRounds.push(customRound);
      }

      const requestData: UpdateMockInterviewRequest = {
        mockInterviewId: values.mockInterviewId,
        seasonId: mockInterview.seasonId,
        intervieweeUserId: values.interviewee,
        mockInterviewRounds,
        interviewerUserId: userId,
        notes: values.notes,
      };

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

  return (
    <Stack>
      <Title order={3} mt={15}>
        Update Mock Interview
      </Title>
      <form onSubmit={form.onSubmit(handleSubmit)}>
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

        <CustomRichTextEditor
          content={form.values.notes}
          onChange={(value) => form.setFieldValue('notes', value || '')}
          label="Interviewer Notes"
          error={form.errors.notes?.toString()}
          maxLength={5000}
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

        <SegmentedControl
          value={interviewType}
          onChange={(value) => {
            const newType = value as 'leetcode' | 'custom';
            setInterviewType(newType);

            // Reset form with new type's initial values while preserving common fields
            const baseValues = {
              mockInterviewId: form.values.mockInterviewId,
              interviewee: form.values.interviewee,
              behaviouralScore: form.values.behaviouralScore,
              notes: form.values.notes,
            };

            if (newType === 'leetcode') {
              form.setValues({
                ...baseValues,
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
              });
            } else {
              form.setValues({
                ...baseValues,
                customContent: customRound?.content ?? '',
                customLink: customRound?.link ?? '',
                customScore: customRound?.score ?? 0,
              });
            }
          }}
          data={[
            { label: 'Standard Leetcode Mock', value: 'leetcode' },
            { label: 'Custom Mock', value: 'custom' },
          ]}
          mt="md"
        />

        {interviewType === 'leetcode' ? (
          <>
            <Fieldset mt="sm">
              <Select
                {...form.getInputProps('leetcodeProblem1')}
                mt="sm"
                label="Leetcode Problem 1"
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
              <Select
                {...form.getInputProps('leetcodeProblem2')}
                mt="sm"
                label="Leetcode Problem 2"
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
          </>
        ) : (
          <Fieldset mt="sm">
            <CustomRichTextEditor
              content={(form.values as any).customContent || ''}
              onChange={(value) => form.setFieldValue('customContent', value || '')}
              label="Problem Content"
              error={form.errors.customContent?.toString()}
              maxLength={10000}
            />
            <TextInput
              {...form.getInputProps('customLink')}
              mt="sm"
              label="Problem Link"
              placeholder="https://example.com/problem-link"
              error={form.errors.customLink}
            />
            <Text size="sm" mt="sm">
              Custom Problem Score
            </Text>
            <Slider
              {...form.getInputProps('customScore')}
              label={(value) => value}
              min={0}
              max={10}
              step={1}
              marks={scoreSliderMarks}
              mb="lg"
            />
          </Fieldset>
        )}

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
};
