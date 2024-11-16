import dayjs from 'dayjs';
import { useMemo } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import { QueryObserverResult, RefetchOptions } from '@tanstack/react-query';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Anchor, Box, Button, Flex, Table, Text, Title, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import {
  GetMockInterviewsResponseApiResult,
  MockInterviewEntity,
  MockInterviewRoundEntity,
  useCreateMockInterview,
  useDeleteMockInterview,
  useGetLeetcodeProblems,
  useGetUserList,
  useUpdateMockInterview,
} from '@/generated/api/client';
import { MockInterviewCreateModal } from './MockInterviewCreateModal';
import { MockInterviewUpdateModal } from './MockInterviewUpdateModal';
import classes from './MockInterviewTable.module.css';

export const MockInterviewTable = ({
  refetchMockInterviews,
  enrollmentId,
  mockInterviews,
  enableEditing,
}: MockInterviewTableProps) => {
  const {
    data: leetcodeProblemsResponse,
    isError: isLoadingLeetcodeProblemsError,
    isFetching: isFetchingLeetcodeProblems,
    isLoading: isLoadingLeetcodeProblems,
  } = useGetLeetcodeProblems();

  const {
    data: usersResponse,
    isError: isLoadingUsersError,
    isFetching: isFetchingUsers,
    isLoading: isLoadingUsers,
  } = useGetUserList();

  const { mutateAsync: createMockInterview, status: isCreatingMockInterviewStatus } =
    useCreateMockInterview();
  const { mutateAsync: updateMockInterview, status: isUpdatingMockInterviewStatus } =
    useUpdateMockInterview();
  const { mutateAsync: deleteMockInterview, status: isDeletingMockInterviewStatus } =
    useDeleteMockInterview();

  const openDeleteConfirmModal = (row: MRT_Row<MockInterviewEntity>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            Delete Mock Interview
          </Title>
          <Text>
            Are you sure you want to kick this mock interview out of RSP? This action cannot be
            undone.
          </Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await deleteMockInterview({ params: { mockInterviewId: row.original.mockInterviewId } });
        await refetchMockInterviews();
        modals.closeAll();
      },
    });
  };

  const enrollmentColumn: MRT_ColumnDef<MockInterviewEntity> | null =
    enrollmentId === null || enrollmentId === ''
      ? {
          header: 'Season',
          accessorFn: (row) => row.enrollment?.season?.slug || 'No Season',
        }
      : null;

  const columns = useMemo<MRT_ColumnDef<MockInterviewEntity>[]>(
    () => [
      {
        header: 'Date',
        id: 'startDate',
        accessorFn: (row) => dayjs(row.startDate).format('D MMM YYYY HH:mm'),
        Cell: ({ row }) => {
          const startFormatted = dayjs(row.original.startDate).format('D MMM YYYY HH:mm');
          return <Text size="sm">{startFormatted}</Text>;
        },
      },
      ...(enrollmentColumn ? [enrollmentColumn] : []),
      {
        header: 'Time Taken (mins)',
        accessorFn: (row) => row.timeTakenInMinutes,
      },
      {
        header: 'Interviewer',
        accessorFn: (row) => row.interviewer?.name || 'Error',
      },
      {
        header: 'Result',
        accessorFn: (row) => (row.isPass ? 'Pass' : 'Fail'),
        Cell: ({ row }) => {
          return (
            <Text size="sm" fw={500} c={row.original.isPass ? 'green.8' : 'red.8'}>
              {row.original.isPass ? 'Pass' : 'Fail'}
            </Text>
          );
        },
      },
      {
        header: 'Behavioural',
        accessorFn: (row) => {
          const rounds = row.mockInterviewRounds || [];
          const behaviouralRound = rounds.filter((r) => r.behaviouralMockInterviewRound != null)[0];
          return behaviouralRound?.behaviouralMockInterviewRound?.behavioralScore || 0;
        },
        Cell: ({ row }) => {
          const rounds = row.original.mockInterviewRounds || [];
          const behaviouralRound = rounds.filter((r) => r.behaviouralMockInterviewRound != null)[0];
          const score = behaviouralRound?.behaviouralMockInterviewRound?.behavioralScore || 0;
          return <ScoreText score={score} />;
        },
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: mockInterviews ?? [],
    mantinePaperProps: {
      className: classes.table,
    },
    createDisplayMode: 'modal',
    mantineCreateRowModalProps: {
      closeOnClickOutside: false,
      withCloseButton: true,
      closeButtonProps: {
        className: classes.modalCloseButton,
      },
    },
    mantineEditRowModalProps: {
      closeOnClickOutside: false,
      withCloseButton: true,
      closeButtonProps: {
        className: classes.modalCloseButton,
      },
    },
    editDisplayMode: 'modal',
    enableEditing,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'startDate',
          desc: true,
        },
      ],
      expanded: true,
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.mockInterviewId?.toString(),
    mantineToolbarAlertBannerProps: undefined,
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <MockInterviewCreateModal
        table={table}
        users={usersResponse?.responseBody?.users}
        leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
        enrollmentId={enrollmentId || ''}
        createMockInterview={createMockInterview}
        refetchMockInterviews={refetchMockInterviews}
      />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <MockInterviewUpdateModal
        table={table}
        row={row}
        users={usersResponse?.responseBody?.users}
        leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
        enrollmentId={enrollmentId || ''}
        updateMockInterview={updateMockInterview}
        refetchMockInterviews={refetchMockInterviews}
      />
    ),
    renderDetailPanel: ({ row }) => {
      const rounds = row.original.mockInterviewRounds || [];
      const leetcodeRounds = rounds.filter((r) => r.leetcodeMockInterviewRound != null);
      const customRounds = rounds.filter((r) => r.customMockInterviewRound != null);

      return (
        <Flex gap={20} direction="column">
          <LeetcodeMockInterviewRoundsInnerTable rounds={leetcodeRounds} />
          <CustomMockInterviewRoundsInnerTable rounds={customRounds} />
        </Flex>
      );
    },
    renderRowActions: ({ row, table }) => (
      <Flex gap="md">
        <Tooltip label="Edit">
          <ActionIcon variant="subtle" onClick={() => table.setEditingRow(row)}>
            <IconEdit />
          </ActionIcon>
        </Tooltip>
        <Tooltip label="Delete">
          <ActionIcon variant="subtle" color="red" onClick={() => openDeleteConfirmModal(row)}>
            <IconTrash />
          </ActionIcon>
        </Tooltip>
      </Flex>
    ),
    renderTopToolbarCustomActions: ({ table }) => (
      <Button
        onClick={() => {
          table.setCreatingRow(true);
        }}
      >
        Create Mock Interview
      </Button>
    ),
    state: {
      isLoading: isLoadingLeetcodeProblems || isLoadingUsers,
      isSaving:
        isCreatingMockInterviewStatus === 'pending' ||
        isUpdatingMockInterviewStatus === 'pending' ||
        isDeletingMockInterviewStatus === 'pending',
      showAlertBanner: isLoadingLeetcodeProblemsError || isLoadingUsersError,
      showProgressBars: isFetchingLeetcodeProblems || isFetchingUsers,
    },
  });

  return <MantineReactTable table={table} />;
};

