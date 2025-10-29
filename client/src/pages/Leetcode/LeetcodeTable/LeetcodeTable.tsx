import dayjs from 'dayjs';
import { useMemo, useState } from 'react';
import { IconEdit, IconExternalLink, IconInfoCircle, IconTrash } from '@tabler/icons-react';
import { QueryObserverResult, RefetchOptions } from '@tanstack/react-query';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Anchor, Button, Flex, Pill, Text, Title, Tooltip } from '@mantine/core';
import { useLocalStorage } from '@mantine/hooks';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  DeleteProblemAttemptResponseApiResponse,
  ListProblemAttemptResponseApiResponse,
  ProblemAttemptEntity,
  useCreateProblemAttempt,
  useDeleteProblemAttempt,
  useGetCurrentUser,
  useListLeetcodeProblems,
  useUpdateProblemAttempt,
} from '@/generated/api/client';
import { LeetcodeDifficultyText } from '@/shared/components/LeetcodeDifficultyText';
import {
  getConfirmModalProps,
  getErrorNotification,
  getMantineTablePropsWithBanner,
  getSuccessNotification,
  NOTIFICATION_MESSAGES,
} from '@/shared/constants/mantineTableProps';
import { CONFIRMATION_MESSAGES } from '@/shared/constants/messages';
import { LeetcodeProblemDifficultyReverseIndex } from '@/shared/entities/reverseIndex';
import classes from '@/shared/styles/tableStyles.module.css';
import { LeetcodeProblemAttemptCreateModal } from './LeetcodeProblemAttemptCreateModal';
import { LeetcodeProblemAttemptUpdateModal } from './LeetcodeProblemAttemptUpdateModal';

