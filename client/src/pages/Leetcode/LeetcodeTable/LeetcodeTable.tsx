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
import {
  ActionIcon,
  Anchor,
  Button,
  Flex,
  Pill,
  Text,
  Title,
  Tooltip,
  useComputedColorScheme,
} from '@mantine/core';
import { modals } from '@mantine/modals';
import { notifications } from '@mantine/notifications';
import {
  DeleteProblemAttemptResponseApiResult,
  GetProblemAttemptsResponseApiResult,
  LeetcodeProblemDifficulty,
  ProblemAttemptEntity,
  useCreateProblemAttempt,
  useDeleteProblemAttempt,
  useGetLeetcodeProblems,
  useUpdateProblemAttempt,
} from '@/generated/api/client';
import { LeetcodeProblemDifficultyReverseIndex } from '@/shared/entities/reverseIndex';
import { LeetcodeProblemAttemptCreateModal } from './LeetcodeProblemAttemptCreateModal';
import { LeetcodeProblemAttemptUpdateModal } from './LeetcodeProblemAttemptUpdateModal';
import classes from './LeetcodeTable.module.css';

export const LeetcodeTable = ({
  refetchProblemAttempts,
  enrollmentId,
  problemAttempts,
  enableEditing,
}: LeetcodeTableProps) => {
  const computedColorScheme = useComputedColorScheme('light', { getInitialValueInEffect: true });

  const {
    data: leetcodeProblemsResponse,
    isError: isLoadingLeetcodeProblemsError,
    isFetching: isFetchingLeetcodeProblems,
    isLoading: isLoadingLeetcodeProblems,
  } = useGetLeetcodeProblems();

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
            Delete Problem Attempt
          </Title>
          <Text>
            Are you sure you want to delete this problem attempt? This action cannot be undone.
          </Text>
        </>
      ),
      labels: { confirm: 'Delete', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        try {
          await deleteProblemAttempt({
            params: { problemAttemptId: row.original.problemAttemptId },
          });
          await refetchProblemAttempts();
          modals.closeAll();
          notifications.show({
            color: 'green',
            title: 'Success',
            message: 'Problem attempt deleted successfully.',
          });
        } catch (err) {
          const response = (err as any)?.response.data as DeleteProblemAttemptResponseApiResult;
          notifications.show({
            color: 'red',
            title: 'Error',
            autoClose: false,
            message: response.error?.message,
          });
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

  const columns = useMemo<MRT_ColumnDef<ProblemAttemptEntity>[]>(
    () => [
      {
        header: 'Attempt Date',
        id: 'attemptStartDate',
        accessorFn: (row) => dayjs(row.attemptStartDateUtc).format('D MMM YYYY HH:mm'),
        Cell: ({ row }) => {
          const startFormatted = dayjs(row.original.attemptStartDateUtc).format('D MMM YYYY HH:mm');
          return startFormatted;
        },
      },
      ...(enrollmentColumn ? [enrollmentColumn] : []),
      {
        header: 'Time Taken (mins)',
        accessorFn: (row) => row.timeTakenInMinutes,
      },
      {
        header: 'Title',
        accessorFn: (row) => row.leetcodeProblem?.problem?.title,
        Cell: ({ row }) => (
          <Anchor
            href={row.original.leetcodeProblem?.problem?.link}
            target="_blank"
            inherit
            c={computedColorScheme === 'light' ? 'dark' : 'white'}
            underline="always"
          >
            {row.original.leetcodeProblem?.problem?.title}
          </Anchor>
        ),
      },
      {
        header: 'Difficulty',
        accessorFn: (row) => {
          const difficulty = row.leetcodeProblem?.leetcodeProblemDifficulty;
          return difficulty != null ? LeetcodeProblemDifficultyReverseIndex[difficulty] : '';
        },
        Cell: ({ row }) => {
          const difficulty = row.original.leetcodeProblem?.leetcodeProblemDifficulty;

          let textClass = '';
          switch (difficulty) {
            case LeetcodeProblemDifficulty.Easy:
              textClass = classes.textGreen;
              break;
            case LeetcodeProblemDifficulty.Medium:
              textClass = classes.textYellow;
              break;
            case LeetcodeProblemDifficulty.Hard:
              textClass = classes.textRed;
              break;
            default:
          }

          return (
            <Text size="sm" className={textClass}>
              {difficulty != null && LeetcodeProblemDifficultyReverseIndex[difficulty]}
            </Text>
          );
        },
      },
      {
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
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: problemAttempts ?? [],
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
          id: 'attemptStartDate',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.problemAttemptId?.toString(),
    mantineToolbarAlertBannerProps: undefined,
    isMultiSortEvent: () => true,
    renderCreateRowModalContent: ({ table }) => (
      <LeetcodeProblemAttemptCreateModal
        table={table}
        leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
        enrollmentId={enrollmentId || ''}
        createProblemAttempt={createProblemAttempt}
        refetchProblemAttempts={refetchProblemAttempts}
      />
    ),
    renderEditRowModalContent: ({ table, row }) => (
      <LeetcodeProblemAttemptUpdateModal
        table={table}
        row={row}
        leetcodeProblems={leetcodeProblemsResponse?.responseBody?.leetcodeProblems}
        enrollmentId={enrollmentId || ''}
        updateProblemAttempt={updateProblemAttempt}
        refetchProblemAttempts={refetchProblemAttempts}
      />
    ),
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
        Create New Problem Attempt
      </Button>
    ),
    state: {
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
  ) => Promise<
    QueryObserverResult<GetProblemAttemptsResponseApiResult, GetProblemAttemptsResponseApiResult>
  >;
  problemAttempts: ProblemAttemptEntity[] | null | undefined;
  enrollmentId: string;
  enableEditing: boolean;
};