type MockInterviewTableProps = {
  refetchMockInterviews: (
    options?: RefetchOptions
  ) => Promise<
    QueryObserverResult<GetMockInterviewsResponseApiResult, GetMockInterviewsResponseApiResult>
  >;
  mockInterviews: MockInterviewEntity[] | null | undefined;
  enrollmentId: string;
  enableEditing: boolean;
};

const LeetcodeMockInterviewRoundsInnerTable = ({ rounds }: InnerMockInterviewTableProps) => {
  const tableRows = rounds.map((r, index) => {
    if (r.leetcodeMockInterviewRound == null) {
      return null;
    }

    const leetcodeMock = r.leetcodeMockInterviewRound;

    return (
      <Table.Tr key={index}>
        <Table.Td>{leetcodeMock.leetcodeProblem?.problem?.title || ''}</Table.Td>
        <Table.Td>
          <ScoreText score={leetcodeMock.confirmQuestionScore} />
        </Table.Td>
        <Table.Td>
          <ScoreText score={leetcodeMock.algorithmDesignScore} />
        </Table.Td>
        <Table.Td>
          <ScoreText score={leetcodeMock.complexityAnalysisScore} />
        </Table.Td>
        <Table.Td>
          <ScoreText score={leetcodeMock.codingScore} />
        </Table.Td>
        <Table.Td>
          <ScoreText score={leetcodeMock.testingScore} />
        </Table.Td>
      </Table.Tr>
    );
  });

  if (rounds.length === 0) {
    return null;
  }

  return (
    <Table horizontalSpacing="md" verticalSpacing="sm" className={classes.innerTable}>
      <Table.Thead className={classes.innerTableHeading}>
        <Table.Tr>
          <Table.Th>Leetcode Problem</Table.Th>
          <Table.Th>Confirm Question</Table.Th>
          <Table.Th>Algorithm Design</Table.Th>
          <Table.Th>Complexity Analysis</Table.Th>
          <Table.Th>Code</Table.Th>
          <Table.Th>Test</Table.Th>
        </Table.Tr>
      </Table.Thead>
      <Table.Tbody>{tableRows}</Table.Tbody>
    </Table>
  );
};

const CustomMockInterviewRoundsInnerTable = ({ rounds }: InnerMockInterviewTableProps) => {
  const tableRows = rounds.map((r, index) => {
    if (r.customMockInterviewRound == null) {
      return null;
    }

    const customMock = r.customMockInterviewRound;

    return (
      <Table.Tr key={index}>
        <Table.Td>
          <Text size="sm">{customMock.content}</Text>
        </Table.Td>
        <Table.Td>
          <Anchor size="sm" fw={500} href={customMock.link}>
            Link
          </Anchor>
        </Table.Td>
      </Table.Tr>
    );
  });

  if (rounds.length === 0) {
    return null;
  }

  return (
    <Table horizontalSpacing="md" verticalSpacing="sm" className={classes.innerTable}>
      <Table.Thead className={classes.innerTableHeading}>
        <Table.Tr>
          <Table.Th>Custom Problem</Table.Th>
          <Table.Th>Link</Table.Th>
        </Table.Tr>
      </Table.Thead>
      <Table.Tbody>{tableRows}</Table.Tbody>
    </Table>
  );
};

type InnerMockInterviewTableProps = {
  rounds: MockInterviewRoundEntity[];
};

const ScoreText = ({ score }: ScoreTextProps) => {
  const colors = [
    '#E27168',
    '#E88366',
    '#EE9663',
    '#F3A961',
    '#F9BC5E',
    '#FFD05B',
    '#D9CB62',
    '#B4C569',
    '#90BF70',
    '#6EB977',
    '#4DB37F',
  ];

  return (
    <Flex align="center" gap={6}>
      <Box bg={colors[score]} w={12} h={12} className={classes.scoreTextCircle} />
      <Text size="sm">{score}</Text>
    </Flex>
  );
};

type ScoreTextProps = {
  score: number;
};