export const LeetcodeTable = ({
  refetchProblemAttempts,
  enrollmentId,
  problemAttempts,
  enableEditing,
  showAuthor,
  showCategory = true,
}: LeetcodeTableProps) => {
  const [pageSize, setPageSize] = useLocalStorage({
    key: 'page-size',
    defaultValue: 10,
    getInitialValueInEffect: false,
  });

  const [pagination, setPagination] = useState({
    pageIndex: 0,
    pageSize,
  });

  const { data: userResponse } = useGetCurrentUser();
  const userId = userResponse?.responseBody?.user.userId ?? '';

  const {
    data: leetcodeProblemsResponse,
    isError: isLoadingLeetcodeProblemsError,
    isFetching: isFetchingLeetcodeProblems,
    isLoading: isLoadingLeetcodeProblems,
  } = useListLeetcodeProblems();

  const { mutateAsync: createProblemAttempt, status: isCreatingProblemAttemptStatus } =
    useCreateProblemAttempt();
  const { mutateAsync: updateProblemAttempt, status: isUpdatingProblemAttemptStatus } =
    useUpdateProblemAttempt();
  const { mutateAsync: deleteProblemAttempt, status: isDeletingProblemAttemptStatus } =
    useDeleteProblemAttempt();

  const openDeleteConfirmModal = (row: MRT_Row<ProblemAttemptEntity>) => {
    modals.openConfirmModal({
      children: (
        <>
          <Title order={3} mt={15} mb={10}>
            {CONFIRMATION_MESSAGES.PROBLEM_ATTEMPT.DELETE_TITLE}
          </Title>
          <Text>{CONFIRMATION_MESSAGES.PROBLEM_ATTEMPT.DELETE_TEXT}</Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      ...getConfirmModalProps(),
      onConfirm: async () => {
        try {
          await deleteProblemAttempt({
            data: { problemAttemptId: row.original.problemAttemptId, userId },
          });
          await refetchProblemAttempts();
          modals.closeAll();
          notifications.show(getSuccessNotification(NOTIFICATION_MESSAGES.PROBLEM_ATTEMPT.DELETED));
        } catch (err) {
          const response = (err as any)?.response.data as DeleteProblemAttemptResponseApiResponse;
          notifications.show(getErrorNotification(response.error?.message));
        }
      },
    });
  };

  const enrollmentColumn: MRT_ColumnDef<ProblemAttemptEntity> | null =
    enrollmentId === null || enrollmentId === ''
      ? {
          header: 'Season',
          accessorFn: (row) => row.enrollment?.season?.slug || 'No Season',
        }
      : null;

  const seasonWeekColumn: MRT_ColumnDef<ProblemAttemptEntity> | null =
    enrollmentId != null && enrollmentId !== ''
      ? {
          header: 'Season Week',
          accessorFn: (row) => row.seasonWeek?.weekNumber || 'No Season week',
        }
      : null;

  const authorColumn: MRT_ColumnDef<ProblemAttemptEntity> | null = showAuthor
    ? {
        header: 'Author',
        accessorFn: (row) => row.enrollment?.user?.name || 'No User',
      }
    : null;

  const categoryColumn: MRT_ColumnDef<ProblemAttemptEntity> | null = showCategory
    ? {
        header: 'Category',
        accessorFn: (row) =>
          row.leetcodeProblem?.leetcodeProblemCategories
            ?.map((category) => category.name)
            .join(' ') || '',
        Cell: ({ row }) => {
          return (
            <Flex className={classes.categoryContainer}>
              {row.original.leetcodeProblem?.leetcodeProblemCategories?.map((category, index) => (
                <Pill size="sm" key={index} className={classes.category}>
                  {category.name}
                </Pill>
              ))}
            </Flex>
          );
        },
      }
    : null;

  const columns = useMemo<MRT_ColumnDef<ProblemAttemptEntity>[]>(
    () => [
      {
        header: 'Attempt Date',
        id: 'attemptStartDate',
        accessorFn: (row) => row.attemptStartDateUtc,
        Cell: ({ row }) => {
          const startFormatted = dayjs(row.original.attemptStartDateUtc).format('D MMM YYYY HH:mm');
          const isDataBackFilled = row.original.enrollment?.season?.isDataBackFilled;

          return (
            <Flex align="center" gap="xs">
              <Text size="sm">{startFormatted}</Text>
              {isDataBackFilled && (
                <Tooltip
                  label="This data was backfilled based on historical records"
                  position="top"
                >
                  <IconInfoCircle size={14} style={{ color: 'var(--mantine-color-blue-6)' }} />
                </Tooltip>
              )}
            </Flex>
          );
        },
      },
      ...(enrollmentColumn ? [enrollmentColumn] : []),
      ...(seasonWeekColumn ? [seasonWeekColumn] : []),
      ...(authorColumn ? [authorColumn] : []),
      {
        header: 'Duration',
        accessorFn: (row) => `${row.timeTakenInMinutes} mins`,
      },
      {
        header: 'Title',
        accessorFn: (row) => row.leetcodeProblem?.problem?.title,
        Cell: ({ row }) => (
          <Anchor
            href={row.original.leetcodeProblem?.problem?.link}
            target="_blank"
            inherit
            className={classes.title}
            underline="always"
          >
            <Flex align="center" gap="xs">
              <Text size="sm">{row.original.leetcodeProblem?.problem?.title}</Text>
              <IconExternalLink size={12} style={{ flexShrink: 0, opacity: 0.7 }} />
            </Flex>
          </Anchor>
        ),
      },
      {
        header: 'Difficulty',
        accessorFn: (row) => {
          const difficulty = row.leetcodeProblem?.leetcodeProblemDifficulty;
          return difficulty != null ? LeetcodeProblemDifficultyReverseIndex[difficulty] : '';
        },
        Cell: ({ row }) => (
          <LeetcodeDifficultyText
            difficulty={row.original.leetcodeProblem?.leetcodeProblemDifficulty}
          />
        ),
      },
      ...(categoryColumn ? [categoryColumn] : []),
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: problemAttempts ?? [],
    ...getMantineTablePropsWithBanner(classes.table, isLoadingLeetcodeProblemsError),
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
          id: 'attemptStartDate',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.problemAttemptId?.toString(),
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) =>
      enableEditing && (
        <LeetcodeProblemAttemptCreateModal
          table={table}
          leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
          enrollmentId={enrollmentId || ''}
          createProblemAttempt={createProblemAttempt}
          refetchProblemAttempts={refetchProblemAttempts}
        />
      ),
    renderEditRowModalContent: ({ table, row }) =>
      enableEditing && (
        <LeetcodeProblemAttemptUpdateModal
          table={table}
          row={row}
          leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
          updateProblemAttempt={updateProblemAttempt}
          refetchProblemAttempts={refetchProblemAttempts}
        />
      ),
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
          Create New Problem Attempt
        </Button>
      ),
    state: {
      pagination,
      isLoading: isLoadingLeetcodeProblems,
      isSaving:
        isCreatingProblemAttemptStatus === 'pending' ||
        isUpdatingProblemAttemptStatus === 'pending' ||
        isDeletingProblemAttemptStatus === 'pending',
      showAlertBanner: isLoadingLeetcodeProblemsError,
      showProgressBars: isFetchingLeetcodeProblems,
    },
  });

  return <MantineReactTable table={table} />;
};

type LeetcodeTableProps = {
  refetchProblemAttempts: (
    options?: RefetchOptions
  ) => Promise<QueryObserverResult<ListProblemAttemptResponseApiResponse, unknown>>;
  problemAttempts: ProblemAttemptEntity[] | null | undefined;
  enrollmentId: string;
  enableEditing: boolean;
  showAuthor: boolean;
  showCategory: boolean;
};
