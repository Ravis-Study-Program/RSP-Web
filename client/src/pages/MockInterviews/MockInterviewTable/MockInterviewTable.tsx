import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { IconEdit, IconTrash } from '@tabler/icons-react';
import { QueryObserverResult, RefetchOptions } from '@tanstack/react-query';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { Link } from 'react-router-dom';
import { ActionIcon, Anchor, Box, Button, Flex, Table, Text, Title, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  DeleteMockInterviewResponseApiResponse,
  ListMockInterviewResponseApiResponse,
  MockInterviewEntity,
  MockInterviewRoundEntity,
  useCreateMockInterview,
  useDeleteMockInterview,
  useGetCurrentUser,
  useGetEnrollmentUsers,
  useListLeetcodeProblems,
  useUpdateMockInterview,
} from '@/generated/api/client';
import {
  getConfirmModalProps,
  getErrorNotification,
  getMantineTablePropsWithBanner,
  getSuccessNotification,
  NOTIFICATION_MESSAGES,
} from '@/shared/constants/mantineTableProps';
import { CONFIRMATION_MESSAGES } from '@/shared/constants/messages';
import classes from '@/shared/styles/tableStyles.module.css';
import { MockInterviewScoreColors } from '@/shared/utils/colorMappings';
import { MockInterviewCreateModal } from './MockInterviewCreateModal';
import { MockInterviewUpdateModal } from './MockInterviewUpdateModal';

export const MockInterviewTable = ({
  refetchMockInterviews,
  seasonId,
  mockInterviews,
  enableEditing,
}: MockInterviewTableProps) => {
  const [pageSize, setPageSize] = useLocalStorage({
    key: 'page-size',
    defaultValue: 10,
    getInitialValueInEffect: false,
  });

  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize,
  });

  const {
    data: leetcodeProblemsResponse,
    isError: isLoadingLeetcodeProblemsError,
    isFetching: isFetchingLeetcodeProblems,
    isLoading: isLoadingLeetcodeProblems,
  } = useListLeetcodeProblems();

  const {
    data: usersResponse,
    isError: isLoadingUsersError,
    isFetching: isFetchingUsers,
    isLoading: isLoadingUsers,
  } = useGetEnrollmentUsers();
  const { data: currentUserResponse } = useGetCurrentUser();
  const currentUserId = currentUserResponse?.responseBody?.user.userId ?? '';
  const users = usersResponse?.responseBody?.enrollmentUsers.filter(
    (u) => u.userId !== currentUserId
  );

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
            {CONFIRMATION_MESSAGES.MOCK_INTERVIEW.DELETE_TITLE}
          </Title>
          <Text>{CONFIRMATION_MESSAGES.MOCK_INTERVIEW.DELETE_TEXT}</Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      ...getConfirmModalProps(),
      onConfirm: async () => {
        try {
          await deleteMockInterview({
            data: { mockInterviewId: row.original.mockInterviewId, userId: currentUserId },
          });
          await refetchMockInterviews();
          modals.closeAll();
          notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.MOCK_INTERVIEW.DELETED));
        } catch (err) {
          const response = (err as any)?.response.data as DeleteMockInterviewResponseApiResponse;
          notifications.show(getErrorNotification(response.error?.message));
        }
      },
    });
  };

  const enrollmentColumn: MRT_ColumnDef<MockInterviewEntity> | null =
    seasonId === null || seasonId === ''
      ? {
          header: 'Season',
          accessorFn: (row) => row.season?.slug || 'No Season',
        }
      : null;

  const seasonWeekColumn: MRT_ColumnDef<MockInterviewEntity> | null =
    seasonId != null && seasonId !== ''
      ? {
          header: 'Season Week',
          accessorFn: (row) => row.seasonWeek?.weekNumber || 'No Season week',
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
      ...(seasonWeekColumn ? [seasonWeekColumn] : []),
      {
        header: 'Time Taken (mins)',
        accessorFn: (row) => row.timeTakenInMinutes,
      },
      {
        header: 'Interviewer',
        accessorFn: (row) => row.interviewer?.name || 'Error',
      },
      {
        header: 'Interviewee',
        accessorFn: (row) => (row.interviewee?.name ? `${row.interviewee.name}` : 'Error'),
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
    ...getMantineTablePropsWithBanner(
      classes.tableNoHover,
      isLoadingLeetcodeProblemsError || isLoadingUsersError
    ),
    createDisplayMode: 'modal',
    onPaginationChange: (updater) => {
      const next = typeof updater === 'function' ? updater(pagination) : updater;
      setPageSize(next.pageSize);
      setPagination(next);
    },
    editDisplayMode: 'modal',
    enableEditing,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'startDate',
          desc: false,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.mockInterviewId?.toString(),
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) =>
      enableEditing && (
        <MockInterviewCreateModal
          table={table}
          users={users}
          leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
          seasonId={seasonId}
          createMockInterview={createMockInterview}
          refetchMockInterviews={refetchMockInterviews}
        />
      ),
    renderEditRowModalContent: ({ table, row }) =>
      enableEditing && (
        <MockInterviewUpdateModal
          table={table}
          row={row}
          users={users}
          leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
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
    renderRowActions: ({ row, table }) =>
      enableEditing && (
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
    renderTopToolbarCustomActions: ({ table }) =>
      enableEditing && (
        <Button
          onClick={() => {
            table.setCreatingRow(true);
          }}
        >
          Create Mock Interview
        </Button>
      ),
    state: {
      pagination,
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
  ) => Promise<QueryObserverResult<ListMockInterviewResponseApiResponse, unknown>>;
  mockInterviews: MockInterviewEntity[] | null | undefined;
  seasonId: string;
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
          <Anchor component={Link} size="sm" fw={500} to={customMock.link}>
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
  return (
    <Flex align="center" gap={6}>
      <Box bg={MockInterviewScoreColors[score]} w={12} h={12} className={classes.scoreTextCircle} />
      <Text size="sm">{score}</Text>
    </Flex>
  );
};

type ScoreTextProps = {
  score: number;
};
