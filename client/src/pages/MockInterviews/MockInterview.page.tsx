import { Layout } from '@/components/Layout/Layout';
import { useGetIsCurrentUserEnrolled, useListMockInterview } from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import { optionsFilter } from '@/shared/table/globalFilters';
import { Flex, Group, MultiSelect, Select } from '@mantine/core';
import { useMemo, useState } from 'react';
import { MockInterviewTable } from './MockInterviewTable/MockInterviewTable';
import classes from './MockInterview.module.css';

export default function MockInterviewPage() {
  const { seasonSlug } = useSeasonSlug();
  const { data: userResponse } = useGetIsCurrentUserEnrolled({ seasonSlug });
  const [selectedIsPassResult, setSelectedIsPassResult] = useState<boolean | null>(null);
  const [selectedIsGivenMocks, setSelectedIsGivenMocks] = useState<boolean | null>(null);
  const [selectedInterviewers, setSelectedInterviewers] = useState<string[]>([]);
  const email = userResponse?.responseBody?.email ?? '';

  const { data: mockInterviewsResponse, refetch: refetchMockInterviews } = useListMockInterview(
    {
      SeasonId: userResponse?.responseBody?.seasonId || undefined,
      IncludeCustom: true,
      IncludeLeetcode: true,
      IncludeBehavioural: true,
      Email: email,
    },
    { query: { enabled: email !== '' } }
  );

  const displayMockOptions = [
    { label: "Received Mocks", value: 'false' },
    { label: "Given Mocks", value: 'true' }
  ];

  const resultOptions = [
    { label: "Pass", value: 'true' },
    { label: "Fail", value: 'false' }
  ];

  const interviewersOptions = [
    ...((mockInterviewsResponse?.responseBody?.mockInterviews || []).map(mockInterview => {
      return {
        label: mockInterview.interviewer?.name ?? "",
        value: mockInterview.interviewer?.email ?? "",
      };
    })),
    ...((mockInterviewsResponse?.responseBody?.mockInterviews || []).map(mockInterview => {
      return {
        label: mockInterview.interviewee?.name
          ? `${mockInterview.interviewee.name}`
          : "",
        value: mockInterview.interviewee?.email ?? "",
      };
    })),
  ];
  const distinctInterviewersOptions = Array.from(
    new Map(interviewersOptions.map(option => [option.value, option])).values()
  );

  const filteredMockInterviews = useMemo(() => {
    const mocks = mockInterviewsResponse?.responseBody?.mockInterviews || [];
    return mocks.filter((mock) =>
      (selectedIsPassResult === null && selectedIsGivenMocks === null && selectedInterviewers.length === 0) ||
      (
        (selectedIsPassResult === null ||
          (mock.isPass !== null &&
           (typeof mock.isPass === 'boolean'
              ? mock.isPass
              : mock.isPass === 'true') === selectedIsPassResult)
        ) &&
        (selectedIsGivenMocks === null ||
          (
            (selectedIsGivenMocks === false &&
             mock.interviewee?.email !== null &&
             email === mock.interviewee?.email) ||
            (selectedIsGivenMocks === true &&
             mock.interviewee?.email !== null &&
             email !== mock.interviewee?.email)
          )
        ) &&
        (selectedInterviewers.length === 0 ||
          (mock.interviewer?.email != null &&
            selectedInterviewers.includes(
              mock.interviewer?.email.toString()
            )))
      )
    );
  }, [
    email,
    mockInterviewsResponse,
    selectedIsPassResult,
    selectedIsGivenMocks,
    selectedInterviewers,
  ]);

  return (
    <Layout>
      <Flex justify="space-between">
        <Group mb="lg" justify="flex-start">
        <Select
            label="Mock Options"
            data={displayMockOptions}
            placeholder="Pick value"
            filter={optionsFilter}
            miw={200}
            nothingFoundMessage="Nothing found..."
            value={selectedIsGivenMocks === null ? null : selectedIsGivenMocks.toString()}
            clearable
            onChange={(value) => {
              if (value === null || value === '') {
                setSelectedIsGivenMocks(null);
              } else {
                setSelectedIsGivenMocks(value === 'true');
              }
            }}
          />
        </Group>
        <Group mb="lg" justify="flex-end">
        <MultiSelect
            classNames={{ inputField: classes.inputField }}
            label="Interviewer"
            placeholder="Pick value(s)"
            data={distinctInterviewersOptions}
            filter={optionsFilter}
            miw={150}
            searchable
            nothingFoundMessage="Nothing found..."
            value={selectedInterviewers}
            onChange={(values) => {
              setSelectedInterviewers(values as string[]);
            }}
          />
        <Select
            label="Result"
            data={resultOptions}
            placeholder="Pick value"
            filter={optionsFilter}
            miw={150}
            nothingFoundMessage="Nothing found..."
            value={selectedIsPassResult === null ? null : selectedIsPassResult.toString()}
            clearable
            onChange={(value) => {
              if (value === null || value === '') {
                setSelectedIsPassResult(null);
              } else {
                setSelectedIsPassResult(value === 'true');
              }
            }}
          />
        </Group>
      </Flex>
      <MockInterviewTable
        refetchMockInterviews={refetchMockInterviews}
        mockInterviews={filteredMockInterviews}
        seasonId={userResponse?.responseBody?.seasonId || ''}
        enableEditing
      />
    </Layout>
  );
}
