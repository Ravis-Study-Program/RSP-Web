import { useMemo } from 'react';
import { IconTrash } from '@tabler/icons-react';
import {
  MantineReactTable,
  MRT_ColumnDef,
  MRT_Row,
  useMantineReactTable,
} from 'mantine-react-table';
import { ActionIcon, Flex, Text, Tooltip } from '@mantine/core';
import { modals } from '@mantine/modals';
import {
  MenteeResponseDto,
  useGetCurrentUserMenteesList,
  useKickStudent,
} from '@/generated/api/client';
import { useSeasonSlug } from '@/shared/hooks/useSeasonSlug';
import classes from './MenteesTable.module.css';

export const MenteesTable = () => {
  const { seasonSlug } = useSeasonSlug();
  const {
    data: menteeResponse,
    isError: isLoadingMenteesError,
    isFetching: isFetchingMentees,
    isLoading: isLoadingMentees,
    refetch: refetchMentees,
  } = useGetCurrentUserMenteesList(seasonSlug);
  const { mutateAsync: kickStudent, status: isKickingStudentStatus } = useKickStudent();

  const openKickMenteeConfirmModal = (row: MRT_Row<MenteeResponseDto>) => {
    modals.openConfirmModal({
      title: 'Delete Mentee',
      children: (
        <Text>
          Are you sure you want to kick this mentee out of RSP? This action cannot be undone.
        </Text>
      ),
      labels: { confirm: 'Kick Mentee', cancel: 'Cancel' },
      confirmProps: { color: 'red' },
      onConfirm: async () => {
        await kickStudent({
          data: {
            seasonSlug,
            menteeEnrollmentId: row.original.menteeEnrollmentId,
          },
        });
        await refetchMentees();
        modals.closeAll();
      },
    });
  };

  const columns = useMemo<MRT_ColumnDef<MenteeResponseDto>[]>(
    () => [
      {
        accessorKey: 'menteeName',
        header: 'Name',
      },
    ],
    []
  );

  const table = useMantineReactTable({
    columns,
    data: menteeResponse?.responseBody?.mentees ?? [],
    mantinePaperProps: {
      className: classes.table,
    },
    enableEditing: true,
    initialState: {
      density: 'xs',
      sorting: [
        {
          id: 'menteeName',
          desc: true,
        },
      ],
    },
    positionActionsColumn: 'last',
    getRowId: (row) => row.menteeEnrollmentId?.toString(),
    mantineToolbarAlertBannerProps: isLoadingMenteesError
      ? {
          color: 'red',
          children: 'Error loading data',
        }
      : undefined,
    isMultiSortEvent: () => true,
    renderRowActions: ({ row }) => (
      <Flex gap="md">
        <Tooltip label="Delete">
          <ActionIcon variant="subtle" color="red" onClick={() => openKickMenteeConfirmModal(row)}>
            <IconTrash />
          </ActionIcon>
        </Tooltip>
      </Flex>
    ),
    state: {
      isLoading: isLoadingMentees,
      isSaving: isKickingStudentStatus === 'pending',
      showAlertBanner: isLoadingMenteesError,
      showProgressBars: isFetchingMentees,
    },
  });

  return <MantineReactTable table={table} />;
};
